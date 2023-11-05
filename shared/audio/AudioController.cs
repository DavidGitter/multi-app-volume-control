using NAudio.CoreAudioApi;
using System.Collections.Generic;

/**
 * The base class to provide access to the windows sound api
 */
class AudioController
{
    // Create an enumerator for audio endpoints
    private MMDeviceEnumerator enumerator;

    public AudioController()
    {
        enumerator = new MMDeviceEnumerator();
    }

    /**
     * Returns an array of all audio devices currently available (e.g. speaker, headset...)
     * 
     * @returns     a array of the audio devices that are containing the audio apps
     */
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

    /**
     * Returns the current default output device (the one selected in windows)
     */
    public AudioDevice GetDefaultAudioDevice()
    {
        return new AudioDevice(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));
    }

    /**
     * Returns all audio outputs (devices, apps..) as AudioOutput object
     * 
     * @returns     a list of the audio outputs
     */
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

    /**
     * Returns a AudioOutput object by a string name if found
     * 
     * @returns     the audio output object if found, else throws exception
     */
    public AudioOutput GetOutputByName(string name)
    {
        List<AudioOutput> outs = GetAllAudioOutputs();
        return outs.Find(e => e.GetName().Equals(name));
    }
}