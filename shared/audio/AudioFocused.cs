using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

class AudioFocused : AudioOutput
{
    // the audio controller
    private AudioController ac;
    // the focused audio outputs -> NULL WHEN NOT A AUDIO APP
    private List<AudioOutput> aos;
    // name of the app
    private string name;
    // pid of the last updated focus
    private int pid;

    /**
     * @param asc           the audio controller
     * @param audioDevice   the parent audio device of the application
     */
    public AudioFocused(AudioController ac)
    {
        this.name = "Focused";
        this.ac = ac;
        this.aos = GetFocusedAudioApp();
    }

    public override bool available()
    {
        try
        {
            if (aos == null || aos[0] == null)
                return false;
            var asc = ((AudioApp)aos[0]).getSessionController();
            if (asc == null)
                return false;
            return asc.State != NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateExpired;
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
        List<AudioOutput> focused = GetFocusedAudioApp();
        if (focused == null || focused.Count == 0)
            return -1;
        float vol = -1;
        foreach(AudioOutput ao in aos)
        {
            AudioApp aa = (AudioApp)ao;
            if (aa.getSessionController().State != NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateInactive && ((AudioApp)ao).getSessionController().State != NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateExpired)
                vol = aa.GetVolume();
        }
        return focused != null ? vol : -1;
    }

    public override void SetVolume(float volume)
    {
        List<AudioOutput> focused = GetFocusedAudioApp();
        if (focused == null || focused.Count == 0)
            return;

        foreach (AudioOutput ao in aos)
        {
            ((AudioApp)ao).SetVolume(volume);
        }
    }

    public override string ToString()
    {
        return "(" + GetAudioType() + ")" + "   " + name;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);


    private static int GetFocusedProcessId()
    {
        IntPtr foregroundWindowHandle = GetForegroundWindow();

        if (foregroundWindowHandle != IntPtr.Zero)
        {
            GetWindowThreadProcessId(foregroundWindowHandle, out int processId);
            return processId;
        }

        return 0;
    }

    private List<AudioOutput> GetFocusedAudioApp()
    {
        try
        {
            int newestPid = GetFocusedProcessId();
            if (pid != newestPid)
            {
                pid = newestPid;
                aos = ac.GetOutputsByName(Process.GetProcessById(GetFocusedProcessId()).ProcessName);
            }
            return aos;
        }
        catch (KeyNotFoundException knfe)
        {
            Console.WriteLine(knfe.Message);
            return null;
        }
    }
}