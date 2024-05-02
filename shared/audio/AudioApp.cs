using System;
using NAudio.CoreAudioApi;
using System.Collections.Generic;

/** 
 * Represents a audio app (e.g. Spotify) that outputs sound to a audio device
 */
class AudioApp : AudioOutput
{
    // bundles all streams that are related to the application
    List<AudioStream> streams;

    /**
     * @param streams   the streams that are related with the application
     */
    public AudioApp(List<AudioStream> streams)
    {
        this.streams = streams;
    }

    /**
     * Returns the name of the application running and outputing sound
     *
     * @returns name of the applications
     */
    public override string GetName()
    {
        return streams[0].GetName();
    }

    /**
     * Returns the name of the audio device the sound is outputed to
     *
     * @returns the audio device
     */
    public AudioDevice GetIODevice()
    {
        return streams[0].GetIODevice();
    }

    /**
     * Converts the object to a string
     *
     * @returns name of the applications
     */
    public override string ToString()
    {
        return "("+GetAudioType()+")" + "       " + GetName();
    }

    /**
     * Set the volume of the app
     *
     * @params volume   a value for the volume between 0 and 1
     */
    public override void SetVolume(float volume)
    {
        foreach (AudioStream stream in streams)
            stream.getSessionController().SimpleAudioVolume.Volume = volume;
    }

    /* Type App
     */
    public override string GetAudioType()
    {
        return "App";
    }

    /**
     * returns the volume of the app
     *
     * @returns a number between 0 and 1
     */
    public override float GetVolume()
    {
        float highest = 0;
        foreach (AudioStream stream in streams)
        {
            float vol = stream.getSessionController().SimpleAudioVolume.Volume;
            highest = vol > highest ? vol : highest;
        }
        return highest;
    }

    /**
     * Returns the icon of the windows application
     *
     * @returns the path to the icon
     */
    public string GetIconPath()
    {
        return streams[0].getSessionController().IconPath;
    }

    /**
     * Returns if the app is available
     *
     * @returns true if the stream is available (active or inactiv but not expired)
     */
    public override bool available()
    {
        try
        {
            if (streams == null || streams[0].getSessionController() == null)
                return false;
            return streams[0].getSessionController().State != NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateExpired;
        }catch(NullReferenceException nre)
        {
            return false;   
        }
    }

    /**
     * Returns the session controller of the stream
     *
     * @returns AudioSessionControl session controller
     */
    public AudioSessionControl getSessionController()
    {
        return streams[0].getSessionController();
    }
}