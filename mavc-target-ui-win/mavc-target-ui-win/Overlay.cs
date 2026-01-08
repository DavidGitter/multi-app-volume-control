using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace mavc_target_ui_win
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using Octokit;
    using System.Diagnostics;
    using Newtonsoft.Json.Linq;

    namespace mavc_target_ui_win
    {

        public class Overlay : Form
        {
            public int volume = 75; // 0° to 360°

            int barHeight = 0;
            int barWidth;

            int screenX;
            int screenY;

            Rectangle barArea;

            public Overlay()
            {
                // window properties
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                DoubleBuffered = true;

                // transparency
                BackColor = Color.Lime;
                TransparencyKey = Color.Lime;

                Bounds = Screen.PrimaryScreen.Bounds;

                StartPosition = FormStartPosition.Manual;

                TopMost = true;  // overlay is always on top

                Rectangle screen = Screen.PrimaryScreen.Bounds;

                barHeight = screen.Height / 3;
                barWidth = Math.Max(12, screen.Width / 60);

                screenX = 10; // at left screen border
                screenY = (screen.Height - barHeight) / 2; ;

                barArea = new Rectangle(screenX, screenY, barWidth, barHeight);
            }

            // Click-Through aktivieren
            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_TRANSPARENT = 0x20;
                    const int WS_EX_LAYERED = 0x80000;

                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
                    return cp;
                }
            }

            class ScreenSize
            {
                public ScreenSize(int width, int height)
                {
                    this.width = width;
                    this.height = height;
                }

                public int width;
                public int height;
            };

            ScreenSize GetScreenSize()
            {
                Screen current = Screen.FromControl(this);
                return new ScreenSize(current.Bounds.Width, current.Bounds.Height);
            }

            // Zeichnen
            protected override void OnPaint(PaintEventArgs e)
            {
                // TODO: fit bar size factor accoring to screen diameter (9:21, 4:3...)
                int barWidth = GetScreenSize().width / 30;
                int barHeight = GetScreenSize().height / 10;

                base.OnPaint(e);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Hintergrund-Balken
                using (Brush bg = new SolidBrush(Color.FromArgb(238, 244, 237)))
                {
                    e.Graphics.FillRectangle(bg, barArea);
                }

                // Berechnung der Füllhöhe (von unten!)
                int filledHeight = (barArea.Height * volume) / 100;

                Rectangle fillRect = new Rectangle(
                    barArea.X,
                    barArea.Bottom - filledHeight,
                    barArea.Width,
                    filledHeight
                );

                // Vordergrund-Balken
                using (Brush fg = new SolidBrush(Color.FromArgb(19, 49, 92)))
                {
                    e.Graphics.FillRectangle(fg, fillRect);
                }

                // percent number in the bar mid
                string text = volume + "%";
                using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
                using (Brush textBrush = Brushes.White)
                {
                    SizeF textSize = e.Graphics.MeasureString(text, font);

                    float textX = barArea.X + (barArea.Width - textSize.Width) / 2;
                    float textY = barArea.Y + (barArea.Height - textSize.Height) / 2;

                    // leichter Schatten für bessere Lesbarkeit
                    e.Graphics.DrawString(
                        text,
                        font,
                        Brushes.Black,
                        textX + 1,
                        textY + 1
                    );

                    e.Graphics.DrawString(
                        text,
                        font,
                        textBrush,
                        textX,
                        textY
                    );
                }
            }

            // Verhindert Fokus
            protected override bool ShowWithoutActivation => true;

            // ESC zum Beenden (optional)
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                if (keyData == Keys.Escape)
                {
                    Close();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
           int X, int Y, int cx, int cy, uint uFlags);

            static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOACTIVATE = 0x0010;

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                // Overlay dauerhaft oben halten
                SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }
    }
}
