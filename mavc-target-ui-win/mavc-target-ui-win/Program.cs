using mavc_target_ui_win.mavc_target_ui_win;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace mavc_target_ui_win
{
    internal static class Program
    {

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Task UI = new Task(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            });

            UI.Start();

            while (Form1.GetMavcSave() == null) // wait till the mavcSave is loaded by the UI
            {
                Thread.Sleep(1000);
            }

            bool screenOverlayEnabled = Form1.GetMavcSave().enableScreenOverlay;

            if (screenOverlayEnabled)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Overlay overlay = new Overlay();
                Application.Run(overlay);
            }

            UI.Wait();
        }
    }
}
