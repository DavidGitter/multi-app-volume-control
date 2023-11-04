using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;

class AudioApp : AudioOutput
{
    private AudioSessionControl asc;
    private AudioDevice audioDevice;

    public AudioApp(AudioSessionControl asc, AudioDevice audioDevice)
    {
        this.asc = asc;
        this.audioDevice = audioDevice;   
    }

    public override string GetName()
    {
        return Process.GetProcessById((int)asc.GetProcessID).ProcessName;
    }

    public AudioDevice GetIODevice()
    {
        return audioDevice;
    }

    public override string ToString()
    {
        return "(App)       " + GetName();
    }

    public override void SetVolume(float volume)
    {
        asc.SimpleAudioVolume.Volume = volume;
    }

    public override float GetVolume()
    {
        return asc.SimpleAudioVolume.Volume;
    }

    public string GetIconPath()
    {
        return asc.IconPath;
    }
}