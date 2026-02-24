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

    #region Volume Mappings

    // number of knobs / volume lists
    public int numberOfKnobs;

    // volume mappings per knob (index-based)
    public List<List<AudioOutput>> volumeMappings;

    // reverse knob flags per knob (index-based)
    public List<bool> reverseKnobs;

    
    #endregion

    #region Knob Settings

    // reverse knob order (Knob 1 <-> Knob N, etc.)
    public bool reverseKnobOrder;

    #endregion

    #region UI Settings

    // enable debug mode
    public bool enableDebugMode;

    // dark mode status
    public bool darkMode;

    // minimize to tray on close
    public bool minimizeOnClose;

    // start the ui minimized
    public bool startMinimized;

    #endregion

    #region Overlay Settings

    // enable the screen overlay
    public bool enableScreenOverlay;

    // enable the auto hide for the overlay
    public bool activateAutoHide;

    // defines the seconds the auto hide waits until it hides
    public int autoHideAfterSec;

    // overlay position X
    public int overlayX;

    // overlay position Y
    public int overlayY;

    #endregion

    #region Window Settings

    // window width (0 = use default)
    public int windowWidth;

    // window height (0 = use default)
    public int windowHeight;

    // window position X (int.MinValue = use default)
    public int windowX = int.MinValue;

    // window position Y (int.MinValue = use default)
    public int windowY = int.MinValue;

    #endregion

    public MAVCSave()
    {
        numberOfKnobs = 4;
        volumeMappings = new List<List<AudioOutput>>();
        reverseKnobs = new List<bool>();
        reverseKnobOrder = false;
        enableDebugMode = false;
        darkMode = false;
        minimizeOnClose = false;
        startMinimized = false;
        enableScreenOverlay = false;
        activateAutoHide = true;
        autoHideAfterSec = 1;
        overlayX = 10;
        overlayY = 10;
        windowWidth = 0;
        windowHeight = 0;
    }

    /**
     * Ensures volumeMappings and reverseKnobs lists have at least numberOfKnobs entries.
     */
    public void EnsureCapacity()
    {
        if (numberOfKnobs < 1)
            numberOfKnobs = 4;

        if (volumeMappings == null)
            volumeMappings = new List<List<AudioOutput>>();
        if (reverseKnobs == null)
            reverseKnobs = new List<bool>();

        while (volumeMappings.Count < numberOfKnobs)
            volumeMappings.Add(new List<AudioOutput>());
        while (reverseKnobs.Count < numberOfKnobs)
            reverseKnobs.Add(false);
    }

    /**
     * Sets the number of knobs and resizes volumeMappings / reverseKnobs accordingly.
     * Excess entries are trimmed (mappings for removed knobs are lost).
     *
     * @param count  desired number of knobs (minimum 1)
     */
    public void SetNumberOfKnobs(int count)
    {
        if (count < 1) count = 1;
        numberOfKnobs = count;

        while (volumeMappings.Count < numberOfKnobs)
            volumeMappings.Add(new List<AudioOutput>());
        while (reverseKnobs.Count < numberOfKnobs)
            reverseKnobs.Add(false);

        // Trim excess (data is lost for removed knobs)
        while (volumeMappings.Count > numberOfKnobs)
            volumeMappings.RemoveAt(volumeMappings.Count - 1);
        while (reverseKnobs.Count > numberOfKnobs)
            reverseKnobs.RemoveAt(reverseKnobs.Count - 1);
    }

    public static MAVCSave LoadConfigFromFile(string configLoadPath, string configSavePath)
    {
        try
        {
            if (System.IO.File.Exists(configLoadPath))
            {
                // load from config file
                string json = System.IO.File.ReadAllText(configLoadPath);
                MAVCSave loadedMavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);
                if (loadedMavcSave != null)
                {
                    loadedMavcSave.EnsureCapacity();
                    return loadedMavcSave;
                }
                throw new FileLoadException("Could not load config file " + configLoadPath);
            }
            else
            {
                throw new FileLoadException("File " + configLoadPath + " not existing.");
            }
        }
        catch
        {
            throw new FileLoadException("Could not load or create config file " + configLoadPath);
        }
    }
}