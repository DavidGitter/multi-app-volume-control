using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
        foreach (var device in GetAudioDevices())
        {
            outputs.Add((AudioOutput)device);
        }
        outputs.AddRange(GetAllAudioApps());
        return outputs;
	}

    /**
     * Returns all audio applications bundeling there related AudioStreams
     * 
     * @returns     a list of the audio apps
     */
    public List<AudioApp> GetAllAudioApps()
    {
        List<AudioApp> apps = new List<AudioApp>();
        List<AudioStream> streams = new List<AudioStream>();
        foreach (var device in GetAudioDevices())
        {
            // add device
            foreach (AudioStream stream in device.GetAudioStreams())
                streams.Add(stream);
        }

        List<List<AudioStream>> sessionGroups = streams.GroupBy(x => x.GetName())
                                                              .Select(g => g.ToList())
                                                              .ToList();

        foreach (List<AudioStream> las in sessionGroups)
        {
            try
            {
                apps.Add(new AudioApp(las));
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldnt get audio output:\n" + e.StackTrace);
            }
        }

        return apps;
    }

    /**
     * Returns the first AudioOutput object by a string name if found
     * 
     * @returns     the audio output object if found, else throws exception
     */
    public AudioOutput GetOutputByName(string name)
    {
        List<AudioOutput> outs = GetAllAudioOutputs();
        AudioOutput res = outs.Find(e => e.GetName().Equals(name));
        if (res == null)
            throw new KeyNotFoundException("no audio output with name " + name + " found");
        return res;
    }

    /**
     * Returns all AudioOutput objects by a string name if found
     * 
     * @returns     the audio output object if found, else throws exception
     */
    public List<AudioOutput> GetOutputsByName(string name)
    {
        List<AudioOutput> outs = GetAllAudioOutputs();
        List<AudioOutput> aos = new List<AudioOutput> ();
        foreach(var outp in outs)
        {
            if (outp.GetName().Equals(name))
            {
                aos.Add(outp);
            }
        }
        return aos;
    }

    /**
     * gets called when a output-added event occures
     */
    public void onOutputAddedCallback(Action<object, IAudioSessionControl> callback)
    {
        //TODO: for calling update audios when new application is opend so it is also used by the mixer 

        var devices = GetAudioDevices();
        foreach(var dev in devices)
        {
            dev.OnOutputCreated(callback);
        }
    }
}