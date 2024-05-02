using NAudio.CoreAudioApi;
using System.Diagnostics;
using System;

class AudioStream : AudioOutput
{
    // the related session controller
    private AudioSessionControl asc;
    // the parent audio deive where the sound gets outputed
    private AudioDevice audioDevice;
    // name of the app session
    private string name;

    /**
     * @param asc           the audio controller
     * @param audioDevice   the parent audio device of the application
     */
    public AudioStream(AudioSessionControl asc, AudioDevice audioDevice)
    {
        this.asc = asc;
        this.audioDevice = audioDevice;
        this.name = Process.GetProcessById((int)asc.GetProcessID).ProcessName;
    }

    /**
     * Returns the name of the application running and outputing sound
     *
     * @returns name of the applications
     */
    public override string GetName()
    {
        return name;
    }

    /**
     * Returns the name of the audio device the sound is outputed to
     *
     * @returns the audio device
     */
    public AudioDevice GetIODevice()
    {
        return audioDevice;
    }

    /**
     * Converts the object to a string
     *
     * @returns name of the applications
     */
    public override string ToString()
    {
        return "(" + GetAudioType() + ")" + "       " + GetName();
    }

    /**
     * Set the volume of the app
     *
     * @params volume   a value for the volume between 0 and 1
     */
    public override void SetVolume(float volume)
    {
        asc.SimpleAudioVolume.Volume = volume;
    }

    /* Type App
     */
    public override string GetAudioType()
    {
        return "Session";
    }

    /**
     * returns the volume of the app
     *
     * @returns a number between 0 and 1
     */
    public override float GetVolume()
    {
        return asc.SimpleAudioVolume.Volume;
    }

    /**
     * Returns the icon of the windows application
     *
     * @returns the path to the icon
     */
    public string GetIconPath()
    {
        return asc.IconPath;
    }

    public override bool available()
    {
        try
        {
            if (asc == null)
                return false;
            return asc.State != NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateExpired;
        }
        catch (NullReferenceException nre)
        {
            return false;
        }
    }

    public AudioSessionControl getSessionController()
    {
        return asc;
    }
}