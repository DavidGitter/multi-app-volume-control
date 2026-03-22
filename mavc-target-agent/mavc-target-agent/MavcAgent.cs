using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Linq;

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
    public static FileSystemWatcher? watcher;

    // Current configuration + lock (config is read/updated from multiple threads).
    private static MAVCSave mavcSave = new MAVCSave();
    private static readonly object mavcSaveLock = new object();

    // Per-knob output mappings (dynamic, one list per knob).
    private static List<List<AudioOutput>> aoLists = new List<List<AudioOutput>>();
    private static List<object> aoListLocks = new List<object>();

    // Serial/COM connection to the hardware mixer (reconnected if needed).
    private static COM? comServer = null;

    // Redirect console output if AllocConsole() is used.
    private static Stream? stdOut = null;
    private static StreamWriter? writer = null;

    // Optional on-screen overlay (WinForms) that shows the last knob/value.
    private static bool screenOverlayEnabled = false;
    private static Overlay? overlay = null;

    // Maps action characters to knob indices (A=0, B=1, C=2, D=3, ...).
    private static readonly char[] knobActions = "ABCDEFGHIJKLMNOP".ToCharArray();

    // Unified logger; writes to console + file with timestamps.
    private static Log? logger;

    // Tracks which named outputs were already reported as offline to avoid log spam.
    // true = last known state was offline; removed from dict = online or never seen.
    private static readonly Dictionary<string, bool> offlineCache = new Dictionary<string, bool>();
    private static readonly object offlineCacheLock = new object();

    // Debounce for the audio-session-added callback (fires once per device, coalesce them).
    private static readonly object sessionDebounceLock = new object();
    private static System.Threading.Timer? sessionDebounceTimer;

    // prevents COM-thread queue buildup
    // Each element holds the most-recent raw knob value (0..100), or -1 = idle.
    // Updated with Interlocked so the COM-receive thread never blocks.
    private static readonly int[] latestKnobRaw = new int[16];   // -1 = nothing pending
    private static readonly int[] knobHasPending = new int[16];  // 0 = idle, 1 = pending
    // Semaphore is released exactly once per idle -> pending transition per knob.
    private static readonly SemaphoreSlim workAvailable = new SemaphoreSlim(0, int.MaxValue);

    // clear whenever COM object is torn down, so reconnect always resends pin mappings
    private static bool initializedPins = false;

    #endregion

    #region Static Constructor

    static MavcAgent()
    {
        // Initialise all slots to "nothing pending".
        for (int i = 0; i < latestKnobRaw.Length; i++)
            latestKnobRaw[i] = -1;
    }

    #endregion

    #region Public Static Methods

    private static void EnsureAoListCapacity()
    {
        while (aoLists.Count < mavcSave.numberOfKnobs)
        {
            aoLists.Add(new List<AudioOutput>());
            aoListLocks.Add(new object());
        }
    }

    /**
     * Called from the COM-receive thread on every incoming word.
     * ONLY stores the latest value per knob; actual processing happens on the
     * dedicated KnobProcessor thread, so this method returns in nanoseconds.
     *
     * @param word  the word to be interpreted (see COM class)
     */
    public static void InterpretWord(COM.Word word)
    {
        char action = word.action;
        string arg = word.args;
        int knobCount = mavcSave.numberOfKnobs;

        if (action == 'Q')
        { // Debug Answer from mixer: print and return
            logger?.Info("Mixer - Message: " + arg);
            return;
        }

        if (mavcSave.reverseKnobOrder && knobCount > 0)
        {
            int idx = Array.IndexOf(knobActions, action);
            if (idx >= 0 && idx < knobCount)
                action = knobActions[knobCount - 1 - idx];
        }

        int knobIndex = Array.IndexOf(knobActions, action);
        if (knobIndex < 0 || knobIndex >= knobCount || knobIndex >= 16)
            return;

        if (!int.TryParse(arg, out int raw))
            return;

        // Overwrite stale value; only signal the processor once per burst.
        Interlocked.Exchange(ref latestKnobRaw[knobIndex], raw);
        if (Interlocked.Exchange(ref knobHasPending[knobIndex], 1) == 0)
            workAvailable.Release();
    }

    private static readonly object confDebounceLock = new object();
    private static System.Threading.Timer? confDebounceTimer;

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

    public static void UpdateMAVCSave()
    {
        MAVCSave loaded;

        lock (mavcSaveLock)
        {
            string json = File.ReadAllText(configFilePath);
            loaded = JsonConvert.DeserializeObject<MAVCSave>(json) ?? new MAVCSave();
            loaded.EnsureCapacity();

            bool onlyOverlayMoved =
                (loaded.overlayX != mavcSave.overlayX || loaded.overlayY != mavcSave.overlayY) &&
                loaded.numberOfKnobs == mavcSave.numberOfKnobs &&
                loaded.reverseKnobOrder == mavcSave.reverseKnobOrder &&
                loaded.enableScreenOverlay == mavcSave.enableScreenOverlay;

            // check if pin mappings has changed
            //if (comServer.IsOpen() && !loaded.pinMappings.SequenceEqual(mavcSave.pinMappings))
            //{
            //    // restart mixer to apply new pin mappings
            //    RestartMixer();
            //}

            mavcSave = loaded;
            EnsureAoListCapacity();

            if (onlyOverlayMoved && screenOverlayEnabled && overlay != null)
            {
                overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);
                return;
            }
        }

        audioContr.InvalidateCache();
        UpdateAllAOs();

        if (screenOverlayEnabled && overlay != null)
            overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);
    }

    public static void UpdateAllAOs()
    {
        lock (mavcSaveLock)
        {
            EnsureAoListCapacity();

            for (int i = 0; i < mavcSave.numberOfKnobs && i < mavcSave.volumeMappings.Count; i++)
                UpdateAOsList(aoLists[i], aoListLocks[i], mavcSave.volumeMappings[i]);
        }
    }

    #endregion

    #region Private Static Methods

    /**
     * Dedicated processor thread for knob events.
     * Wakes on workAvailable, drains every pending knob slot (latest value only),
     * and goes back to sleep.  One thread is enough; audio API calls are serialised
     * naturally, and stale intermediate values are simply skipped.
     */
    private static void StartKnobProcessor()
    {
        Thread t = new Thread(() =>
        {
            while (true)
            {
                workAvailable.Wait();

                int knobCount = mavcSave.numberOfKnobs;
                for (int i = 0; i < knobCount && i < 16; i++)
                {
                    // Atomically claim the pending flag.
                    if (Interlocked.Exchange(ref knobHasPending[i], 0) != 1)
                        continue;

                    // Read and clear the latest raw value.
                    int raw = Interlocked.Exchange(ref latestKnobRaw[i], -1);
                    if (raw < 0)
                        continue;

                    bool reverse = i < mavcSave.reverseKnobs.Count && mavcSave.reverseKnobs[i];

                    List<AudioOutput> outputs;
                    lock (aoListLocks[i])
                        outputs = new List<AudioOutput>(aoLists[i]);

                    HandleKnob(i + 1, raw, reverse, outputs);
                }
            }
        })
        {
            IsBackground = true,
            Name = "KnobProcessor"
        };
        t.Start();
    }

    /**
     * Converts a raw knob value into volume and applies it to all mapped targets.
     * Now receives a pre-parsed int so no string allocation happens here.
     */
    private static void HandleKnob(int knobIndex, int rawValue, bool reverse, List<AudioOutput> outputs)
    {
        float value = reverse
            ? (rawValue > 0 ? 1f - rawValue / 100f : 1f)
            : (rawValue > 0 ? rawValue / 100f : 0f);

        int pct = (int)(value * 100);
        int filled = pct == 0 ? 0 : Math.Max(1, (int)Math.Round(value * 4));
        string bar = new string('=', filled).PadRight(4);

        logger?.Mixer($"[Knob {knobIndex}] {pct,3}% [{bar}] -> {outputs.Count} target(s)");

        foreach (AudioOutput ao in outputs)
        {
            try { ao?.SetVolume(value); }
            catch (Exception ex) { logger?.Error($"  !! {ao?.GetName()}: {ex.Message}"); }
        }

        if (screenOverlayEnabled && overlay != null)
            overlay.setUpdatedVolume($"Knob {knobIndex}", pct);
    }

    /**
     * Summary of aoLists.
     * Logs multi-line breakdown if mappings changed.
     */
    private static string lastMappingSummary = "";
    private static void MappingSummary(string trigger)
    {
        string current = string.Join(" | ", Enumerable.Range(0, Math.Min(aoLists.Count, mavcSave.numberOfKnobs))
            .Select(i => { lock (aoListLocks[i]) { return $"Knob {i + 1}: [{string.Join(", ", aoLists[i].Select(ao => ao?.GetName() ?? "?"))}]"; } }));

        if (current != lastMappingSummary)
        {
            lastMappingSummary = current;
            logger?.Info($"{trigger}: mappings updated");
            foreach (string knob in current.Split(" | "))
                logger?.Info($"  {knob}");
        }
        else
        {
            logger?.Info($"{trigger}: no changes");
        }
    }

    /**
     * Rebuilds one mapping list from the config.
     * Disposes old outputs before clearing to release COM references immediately.
     *
     * @param target      the list of AudioOutput targets to rebuild
     * @param targetLock  lock object protecting the target list
     * @param config      configured audio outputs to resolve
     */
    private static void UpdateAOsList(List<AudioOutput> target, object targetLock,
                                      IEnumerable<MAVCSave.AudioOutput> config)
    {
        lock (targetLock)
        {
            // Release COM / unmanaged resources held by the previous outputs.
            foreach (AudioOutput ao in target)
            {
                try
                {
                    (ao as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    logger?.Error($"Error disposing AudioOutput '{ao?.GetName()}': {ex.Message}");
                }
            }
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
                            bool wasOffline = offlineCache.TryGetValue(ao.name, out bool cached) && cached;

                            if (isOffline && !wasOffline)
                            {
                                logger?.Warning($"[{ao.name}] offline");
                                offlineCache[ao.name] = true;
                            }
                            else if (!isOffline && wasOffline)
                            {
                                logger?.Info($"[{ao.name}] back online");
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
                        target.Add(new AudioFocused(audioContr));
                    else if (ao.name.Equals("Other Apps"))
                        target.Add(new AudioOtherApps(audioContr, mavcSave));
                }
                catch (Exception ex)
                {
                    logger?.Error($"Failed to resolve '{ao.name}': {ex.Message}");
                }
            }
        }
    }

    /** Opens a console and redirects stdout/stderr so Console.WriteLine() is visible. */
    private static void EnableDebugWindow()
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

    private static void RestartMixer()
    {
        logger?.Info("Sending restarting command to mixer...");
        comServer?.SendCommand('Z', "Restart");
        Thread.Sleep(6000);
    }

    /**
     * Tears down the current COM object and resets pin initialisation state.
     * Must be called before nulling comServer so the next reconnect loop
     * iteration sends fresh pin mappings to the mixer.
     */
    // [fix/#134] extracted so both the OnDisconnected handler and the reconnect loop use the same teardown path
    private static void TeardownCom()
    {
        initializedPins = false;
        logger?.Info("COM teardown: initializedPins reset, comServer cleared");
        try { comServer?.OnWordStreamReceive(_ => { }); } catch { }
        comServer = null;
    }

    #endregion

    // initializes all pins of the microcontroller at agent startup
    private static void HandlePinInitialization(COM com)
    {
        if (!initializedPins)
        {
            string dotSeperatedPins = "";
            foreach (int i in mavcSave.pinMappings)
            {
                dotSeperatedPins += i + ".";
            }
            COM.Word w = new COM.Word('V', dotSeperatedPins);
            com.SendCommand(w);
            logger?.Info("Send pin mappings: " + w.args);
            // [fix/#134] set true only after the send so a failed send leaves it false and retries on next connect
            initializedPins = true;
        }
    }

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

        if (mavcSave.enableDebugMode) EnableDebugWindow();
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
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Config load failed: {ex.Message}");
                Thread.Sleep(5000);
            }
        }

        logger.Info("============================================================");
        logger.Info("         MAVC Agent - Multi-App Volume Control");
        logger.Info("============================================================");
        logger.Info($"Agent:  {AppDomain.CurrentDomain.BaseDirectory}");
        logger.Info($"Config: {configFilePath}");

        audioContr.onOutputAddedCallback((sender, newSession) =>
        {
            // Capture process name immediately on the callback thread before the debounce delay.
            string sessionName = "(unknown)";
            try
            {
                uint pid = 0;
                (newSession as NAudio.CoreAudioApi.Interfaces.IAudioSessionControl2)?.GetProcessId(out pid);
                sessionName = pid > 0 ? System.Diagnostics.Process.GetProcessById((int)pid).ProcessName : "(unnamed)";
            }
            catch { }

            lock (sessionDebounceLock)
            {
                sessionDebounceTimer?.Dispose();
                string capturedName = sessionName; // capture value into closure, not the variable reference
                sessionDebounceTimer = new System.Threading.Timer(_ =>
                {
                    logger?.Info($"New audio session detected: '{capturedName}'");
                    audioContr.InvalidateCache();
                    lock (mavcSaveLock) { UpdateAllAOs(); }
                    MappingSummary("Session refresh");
                    comServer?.UpdateVolumes();
                }, null, 500, Timeout.Infinite);
            }
        });

        Task.Factory.StartNew(() =>
        {
            logger?.Info("Periodic refresh task started");
            while (true)
            {
                try
                {
                    Thread.Sleep(30_000);
                    audioContr.InvalidateCache();
                    lock (mavcSaveLock) { UpdateAllAOs(); }
                    // Log after the refresh so the summary reflects the new state.
                    MappingSummary("Periodic refresh");
                }
                catch (Exception ex)
                {
                    logger?.Error($"Interval updater error: {ex.Message}");
                }
            }
        }, TaskCreationOptions.LongRunning);

        // Start the single knob-processor thread (latest-value, zero backlog).
        StartKnobProcessor();

        screenOverlayEnabled = mavcSave.enableScreenOverlay;
        logger.Info($"Overlay: {(screenOverlayEnabled ? "Enabled" : "Disabled")}");
        MappingSummary("AO List");

        if (screenOverlayEnabled)
        {
            Task.Factory.StartNew(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                overlay = new Overlay(mavcSave.autoHideAfterSec);
                overlay.SetAutoHideActive(mavcSave.activateAutoHide);
                overlay.SetOverlayPosition(mavcSave.overlayX, mavcSave.overlayY);

                Application.Run(overlay);
            }, TaskCreationOptions.LongRunning);
        }

        logger.Info("------------------------------------------------------------");

        while (true)
        {
            try
            {
                if (comServer == null || !comServer.IsOpen())
                {
                    logger.Info("Waiting for hardware (COM3, 115200 baud)...");
                    var com = new COM("COM3", 115200);

                    com.SetErrorLogger(msg => logger.Error($"COM error: {msg}"));

                    // [fix/#134] if a send fails mid-session the COM class signals us here so we tear down immediately
                    // rather than waiting for the next IsOpen() check (which would stay true on physical disconnect)
                    com.OnDisconnected += () =>
                    {
                        logger?.Info("Disconnect signalled by COM layer, tearing down...");
                        TeardownCom();
                    };

                    com.OnWordStreamReceive(MavcAgent.InterpretWord);
                    comServer = com;

                    logger.Info("Hardware connected");
                    HandlePinInitialization(comServer);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Connection error: {e.Message}");
                // [fix/#134] ensure state is clean before the next reconnect attempt
                TeardownCom();
                Thread.Sleep(1000);
            }

            Thread.Sleep(5000);
        }
    }

    #endregion
}