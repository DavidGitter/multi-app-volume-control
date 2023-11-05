using System.Collections.Generic;

/**
 * The file that represents the config for the mavc and that can be serialized to store the config on the disk
 */
[System.Serializable]
class MAVCSave
{
	public MAVCSave()
	{
		namesVol1 = new List<string>();
		namesVol2 = new List<string>();
		namesVol3 = new List<string>();
		namesVol4 = new List<string>();
	}

	// volume 1 mappings
	public List<string> namesVol1;

    // volume 2 mappings
    public List<string> namesVol2;

    // volume 3 mappings
    public List<string> namesVol3;

    // volume 4 mappings
    public List<string> namesVol4;
}