using static COM;
using Newtonsoft.Json;
using System.IO.Ports;

/**
 * Test enviroment of the target service/agent
 */

// For console debugging -> change Project > Properties > Windows Application to Console Application
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

    private static COM comServer = null;

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
                    if(mavcSave.reverseKnob1 == true)
                        argNum = argNum > 0 ? 1f - argNum / 100f : 100; // reversed knob 1
                    else
                        argNum = argNum > 0 ? argNum / 100f : 0;
                    Console.WriteLine("Set Volume 1: " + argNum);
                    lock (mavcSaveLock)
                    {
                        foreach (AudioOutput ao in aoListVol1)
                        {
                            if (ao != null)
                                ao.SetVolume(argNum);
                        }
                    }
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
                    lock (mavcSaveLock)
                    {
                        foreach (AudioOutput ao in aoListVol2)
                        {
                            if(ao != null)
                                ao.SetVolume(argNum);
                        }
                    }
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
                    lock (mavcSaveLock)
                    {
                        foreach (AudioOutput ao in aoListVol3)
                        {
                            if (ao != null)
                                ao.SetVolume(argNum);
                        }
                    }
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
                    lock (mavcSaveLock)
                    {
                        foreach (AudioOutput ao in aoListVol4)
                        {
                            if (ao != null)
                                ao.SetVolume(argNum);
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
                comServer.updateVolumes();
                UpdateMAVCSave();
                Console.WriteLine("Conf Update: " + mavcSave);
            }catch(Exception ex)
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

        UpdateAudioOutputs();
    }

    /**
     * Updates the available volume mappings
     */
    public static void UpdateAudioOutputs()
    {
        aoListVol1.Clear();
        aoListVol2.Clear();
        aoListVol3.Clear();
        aoListVol4.Clear();

        // update the vol mappings with the conf
        foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol1)
            if(!mavc_ao.type.Equals("Function"))
                aoListVol1.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
            else
            {
                if (mavc_ao.name.Equals("Focused"))
                    aoListVol1.Add(new AudioFocused(audioContr));
                else
                    throw new NotImplementedException();
            }

        foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol2)
            if (!mavc_ao.type.Equals("Function"))
                aoListVol2.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
            else
            {
                if (mavc_ao.name.Equals("Focused"))
                    aoListVol2.Add(new AudioFocused(audioContr));
                else
                    throw new NotImplementedException();
            }

        foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol3)
            if (!mavc_ao.type.Equals("Function"))
                aoListVol3.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
            else
            {
                if (mavc_ao.name.Equals("Focused"))
                    aoListVol3.Add(new AudioFocused(audioContr));
                else
                    throw new NotImplementedException();
            }

        foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol4)
            if (!mavc_ao.type.Equals("Function"))
                aoListVol4.AddRange(audioContr.GetOutputsByName(mavc_ao.name));
            else
            {
                if (mavc_ao.name.Equals("Focused"))
                    aoListVol4.Add(new AudioFocused(audioContr));
                else
                    throw new NotImplementedException();
            }
    }

    static void Main(string[] args)
    {
        bool foundFile = false;

        var def = audioContr.GetAudioDevices();
        audioContr.onOutputAddedCallback((sender, newSession) => { Console.WriteLine("new audio output found!"); UpdateAudioOutputs(); comServer.updateVolumes(); });
        foreach (var dev in def)
        {
            Console.WriteLine(dev.GetName());   
            var apps = dev.GetAudioApps();
            int count = 0;
            foreach (var app in apps)
            {
                if (app.GetName().Equals("Spotify"))
                {
                    count++;
                }
            }
            Console.WriteLine("Spotify Count: " + count);
        }

        Console.WriteLine(SerialPort.GetPortNames());

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
            catch (Exception e){
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(5000);
            }

        }

        while (true) {
            Console.WriteLine("Waiting for hardware to connect.");
            try
            {
                comServer = new COM("COM3", 9600);
                Console.WriteLine("Hardware connected.");
                comServer.OnWordStreamReceive(MavcAgent.interpretWord);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(1000);
            }

            while (1 == 1)
            {
                try
                {
                    //Console.ReadLine();
                    Thread.Sleep(10000);

                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(1000);
                }
            }
        }
    }
}