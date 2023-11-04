using System.Collections.Generic;

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

	public List<string> namesVol1;
	public List<string> namesVol2;
	public List<string> namesVol3;
	public List<string> namesVol4;
}