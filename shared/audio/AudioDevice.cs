using NAudio.CoreAudioApi;
using System.Collections.Generic;

class AudioDevice : AudioOutput
{
    private MMDevice mmd;

    public AudioDevice(MMDevice mmd)
    {
        this.mmd = mmd;
    }

    public override string GetName()
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

    public override void SetVolume(float volume)
    {
        mmd.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
    }

    public override float GetVolume()
    {
        return mmd.AudioEndpointVolume.MasterVolumeLevelScalar;
    }

    public override string ToString()
    {
        return "(Device)  " + GetName();
    }
}
