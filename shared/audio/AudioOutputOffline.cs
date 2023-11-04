
/**
 * A class that serves as a placeholder for audio outputs that are currently offline
 * therefore containing the name of the offline device
 */
class AudioOutputOffline : AudioOutput
{
	private string name;

	public AudioOutputOffline(string name)
	{
		this.name = name;	
	}

	public override string GetName()
	{
        return "(offline)   " + name;
    }

	public override float GetVolume()
	{
        // audio output offline, no funtion
        return -1;
	}

	public override void SetVolume(float volume)
	{
		// audio output offline, no funtion
	}
}