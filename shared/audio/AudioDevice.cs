using NAudio.CoreAudioApi;
using System.Collections.Generic;

/**
 * Represents a audio output device of windows
 */
class AudioDevice : AudioOutput
{
    //naudio object of a device
    private MMDevice mmd;

    /**
     * @param mmd   the naudio audio device
     */
    public AudioDevice(MMDevice mmd)
    {
        this.mmd = mmd;
    }

    /**
     * Returns the name of the audio device
     *
     * @returns the name of the audio device
     */
    public override string GetName()
    {
        return mmd.FriendlyName;
    }

    /**
     * Returns all audio apps that are running and outputing to this device
     *
     * @returns a list of apps using this device
     */
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

    /**
     * Sets the master volume of this device
     *
     * @param volume    a number betwenn 0 and 1
     */
    public override void SetVolume(float volume)
    {
        mmd.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
    }

    /**
     * Returns the master volume of this device
     *
     * @returns     a number betwenn 0 and 1
     */
    public override float GetVolume()
    {
        return mmd.AudioEndpointVolume.MasterVolumeLevelScalar;
    }

    /**
     * Returns a string representation of this object
     *
     * @returns object as string
     */
    public override string ToString()
    {
        return "(Device)  " + GetName();
    }
}
