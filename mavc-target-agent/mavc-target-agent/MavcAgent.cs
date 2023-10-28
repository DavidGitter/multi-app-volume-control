
class MavcAgent
{
    static void Main(string[] args)
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
    }
}