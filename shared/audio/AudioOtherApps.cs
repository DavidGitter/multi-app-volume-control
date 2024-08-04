using System;
using System.Collections.Generic;
using System.Linq;

class AudioOtherApps : AudioOutput
{
    // the focused audio outputs -> NULL WHEN NOT A AUDIO APP
    private AudioController ac;
    // the ao save to check against for apps that are not selected
    private MAVCSave save;
    // name of the app
    private string name;
    // volume of all other apps not selected in UI
    private float allVolumes;
    // store non selected "other apps"
    private List<AudioApp> otherApps = new List<AudioApp>();

    /**
     * @param asc           the audio controller
     * @param audioDevice   the parent audio device of the application
     */
    public AudioOtherApps(AudioController ac, MAVCSave save)
    {
        this.name = "Other Apps";
        this.ac = ac;
        this.save = save;
        UpdateOtherApps();
    }

    private void UpdateOtherApps()
    {
        List<AudioApp> aos = ac.GetAllAudioApps();
        Console.WriteLine(save.AOsVol1);
        foreach (AudioApp app in aos)
        {
            if( !save.AOsVol1.Any(o => o.name.Equals(app.GetName())) &&
                !save.AOsVol2.Any(o => o.name.Equals(app.GetName())) &&
                !save.AOsVol3.Any(o => o.name.Equals(app.GetName())) &&
                !save.AOsVol4.Any(o => o.name.Equals(app.GetName()))) {
                otherApps.Add(app);
            }
        }

        Console.WriteLine("Other-Apps:\n");

        foreach (AudioApp app in otherApps)
        {
            Console.WriteLine(app.GetName());
        }
    }

    public override bool available()
    {
        try
        {
            if (ac == null)
                return false;
            return true;
        }
        catch (NullReferenceException nre)
        {
            return false;
        }
    }

    public override string GetAudioType()
    {
        return "Function";
    }

    public override string GetName()
    {
        return name;
    }

    public override float GetVolume()
    {
        return allVolumes;
    }

    public override void SetVolume(float volume)
    {
        allVolumes = volume;
        foreach (AudioApp aa in otherApps)
            aa.SetVolume(allVolumes);
    }

    public override string ToString()
    {
        return "(" + GetAudioType() + ")" + "   " + name;
    }
}