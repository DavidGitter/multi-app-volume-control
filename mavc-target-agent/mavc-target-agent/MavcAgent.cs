using static COM;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// For console debugging -> change Project > Properties > Windows Application to Console Application
class MavcAgent
{
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

    /**
     * Function that interprets the words received from the mixer.
     *
     * <param name="word">The word to be interpreted (see COM class).</param>
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

    /**
     * Converts the raw knob value into a 0..1 volume value (optionally reversed),
     * applies it to all mapped targets, and updates the overlay.
     *
     * <param name="knobIndex">Knob number (1..4).</param>
     * <param name="arg">Raw device argument (typically "0".."100").</param>
     * <param name="reverse">If true, invert the knob direction.</param>
     * <param name="outputs">Resolved output targets controlled by this knob.</param>
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

    // Sets up FileSystemWatcher to reload config when config.json changes.
    // Note: FileSystemWatcher can raise duplicate/multiple Changed events in practice.
    public static void SetupConfUpdater()
    {
        watcher = new FileSystemWatcher
        {
            Path = configSavePath,
            Filter = configFileName,
            EnableRaisingEvents = true
        };

        // Subscribe to the Changed event
        watcher.Changed += (sender, e) =>
        {
            try
            {
                // Ask hardware to resend its current state (keeps software and hardware aligned),
                // then reload config + remap outputs.
                comServer.updateVolumes();
                UpdateMAVCSave();

                Console.WriteLine("Conf Update: " + mavcSave);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        };
    }

    // Reads config.json and refreshes audio mappings.
    public static void UpdateMAVCSave()
    {
        lock (mavcSaveLock)
        {
            string json = File.ReadAllText(configFilePath);
            mavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);
        }

        // AudioController caches enumerations; invalidate so new outputs/sessions are visible.
        audioContr.InvalidateCache();
        UpdateAllAOs();
    }

    // Rebuild all knob->AudioOutput mappings from the current config.
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

    // Rebuilds one mapping list from config:
    // - Normal entries: resolve outputs by name.
    // - Function entries: create synthetic outputs (Focused, Other Apps).
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

    // Opens a console and redirects stdout/stderr so Console.WriteLine() is visible.
    private static void enableDebugWindow()
    {
        AllocConsole();

        stdOut = Console.OpenStandardOutput();
        writer = new StreamWriter(stdOut) { AutoFlush = true };
        Console.SetOut(writer);
        Console.SetError(writer);

        Console.OutputEncoding = Encoding.UTF8;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Started Mavc Debug-Console");
        bool foundFile = false;

        Log logger = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC", "agent-log.txt"));

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

        // Wait until config file exists, then load it and start watching for changes.
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
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(5000);
            }
        }

        screenOverlayEnabled = mavcSave.enableScreenOverlay;
        Console.WriteLine("overlay enabled: " + screenOverlayEnabled);

        // Start overlay UI (separate thread with its own message loop).
        if (screenOverlayEnabled)
        {
            Task ui = new Task(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                overlay = new Overlay(mavcSave.autoHideAfterSec);
                overlay.SetAutoHideActive(mavcSave.activateAutoHide);
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
}
