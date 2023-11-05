using static COM;
using Newtonsoft.Json;


/**
 * Test enviroment of the target service/agent
 */
class MavcAgent
{
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
    private static List<AudioOutput> aoListVol2 = new List<AudioOutput>();
    private static List<AudioOutput> aoListVol3 = new List<AudioOutput>();
    private static List<AudioOutput> aoListVol4 = new List<AudioOutput>();

    public static void interpretWord(COM.Word word)
    {
        char action = word.action;
        String arg = word.args;

        switch (action)
        {
            case 'A':
                {
                    int argNum = int.Parse(arg);
                    Console.WriteLine("Set audio -2");
                    AudioDevice deflDev = audioContr.GetDefaultAudioDevice();
                    deflDev.SetVolume(argNum);
                    break;
                }
            case 'B':
                {
                    float argNum = int.Parse(arg);
                    Console.WriteLine("Set audio: " + argNum);
                    lock (mavcSaveLock)
                    {
                        foreach (AudioOutput ao in aoListVol1)
                        {
                            ao.SetVolume(argNum / 100f);
                        }
                    }
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
                UpdateAudioOutputs();
                Console.WriteLine("Conf Update: " + mavcSave);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        };

        // Enable the watcher
        watcher.EnableRaisingEvents = true;
    }

    /**
     * Updates the available volume mappings
     */
    public static void UpdateAudioOutputs()
    {
       lock(mavcSaveLock)
        {
            string json = File.ReadAllText(configFilePath);

            // Deserialize the JSON back to a class instance
            mavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);

            aoListVol1.Clear();
            aoListVol2.Clear();
            aoListVol3.Clear();
            aoListVol4.Clear();

            // update the vol mappings with the conf
            foreach (string name in mavcSave.namesVol1)
                aoListVol1.Add(audioContr.GetOutputByName(name));

            foreach (string name in mavcSave.namesVol2)
                aoListVol2.Add(audioContr.GetOutputByName(name));

            foreach (string name in mavcSave.namesVol3)
                aoListVol3.Add(audioContr.GetOutputByName(name));

            foreach (string name in mavcSave.namesVol4)
                aoListVol4.Add(audioContr.GetOutputByName(name));
        }
    }

    static void Main(string[] args)
    {
        bool foundFile = false;
        while (!foundFile)
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    UpdateAudioOutputs();
                    SetupConfUpdater();
                    foundFile = true;
                }

            }
            catch (Exception e){
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(5000);
            }

        }

        while (true) {
            Console.WriteLine("Waiting for hardware to connect.");
            try
            {
                COM comServer = new COM();
                comServer.OnWordStreamReceive(MavcAgent.interpretWord);
                Console.ReadLine();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(1000);
            }
        }
    }
}