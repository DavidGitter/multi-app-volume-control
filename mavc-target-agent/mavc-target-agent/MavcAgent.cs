using static COM;


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

    /**
     * Function that interprets the words receiveds of the mixer
     * 
     * @param word  the word to be interpreted (see COM class)
     */
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
                    AudioDevice deflDev = audioContr.GetDefaultAudioDevice();
                    deflDev.SetVolume(argNum/100);
                    break;
                }
            default:
                throw new InvalidDataException();
        }
    }

    static void Main(string[] args)
    {
        while (true) {
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