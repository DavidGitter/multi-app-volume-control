

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
    // the focused audio app -> NULL WHEN NOT A AUDIO APP
    private AudioApp aa;
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
        this.aa = GetFocusedAudioApp();
    }

    public override bool available()
    {
        try
        {
            if (aa == null)
                return false;
            var asc = aa.getSessionController();
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
        AudioApp focused = GetFocusedAudioApp();
        return focused != null ? focused.GetVolume() : -1;
    }

    public override void SetVolume(float volume)
    {
        AudioApp focused = GetFocusedAudioApp();
        if(focused != null)
            focused.SetVolume(volume);
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

    private AudioApp GetFocusedAudioApp()
    {
        try
        {
            int newestPid = GetFocusedProcessId();
            if (pid != newestPid)
            {
                pid = newestPid;
                aa = (AudioApp)ac.GetOutputByName(Process.GetProcessById(GetFocusedProcessId()).ProcessName);
            }
            return aa;
        }
        catch (KeyNotFoundException knfe)
        {
            Console.WriteLine(knfe.Message);
            return null;
        }
    }
}