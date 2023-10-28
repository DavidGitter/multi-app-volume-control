using NAudio.CoreAudioApi;
using System.Diagnostics;

class AudioController
{
    // Create an enumerator for audio endpoints
    private MMDeviceEnumerator enumerator;

    public AudioController()
    {
        enumerator = new MMDeviceEnumerator();
    }

    public AudioDevice[] GetAudioDevices()
    {

        List<AudioDevice> audioDevices = new List<AudioDevice>();

        // Collect audio playback devices
        foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            audioDevices.Add(new AudioDevice(endpoint));
        }

        return audioDevices.ToArray();
    }

    public AudioDevice GetDefaultAudioDevice()
    {
        return new AudioDevice(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));
    }
}