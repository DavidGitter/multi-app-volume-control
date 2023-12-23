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
        reverseKnob1 = false;
        reverseKnob2 = false;
        reverseKnob3 = false;
        reverseKnob4 = false;
    }

	// volume 1 mappings
	public List<string> namesVol1;

    // volume 2 mappings
    public List<string> namesVol2;

    // volume 3 mappings
    public List<string> namesVol3;

    // volume 4 mappings
    public List<string> namesVol4;

    // reverse knob of volume 1
    public bool reverseKnob1;

    // reverse knob of volume 2
    public bool reverseKnob2;

    // reverse knob of volume 3
    public bool reverseKnob3;

    // reverse knob of volume 4
    public bool reverseKnob4;

    //reverse knob order (Knob 1 <-> Knob 4, Knob 2 <-> Knob 3)
    public bool reverseKnobOrder;
}