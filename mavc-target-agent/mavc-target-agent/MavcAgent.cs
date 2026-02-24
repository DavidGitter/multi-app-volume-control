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

    // Per-knob output mappings (dynamic, one list per knob).
    private static List<List<AudioOutput>> aoLists = new List<List<AudioOutput>>();
    private static List<object> aoListLocks = new List<object>();

    // Serial/COM connection to the hardware mixer (reconnected if needed).
    private static COM comServer = null;

    // Redirect console output if AllocConsole() is used.
    private static Stream stdOut = null;
    private static StreamWriter writer = null;

    // Optional on-screen overlay (WinForms) that shows the last knob/value.
    private static bool screenOverlayEnabled = false;
    private static Overlay overlay = null;

    // Maps action characters to knob indices (A=0, B=1, C=2, D=3, ...).
    private static readonly char[] knobActions = "ABCDEFGHIJKLMNOP".ToCharArray();

    // Unified logger; writes to console + file with timestamps.
    private static Log logger;

    // Tracks which named outputs were already reported as offline to avoid log spam.
    // true = last known state was offline; removed from dict = online or never seen.
    private static readonly Dictionary<string, bool> offlineCache = new Dictionary<string, bool>();
    private static readonly object offlineCacheLock = new object();

    // Debounce for the audio-session-added callback (fires once per device, coalesce them).
    private static readonly object sessionDebounceLock = new object();
    private static System.Threading.Timer sessionDebounceTimer;

    #endregion

    #region Public Static Methods

    /**
     * Ensures aoLists and aoListLocks have enough entries for the current numberOfKnobs.
     */
    private static void EnsureAoListCapacity()
    {
        while (aoLists.Count < mavcSave.numberOfKnobs)
        {
            aoLists.Add(new List<AudioOutput>());
            aoListLocks.Add(new object());
        }
    }

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

        int knobCount = mavcSave.numberOfKnobs;

        // Optional knob order reversal (mirror: first <-> last, etc.).
        if (mavcSave.reverseKnobOrder && knobCount > 0)
        {
            int idx = Array.IndexOf(knobActions, action);
            if (idx >= 0 && idx < knobCount)
            {
                int reversed = knobCount - 1 - idx;
                action = knobActions[reversed];
            }
        }

        // Dispatch to the correct knob handler.
        int knobIndex = Array.IndexOf(knobActions, action);
        if (knobIndex < 0 || knobIndex >= knobCount)
            return;

        bool reverse = knobIndex < mavcSave.reverseKnobs.Count && mavcSave.reverseKnobs[knobIndex];

        // Use the pre-resolved output lists (kept up-to-date by the interval updater).
        List<AudioOutput> outputs;
        if (knobIndex < aoLists.Count)
        {
            lock (aoListLocks[knobIndex])
            {
                outputs = new List<AudioOutput>(aoLists[knobIndex]);
            }
        }
        else
        {
            outputs = new List<AudioOutput>();
        }

        HandleKnob(knobIndex + 1, arg, reverse, outputs);
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
                    catch (Exception ex) { logger?.Error($"Config reload failed: {ex.Message}"); }
                }, null, 50, Timeout.Infinite);
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
            loaded.EnsureCapacity();

            // Only skip heavy rebuild when ONLY overlay position changed
            bool onlyOverlayMoved =
                (loaded.overlayX != mavcSave.overlayX || loaded.overlayY != mavcSave.overlayY) &&
                loaded.numberOfKnobs == mavcSave.numberOfKnobs &&
                loaded.reverseKnobOrder == mavcSave.reverseKnobOrder &&
                loaded.enableScreenOverlay == mavcSave.enableScreenOverlay;

            mavcSave = loaded;
            EnsureAoListCapacity();

            if (onlyOverlayMoved && screenOverlayEnabled && overlay != null)
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
            EnsureAoListCapacity();

            for (int i = 0; i < mavcSave.numberOfKnobs && i < mavcSave.volumeMappings.Count; i++)
            {
                UpdateAOsList(aoLists[i], aoListLocks[i], mavcSave.volumeMappings[i]);
            }
        }
    }

    #endregion

    #region Private Static Methods

    /**
     * Converts the raw knob value into a 0..1 volume value (optionally reversed),
     * applies it to all mapped targets, and updates the overlay.
     *
     * @param knobIndex  knob number (1..N)
     * @param arg        raw device argument (typically "0".."100")
     * @param reverse    if true, invert the knob direction
     * @param outputs    resolved output targets controlled by this knob
     */
    private static void HandleKnob(int knobIndex, string arg, bool reverse, List<AudioOutput> outputs)
    {
        float raw = int.Parse(arg);

        float value = reverse
            ? (raw > 0 ? 1f - raw / 100f : 1f)
            : (raw > 0 ? raw / 100f : 0);

        int pct = (int)(value * 100);

        // Bar: 4 segments, each lights up at 25/50/75/100%.
        // Even 1% shows one '=' so the bar is never misleadingly blank.
        int filled = pct == 0 ? 0 : Math.Max(1, (int)Math.Round(value * 4));
        string bar = new string('=', filled).PadRight(4);

        // {pct,3} right-aligns in 3 chars: "  0", "  1" .. " 99", "100"
        logger?.Info($"[Knob {knobIndex}] {pct,3}% [{bar}] -> {outputs.Count} target(s)");

        // Apply volume to all mapped targets.
        foreach (AudioOutput ao in outputs)
        {
            try
            {
                if (ao != null)
                    ao.SetVolume(value);
            }
            catch (Exception ex)
            {
                logger?.Error($"  !! {ao?.GetName()}: {ex.Message}");
            }
        }

        // Best-effort overlay update
        if (screenOverlayEnabled && overlay != null)
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
            int previousCount = target.Count;
            target.Clear();

            foreach (MAVCSave.AudioOutput ao in config)
            {
                try
                {
                    if (!ao.type.Equals("Function"))
                    {
                        var resolved = audioContr.GetOutputsByName(ao.name);
                        bool isOffline = resolved.Count == 0;

                        lock (offlineCacheLock)
                        {
                            bool wasOffline = !offlineCache.TryGetValue(ao.name, out bool cached) ? false : cached;

                            if (isOffline && !wasOffline)
                            {
                                logger?.Warning($"'{ao.name}' went offline (not running or no audio)");
                                offlineCache[ao.name] = true;
                            }
                            else if (!isOffline && wasOffline)
                            {
                                logger?.Info($"'{ao.name}' is back online");
                                offlineCache[ao.name] = false;
                            }
                            else if (!isOffline)
                            {
                                offlineCache[ao.name] = false;
                            }
                        }

                        target.AddRange(resolved);
                    }
                    else if (ao.name.Equals("Focused"))
                    {
                        target.Add(new AudioFocused(audioContr));
                    }
                    else if (ao.name.Equals("Other Apps"))
                    {
                        target.Add(new AudioOtherApps(audioContr, mavcSave));
                    }
                }
                catch (Exception ex)
                {
                    logger?.Error($"Failed to resolve '{ao.name}': {ex.Message}");
                }
            }

            if (target.Count != previousCount)
                logger?.Info($"Targets updated: {previousCount} -> {target.Count}");
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

        Console.Title = "MAVC Agent";
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
        try { Directory.CreateDirectory(configSavePath); } catch { }

        // initialize logger first; capture subsequent messages
        logger = new Log(Path.Combine(configSavePath, "agent-log.txt"));

        bool foundFile = false;

        while (!foundFile)
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    UpdateMAVCSave();
                    SetupConfUpdater();
                    foundFile = true;

                    if (mavcSave.enableDebugMode)
                        enableDebugWindow();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Config load failed: {ex.Message}");
                Thread.Sleep(5000);
            }
        }

        logger.Info("============================================================");
        logger.Info("         MAVC Agent - Multi-App Volume Control              ");
        logger.Info("============================================================");
        logger.Info($"Agent:  {AppDomain.CurrentDomain.BaseDirectory}");
        logger.Info($"Config: {configFilePath}");

        audioContr.onOutputAddedCallback((sender, newSession) =>
        {
            // The event fires once per audio device; debounce into a single refresh.
            lock (sessionDebounceLock)
            {
                sessionDebounceTimer?.Dispose();
                sessionDebounceTimer = new System.Threading.Timer(_ =>
                {
                    logger.Info("New audio session detected - refreshing mappings");
                    audioContr.InvalidateCache();
                    lock (mavcSaveLock) { UpdateAllAOs(); }
                    comServer?.updateVolumes();
                }, null, 500, Timeout.Infinite);
            }
        });

        Task intervalUpdater = new Task(() =>
        {
            while (true)
            {
                audioContr.InvalidateCache();
                lock (mavcSaveLock) { UpdateAllAOs(); }
                Thread.Sleep(10_000);
            }
        });
        intervalUpdater.Start();

        screenOverlayEnabled = mavcSave.enableScreenOverlay;
        logger.Info($"Overlay: {(screenOverlayEnabled ? "Enabled" : "Disabled")}");

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

        logger.Info("------------------------------------------------------------");
        while (true)
        {
            try
            {
                if (comServer == null || !comServer.IsOpen())
                {
                    logger.Info("Waiting for hardware (COM3, 9600 baud)...");
                    comServer = new COM("COM3", 9600);
                    comServer.SetErrorLogger((msg) => logger.Error($"COM error: {msg}"));
                    logger.Info("Hardware connected");

                    comServer.OnWordStreamReceive(MavcAgent.interpretWord);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Connection error: {e.Message}");
                Thread.Sleep(1000);
            }

            Thread.Sleep(5000);
        }
    }

    #endregion
}
