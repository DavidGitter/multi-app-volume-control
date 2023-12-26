using System;

/**
 * A abstract class for audio outputs (devices, apps)
 */
public abstract class AudioOutput
{
    public struct LastUpdate
    {
        public bool Available;
    }
    private LastUpdate lastUpdate;

    public AudioOutput()
    {
        lastUpdate.Available = available();
    }

    public LastUpdate getLastUpdatedValues()
    {
        return lastUpdate;
    }

    /*
     * Returns the name of the Audio Output
     *
     * @returns     name of the audio output
     */
    abstract public string GetName();

    /*
     * Returns a shortened version of the audio output name
     *
     * @returns     shortned name of the audio output
     */
    public string GetTruncateName(int maxLength)
    {
        return TruncateString(GetName(), maxLength);
    }

    /*
     * Converts the audio output object to a string
     *
     * @return string of the audio output
     */
    public override string ToString()
    {
        return GetTruncateName(30);
    }

    /*
     * Sets the volume of the audio output when not being offline
     * 
     * 
     * @param volume    a number betwenn 1(loud) and 0(none volume)
     */
    abstract public void SetVolume(float volume);

    /*
     * Returns the Type of the Audio
     * e.g.:
     * Speaker                      -> Device
     * Spotify                      -> App
     * Offline Application/Device   -> Offline
     */
    abstract public string GetAudioType();


    /*
     * Returns the current volume of the audio output when not being offline
     * 
     * @returns     a number betwenn 1(loud) and 0(none volume)
     */
    abstract public float GetVolume();

    /*
     * Function to truncate a string
     * 
     * @param input     the string to truncate
     * @returns         the max length of the string after truncating
     */
    private static string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
        {
            return input;
        }
        else
        {
            return "..." + input.Substring(input.Length - maxLength + 3);
        }
    }


    /*
     * Checks if the audio output is available
     *
     * @returns     true if the output is available, otherwise false
     */
    public abstract bool available();
}