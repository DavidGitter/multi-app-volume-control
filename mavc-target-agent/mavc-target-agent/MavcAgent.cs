using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/**
 * Background agent that bridges the hardware mixer (serial/COM) with the
 * Windows audio system.  Reads knob values, maps them to audio sessions
 * and devices, and optionally shows an on-screen overlay.
 */
class MavcAgent
{
    #region Static Fields

    // Optional console allocation (useful when the project is built as a Windows app).
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    // Main audio controller used to enumerate and control app/device sessions.
    public static AudioController audioContr = new AudioController();

    // Config location: %USERPROFILE%\Documents\MAVC\config.json
    public static string configSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
    public static string configFileName = "config.json";
    public static string configFilePath = Path.Combine(configSavePath, configFileName);

    // Watches config file changes and reloads mappings when the config is saved.
    public static FileSystemWatcher watcher;

    // Current configuration + lock (config is read/updated from multiple threads).
    private static MAVCSave mavcSave = new MAVCSave();
    private static readonly object mavcSaveLock = new object();

    // Per-knob output mappings (each knob controls a list of AudioOutput targets).
    // Each list has its own lock because updates happen independently.
    private static readonly List<AudioOutput> aoListVol1 = new List<AudioOutput>();
    private static readonly object aoList1Lock = new object();

    private static readonly List<AudioOutput> aoListVol2 = new List<AudioOutput>();
    private static readonly object aoList2Lock = new object();

    private static readonly List<AudioOutput> aoListVol3 = new List<AudioOutput>();
    private static readonly object aoList3Lock = new object();

    private static readonly List<AudioOutput> aoListVol4 = new List<AudioOutput>();
    private static readonly object aoList4Lock = new object();

    // Serial/COM connection to the hardware mixer (reconnected if needed).
    private static COM comServer = null;

    // Redirect console output if AllocConsole() is used.
    private static Stream stdOut = null;
    private static StreamWriter writer = null;

    // Optional on-screen overlay (WinForms) that shows the last knob/value.
    private static bool screenOverlayEnabled = false;
    private static Overlay overlay = null;

    #endregion

    #region Public Static Methods

    /**
     * Interprets a word received from the hardware mixer and dispatches
     * the corresponding volume change to the correct knob handler.
     *
     * @param word  the word to be interpreted (see COM class)
     */
    public static void interpretWord(COM.Word word)
    {
        char action = word.action;
        string arg = word.args;

        // Optional knob order reversal (swap A<->D and B<->C).
        if (mavcSave.reverseKnobOrder)
        {
            action = action switch
            {
                'A' => 'D',
                'B' => 'C',
                'C' => 'B',
                'D' => 'A',
                _ => action
            };
        }

        // Dispatch to the correct knob handler.
        switch (action)
        {
            case 'A': HandleKnob(1, arg, mavcSave.reverseKnob1, aoListVol1); break;
            case 'B': HandleKnob(2, arg, mavcSave.reverseKnob2, aoListVol2); break;
            case 'C': HandleKnob(3, arg, mavcSave.reverseKnob3, aoListVol3); break;
            case 'D': HandleKnob(4, arg, mavcSave.reverseKnob4, aoListVol4); break;
            default: throw new InvalidDataException();
        }
    }

    // Sets up FileSystemWatcher to reload config when config.json changes.
    private static readonly object confDebounceLock = new object();
    private static System.Threading.Timer confDebounceTimer;

