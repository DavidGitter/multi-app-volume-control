using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace mavc_target_ui_win
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9;

        public static void NotifyExistingInstance()
        {
            var current = Process.GetCurrentProcess();
            IntPtr hWnd = FindWindow(null, "MAVC");
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    internal static class Program
    {

        private static Mutex appRunningMutex;

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //check if application already running, if so open the already existing
            bool createdNew;
            appRunningMutex = new Mutex(true, "MavcApplicationRunningMutex", out createdNew);

            if (!createdNew)
            {
                // App läuft bereits
                NativeMethods.NotifyExistingInstance();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
