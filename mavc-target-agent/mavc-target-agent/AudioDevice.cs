using NAudio.CoreAudioApi;

class AudioDevice
{
    private MMDevice mmd;

    public AudioDevice(MMDevice mmd)
    {
        this.mmd = mmd;
    }

    public String GetName()
    {
        return mmd.FriendlyName;
    }

    public AudioApp[] GetAudioApps()
    {
        List<AudioApp> audioApps = new List<AudioApp>();

        // Get the audio session manager for the endpoint
        var sessionManager = mmd.AudioSessionManager;

        // Get the audio session control collection
        var sessionCollection = sessionManager.Sessions;

        //Collect Apps related to device 
        for (int i = 0; i < sessionCollection.Count; i++)
        {
            var asc = sessionCollection[i];
            audioApps.Add(new AudioApp(asc, this));
        }

        return audioApps.ToArray(); 
    }

    public void SetMasterVolume(float volume)
    {
        mmd.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
    }

    public float GetMasterVolume()
    {
        return mmd.AudioEndpointVolume.MasterVolumeLevelScalar;
    }

    public override string ToString()
    {
        return "AuioDevice: " + GetName() + ", AudioApps: " + GetAudioApps().Length;
    }
}