    /**
     * Initializes a FileSystemWatcher that reloads the configuration
     * whenever config.json is modified on disk.
     */
    public static void SetupConfUpdater()
    {
        watcher = new FileSystemWatcher
        {
            Path = configSavePath,
            Filter = configFileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        watcher.Changed += (s, e) =>
        {
            lock (confDebounceLock)
            {
                confDebounceTimer?.Dispose();
                confDebounceTimer = new System.Threading.Timer(_ =>
                {
                    try { UpdateMAVCSave(); }
                    catch { /* log */ }
                }, null, 50, Timeout.Infinite); // 50ms: tweak
            }
        };
    }

    /**
     * Reads config.json from disk and refreshes the audio output mappings.
     * If only the overlay position changed, the heavy rebuild is skipped.
     */
    public static void UpdateMAVCSave()
    {
        MAVCSave loaded;

        lock (mavcSaveLock)
        {
            string json = File.ReadAllText(configFilePath);
            loaded = JsonConvert.DeserializeObject<MAVCSave>(json) ?? new MAVCSave();

            bool onlyOverlayChanged =
                loaded.overlayX != mavcSave.overlayX ||
                loaded.overlayY != mavcSave.overlayY;

            mavcSave = loaded;

            if (onlyOverlayChanged && screenOverlayEnabled && overlay != null)
            {
                overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);
                return; // skip heavy rebuild
            }
        }

        audioContr.InvalidateCache();
        UpdateAllAOs();

        if (screenOverlayEnabled && overlay != null)
            overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);
    }


    /** Rebuilds all knob-to-AudioOutput mappings from the current config. */
    public static void UpdateAllAOs()
    {
        // Lock config during rebuild to keep it consistent across all lists.
        lock (mavcSaveLock)
        {
            UpdateAOsList(aoListVol1, aoList1Lock, mavcSave.AOsVol1);
            UpdateAOsList(aoListVol2, aoList2Lock, mavcSave.AOsVol2);
            UpdateAOsList(aoListVol3, aoList3Lock, mavcSave.AOsVol3);
            UpdateAOsList(aoListVol4, aoList4Lock, mavcSave.AOsVol4);
        }
    }

    #endregion

    #region Private Static Methods

    /**
     * Converts the raw knob value into a 0..1 volume value (optionally reversed),
     * applies it to all mapped targets, and updates the overlay.
     *
     * @param knobIndex  knob number (1..4)
     * @param arg        raw device argument (typically "0".."100")
     * @param reverse    if true, invert the knob direction
     * @param outputs    resolved output targets controlled by this knob
     */
    private static void HandleKnob(int knobIndex, string arg, bool reverse, List<AudioOutput> outputs)
    {
        // Device sends 0..100 as text.
        float raw = int.Parse(arg);

        // Keep the exact logic you had (including the special-case raw==0 behavior).
        float value = reverse
            ? (raw > 0 ? 1f - raw / 100f : 100)
            : (raw > 0 ? raw / 100f : 0);

        Console.WriteLine($"Set Volume {knobIndex}: {value}");

        // Apply volume to all mapped targets.
        foreach (AudioOutput ao in outputs)
            ao?.SetVolume(value);

        // Best-effort overlay update (overlay object is created only if enabled at startup).
        if (screenOverlayEnabled)
            overlay.setUpdatedVolume($"Knob {knobIndex}", (int)(value * 100));
    }

    /**
     * Rebuilds one mapping list from the config.  Normal entries are resolved
     * by name; function entries create synthetic outputs (Focused, Other Apps).
     *
     * @param target      the list of AudioOutput targets to rebuild
     * @param targetLock  lock object protecting the target list
     * @param config      configured audio outputs to resolve
     */
    private static void UpdateAOsList(List<AudioOutput> target, object targetLock, IEnumerable<MAVCSave.AudioOutput> config)
    {
        lock (targetLock)
        {
            target.Clear();

            foreach (MAVCSave.AudioOutput ao in config)
            {
                if (!ao.type.Equals("Function"))
                {
                    target.AddRange(audioContr.GetOutputsByName(ao.name));
                }
                else if (ao.name.Equals("Focused"))
                {
                    target.Add(new AudioFocused(audioContr));
                }
                else if (ao.name.Equals("Other Apps"))
                {
                    target.Add(new AudioOtherApps(audioContr, mavcSave));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    /** Opens a console and redirects stdout/stderr so Console.WriteLine() is visible. */
    private static void enableDebugWindow()
    {
        if (!AllocConsole())
            return;

        stdOut = Console.OpenStandardOutput();
        writer = new StreamWriter(stdOut) { AutoFlush = true };
        Console.SetOut(writer);
        Console.SetError(writer);
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Console allocated.");
        Console.Title = "MAVC Agent Debug";
    }

    #endregion

    #region Main Method

    /**
     * Application entry point.  Loads the configuration, starts the overlay
     * and periodic-refresh tasks, then enters the main reconnect loop for
     * the hardware COM connection.
     *
     * @param args  command-line arguments (unused)
     */
    static void Main(string[] args)
    {
        // Make sure folder exists (prevents FileSystemWatcher/path issues)
        try
        {
            Directory.CreateDirectory(configSavePath);
        }
        catch { }

        bool foundFile = false;

        // Load config
        while (!foundFile)
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    UpdateMAVCSave();          // IMPORTANT: this must use DeserializeObject<MAVCSave>(...)
                    SetupConfUpdater();
                    foundFile = true;

                    if (mavcSave.enableDebugMode)
                        enableDebugWindow();   // AllocConsole BEFORE any Console.WriteLine you care about
                }
            }
            catch
            {
                Thread.Sleep(5000);
            }
        }

        Console.WriteLine("Started Mavc Debug-Console");

        Log logger = new Log(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "MAVC",
            "agent-log.txt"
        ));

        // If the audio system reports a new output/session, refresh mappings and request current hardware volumes.
        audioContr.onOutputAddedCallback((sender, newSession) =>
        {
            Console.WriteLine("new audio output found!");
            logger.Info("A new output was found and added to the agent.");
            audioContr.InvalidateCache();
            lock (mavcSaveLock) { UpdateAllAOs(); }
            comServer?.updateVolumes();
        });

        // Periodically refresh mappings in case sessions change without triggering the callback.
        Task intervalUpdater = new Task(() =>
        {
            while (true)
            {
                lock (mavcSaveLock) { UpdateAllAOs(); }
                Thread.Sleep(10_000);
            }
        });
        intervalUpdater.Start();

        // Overlay
        screenOverlayEnabled = mavcSave.enableScreenOverlay;
        Console.WriteLine("overlay enabled: " + screenOverlayEnabled);

        if (screenOverlayEnabled)
        {
            Task ui = new Task(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                overlay = new Overlay(mavcSave.autoHideAfterSec);
                overlay.SetAutoHideActive(mavcSave.activateAutoHide);
                overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);

                Application.Run(overlay);
            });
            ui.Start();
        }

        // Main reconnect loop for the hardware COM connection.
        while (true)
        {
            try
            {
                if (comServer == null || !comServer.IsOpen())
                {
                    Console.WriteLine("Waiting for hardware to connect (COM3, 9600).");
                    comServer = new COM("COM3", 9600);
                    Console.WriteLine("Hardware connected.");

                    // Start parsing incoming words and route them to interpretWord().
                    comServer.OnWordStreamReceive(MavcAgent.interpretWord);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: " + e);
                Thread.Sleep(1000);
            }

            Thread.Sleep(5000);
        }
    }

    #endregion
}
