using NAudio.CoreAudioApi;
using System.Collections.Generic;

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
	
	public List<AudioOutput> GetAllAudioOutputs() {
		List<AudioOutput> outputs = new List<AudioOutput>();        
        foreach(var device in GetAudioDevices())
        {
            // add device
            outputs.Add((AudioOutput)device);
            foreach(var app in device.GetAudioApps())
            {
                outputs.Add((AudioOutput)app);
            }
        }
        return outputs;
	}

    public AudioOutput GetOutputByName(string name)
    {
        List<AudioOutput> outs = GetAllAudioOutputs();
        return outs.Find(e => e.GetName().Equals(name));
    }
}