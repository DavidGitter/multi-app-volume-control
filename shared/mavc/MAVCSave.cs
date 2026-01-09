using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

/**
 * The file that represents the config for the mavc and that can be serialized to store the config on the disk
 */
[System.Serializable]
public class MAVCSave
{
    public class AudioOutput
    {
        public AudioOutput(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
        public string name;
        public string type;
    }

	public MAVCSave()
	{
        AOsVol1 = new List<AudioOutput>();
        AOsVol2 = new List<AudioOutput>();
        AOsVol3 = new List<AudioOutput>();
        AOsVol4 = new List<AudioOutput>();
        reverseKnob1 = false;
        reverseKnob2 = false;
        reverseKnob3 = false;
        reverseKnob4 = false;
        darkMode = false;
        enableDebugMode = false;
        minimizeOnClose = false;
        enableScreenOverlay = false;
        startMinimized = false;
    }

	// volume 1 mappings
	public List<AudioOutput> AOsVol1;

    // volume 2 mappings
    public List<AudioOutput> AOsVol2;

    // volume 3 mappings
    public List<AudioOutput> AOsVol3;

    // volume 4 mappings
    public List<AudioOutput> AOsVol4;

    // reverse knob of volume 1
    public bool reverseKnob1;

    // reverse knob of volume 2
    public bool reverseKnob2;

    // reverse knob of volume 3
    public bool reverseKnob3;

    // reverse knob of volume 4
    public bool reverseKnob4;

    // reverse knob order (Knob 1 <-> Knob 4, Knob 2 <-> Knob 3)
    public bool reverseKnobOrder;

    // enable debug mode
    public bool enableDebugMode;

    // dark mode status
    public bool darkMode;

    // minimize to tray on close
    public bool minimizeOnClose;

    // enable the screen overlay
    public bool enableScreenOverlay;

    // enable the auto hide for the overlay
    public bool activateAutoHide;

    // defines the seconds the auto hide waits until it hides
    public int autoHideAfterSec;

    public static MAVCSave LoadConfigFromFile(string configLoadPath, string configSavePath)
    {
        try
        {
            if (System.IO.File.Exists(configLoadPath))
            {
                // load from config file
                string json = System.IO.File.ReadAllText(configLoadPath);
                MAVCSave loadedMavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);
                if(loadedMavcSave != null)
                    return loadedMavcSave;
                throw new FileLoadException("Could not load config file " + configLoadPath);
            }
            else
            {
                throw new FileLoadException("File " + configLoadPath + " not exisiting.");
            }
        }
        catch
        {
            throw new FileLoadException("Could not load or create config file " + configLoadPath);
        }
    }
    // start application minimized to tray
    public bool startMinimized;
}