using NAudio.CoreAudioApi;
using System.Diagnostics;

class AudioApp
{
    private AudioSessionControl asc;
    private AudioDevice audioDevice;

    public AudioApp(AudioSessionControl asc, AudioDevice audioDevice)
    {
        this.asc = asc;
        this.audioDevice = audioDevice;   
    }

    public String GetName()
    {
        return Process.GetProcessById((int)asc.GetProcessID).ProcessName;
    }

    public AudioDevice GetIODevice()
    {
        return audioDevice;
    }

    public override string ToString()
    {
        return "AudioDevice: " + GetName() + ", Related AudioDevice: " + GetIODevice();
    }

    public void SetVolume(float volume)
    {
        asc.SimpleAudioVolume.Volume = volume;
    }

    public float GetVolume()
    {
        return asc.SimpleAudioVolume.Volume;
    }

    public String GetIconPath()
    {
        return asc.IconPath;
    }
}