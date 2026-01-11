using static COM;
using Newtonsoft.Json;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading.Tasks;
using System.Windows.Forms;
/**
 * Test enviroment of the target service/agent
 */

// For console debugging -> change Project > Properties > Windows Application to Console Application
class MavcAgent
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    /*static void Main(string[] args)
    {
        AudioController controller = new AudioController();
        foreach (var device in controller.GetAudioDevices())
        {
            Console.WriteLine("__Output Device: " + device + "__");
            foreach (var app in device.GetAudioApps())
            {
                Console.WriteLine(app);
                if (app.GetName().Equals("Spotify"))
                {
                    Console.WriteLine("Audio Device Name: " + app.GetIODevice().GetName());
                }
            }
        }

        AudioDevice defaultDevice = controller.GetDefaultAudioDevice();
        Console.WriteLine("__Default Output Device: " + defaultDevice + "__");
    }*/

    public static AudioController audioContr = new AudioController();
    public static string configSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
    public static string configFileName = "config.json";
    public static string configFilePath = Path.Combine(configSavePath, configFileName);
    public static FileSystemWatcher watcher;

    /**
     * Function that interprets the words receiveds of the mixer
     * 
     * @param word  the word to be interpreted (see COM class)
     */

    private static MAVCSave mavcSave = new MAVCSave();
    private static object mavcSaveLock = new object();

    private static List<AudioOutput> aoListVol1 = new List<AudioOutput>();
    private static object aoList1Lock = new object();

    private static List<AudioOutput> aoListVol2 = new List<AudioOutput>();
    private static object aoList2Lock = new object();

    private static List<AudioOutput> aoListVol3 = new List<AudioOutput>();
    private static object aoList3Lock = new object();

    private static List<AudioOutput> aoListVol4 = new List<AudioOutput>();
    private static object aoList4Lock = new object();

    private static COM comServer = null;

    private static Stream stdOut = null;
    private static StreamWriter writer = null;

    private static bool screenOverlayEnabled = false;
    private static Overlay overlay = null;

    public static void interpretWord(COM.Word word)
    {
        char action = word.action;
        String arg = word.args;

        if (mavcSave.reverseKnobOrder)
        {
            switch (action)
            {
                case 'A':
                    action = 'D';
                    break;
                case 'B':
                    action = 'C';
                    break;
                case 'C':
                    action = 'B';
                    break;
                case 'D':
                    action = 'A';
                    break;
            }
        }

        switch (action)
        {
            case 'A':
                {
                    float argNum = int.Parse(arg);
                    if (mavcSave.reverseKnob1 == true)
                        argNum = argNum > 0 ? 1f - argNum / 100f : 100; // reversed knob 1
                    else
                        argNum = argNum > 0 ? argNum / 100f : 0;
                    Console.WriteLine("Set Volume 1: " + argNum);

                    foreach (AudioOutput ao in aoListVol1)
                    {
                        if (ao != null)
                            ao.SetVolume(argNum);
                    }

                    if (screenOverlayEnabled)
                        overlay.setUpdatedVolume("Volume 1", (int)(argNum * 100));

                    break;
                }
            case 'B':
                {
                    float argNum = int.Parse(arg);
                    if (mavcSave.reverseKnob2 == true)
                        argNum = argNum > 0 ? 1f - argNum / 100f : 100; // reversed knob 2
                    else
                        argNum = argNum > 0 ? argNum / 100f : 0;
                    Console.WriteLine("Set Volume 2: " + argNum);

                    foreach (AudioOutput ao in aoListVol2)
                    {
                        if (ao != null)
                            ao.SetVolume(argNum);
                    }

                    if (screenOverlayEnabled)
                        overlay.setUpdatedVolume("Volume 2", (int)(argNum * 100));

                    break;
                }
            case 'C':
                {
                    float argNum = int.Parse(arg);
                    if (mavcSave.reverseKnob3 == true)
                        argNum = argNum > 0 ? 1f - argNum / 100f : 100; // reversed knob 3
                    else
                        argNum = argNum > 0 ? argNum / 100f : 0;
                    Console.WriteLine("Set Volume 3: " + argNum);

                    foreach (AudioOutput ao in aoListVol3)
                    {
                        if (ao != null)
                            ao.SetVolume(argNum);
                    }

                    if (screenOverlayEnabled)
                        overlay.setUpdatedVolume("Volume 3", (int)(argNum*100));

                    break;
                }
            case 'D':
                {
                    float argNum = int.Parse(arg);
                    if (mavcSave.reverseKnob4 == true)
                        argNum = argNum > 0 ? 1f - argNum / 100f : 100; // reversed knob 4
                    else
                        argNum = argNum > 0 ? argNum / 100f : 0;
                    Console.WriteLine("Set Volume 4: " + argNum);

                    foreach (AudioOutput ao in aoListVol4)
                    {
                        if (ao != null)
                            ao.SetVolume(argNum);
                    }

                    if (screenOverlayEnabled)
                        overlay.setUpdatedVolume("Volume 4", (int)(argNum * 100));

                    break;
                }
            default:
                throw new InvalidDataException();
        }
    }

    /**
     * Setsup a file watcher that updates the config when the file changed
     */
    public static void SetupConfUpdater()
    {
        //enable file watcher
        watcher = new FileSystemWatcher();

        // Set the path to the directory containing the file
        watcher.Path = configSavePath;

        // Set the filter to watch for changes to a specific file
        watcher.Filter = configFileName;

        // Subscribe to the Changed event
        watcher.Changed += (sender, e) =>
        {
            try
            {
                comServer.updateVolumes();
                UpdateMAVCSave();
                Console.WriteLine("Conf Update: " + mavcSave);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        };

        // Enable the watcher
        watcher.EnableRaisingEvents = true;
    }


    public static void UpdateMAVCSave()
    {

        lock (mavcSaveLock)
        {
            string json = File.ReadAllText(configFilePath);

            // Deserialize the JSON back to a class instance
            mavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);
        }

        UpdateAllAOs();
    }

    /**
     * Updates the available volume mappings
     */
    public static void UpdateAOsList1()
    {
        lock (aoList1Lock)
        {
            aoListVol1.Clear();

            // update the vol mappings with the conf
            foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol1)
                if (!mavc_ao.type.Equals("Function"))
                    aoListVol1.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
                else
                {
                    if (mavc_ao.name.Equals("Focused"))
                        aoListVol1.Add(new AudioFocused(audioContr));
                    else if(mavc_ao.name.Equals("Other Apps"))
                        aoListVol1.Add(new AudioOtherApps(audioContr, mavcSave));
                    else
                        throw new NotImplementedException();
                }
        }
    }

    public static void UpdateAOsList2()
    {
        lock (aoList2Lock)
        {
            aoListVol2.Clear();

            // update the vol mappings with the conf
            foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol2)
                if (!mavc_ao.type.Equals("Function"))
                    aoListVol2.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
                else
                {
                    if (mavc_ao.name.Equals("Focused"))
                        aoListVol2.Add(new AudioFocused(audioContr));
                    else if(mavc_ao.name.Equals("Other Apps"))
                        aoListVol2.Add(new AudioOtherApps(audioContr, mavcSave));
                    else
                        throw new NotImplementedException();
                }
        }
    }

    public static void UpdateAOsList3()
    {
        lock (aoList3Lock)
        {
            aoListVol3.Clear();

            // update the vol mappings with the conf
            foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol3)
                if (!mavc_ao.type.Equals("Function"))
                    aoListVol3.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
                else
                {
                    if (mavc_ao.name.Equals("Focused"))
                        aoListVol3.Add(new AudioFocused(audioContr));
                    else if(mavc_ao.name.Equals("Other Apps"))
                        aoListVol3.Add(new AudioOtherApps(audioContr, mavcSave));
                    else
                        throw new NotImplementedException();
                }
        }
    }

    public static void UpdateAOsList4()
    {
        lock (aoList4Lock)
        {
            aoListVol4.Clear();

            // update the vol mappings with the conf
            foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol4)
                if (!mavc_ao.type.Equals("Function"))
                    aoListVol4.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
                else
                {
                    if (mavc_ao.name.Equals("Focused"))
                        aoListVol4.Add(new AudioFocused(audioContr));
                    else if (mavc_ao.name.Equals("Other Apps"))
                        aoListVol4.Add(new AudioOtherApps(audioContr, mavcSave));
                    else
                        throw new NotImplementedException();
                }
        }
    }

    public static void UpdateAllAOs()
    {
        lock (mavcSaveLock)
        {
            UpdateAOsList1();
            UpdateAOsList2();
            UpdateAOsList3();
            UpdateAOsList4();
        }
    }

    private static void enableDebugWindow()
    {
        AllocConsole();

        stdOut = Console.OpenStandardOutput();
        writer = new StreamWriter(stdOut)
        {
            AutoFlush = true
        };
        Console.SetOut(writer);
        Console.SetError(writer);

        Console.OutputEncoding = Encoding.UTF8;
    }


    static void Main(string[] args)
    {

        Console.WriteLine("Started Mavc Debug-Console");
        bool foundFile = false;
        Log logger = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC", "agent-log.txt"));

        var def = audioContr.GetAudioDevices();
        audioContr.onOutputAddedCallback((sender, newSession) => {
            Console.WriteLine("new audio output found!");
            logger.Info("A new output was found and added to the agent.");
            UpdateAllAOs();
            comServer.updateVolumes();
        });

        //Interval Updater
        Task intervalUpdater = new Task(() => {
            while (true)
            {
                lock (mavcSaveLock)
                    UpdateAOsList1();
                Thread.Sleep(2500);
                lock (mavcSaveLock)
                    UpdateAOsList2();
                Thread.Sleep(2500);
                lock (mavcSaveLock)
                    UpdateAOsList3();
                Thread.Sleep(2500);
                lock (mavcSaveLock)
                    UpdateAOsList4();
                Thread.Sleep(2500);
            }
        });
        intervalUpdater.Start();

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

        if (screenOverlayEnabled)   // Start Overlay UI if activated by conf
        {
            Task UI = new Task(() =>
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                overlay = new Overlay(mavcSave.autoHideAfterSec);
                overlay.SetAutoHideActive(true);
                Application.Run(overlay);
            });
            UI.Start();
        }


        while (true)
        {
            try
            {
                if (comServer == null || !comServer.IsOpen())
                {
                    Console.WriteLine("Waiting for hardware to connect (COM3, 9600).");
                    comServer = new COM("COM3", 9600);
                    Console.WriteLine("Hardware connected.");
                    comServer.OnWordStreamReceive(MavcAgent.interpretWord);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: " + e.ToString());
                Thread.Sleep(1000);
            }

            Thread.Sleep(5000);
        }
    }
}