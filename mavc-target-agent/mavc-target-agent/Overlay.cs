using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Text;

public class Overlay : Form
{
    int barHeight = 0;
    int barWidth;

    int screenX;
    int screenY;

    Rectangle barArea;

    private String volName = "Volume";
    private int volValue = 0;

    private bool autoHideActive = false;
    private static int autoHideAfterSec = 1;
    private static bool hideIsActive = false;
    private static bool setbackHide = false;

    public Overlay(int autoHideAfterSecs)
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

    public void SetAutoHideActive(bool autoHideActive)
    {
        this.autoHideActive = autoHideActive;
    }

    private async void AutoHideAsync(int sec)
    {
        hideIsActive = true;
        await Task.Delay(sec * 1000);
        this.Invoke((Action)(() =>
        {
            this.Hide();
            hideIsActive = false;
        }));
    }

    public void setUpdatedVolume(String name,  int volume)
    {
        volName = name;
        volValue = volume;
        this.Invalidate();

        this.Invoke((Action)(() =>
        {
            this.Show();
            this.Update();
        }));

        if(!hideIsActive)
            AutoHideAsync(autoHideAfterSec);
    }

    // activate Click-Through
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

    // Zeichnen
    protected override void OnPaint(PaintEventArgs e)
    {

        // Text-based Overlay
        e.Graphics.TextRenderingHint =
        TextRenderingHint.SingleBitPerPixelGridFit;

        using (Font font = new Font("Consolas", 16, FontStyle.Bold))
        {
            e.Graphics.DrawString(
                volName + ": " + volValue,
                font,
                new SolidBrush(Color.FromArgb(255, 3, 159, 2)),
                10,
                10
            );
        }


        // Bar-Chart-based Overlay

        //// TODO: fit bar size factor accoring to screen diameter (9:21, 4:3...)
        //int barWidth = GetScreenSize().width / 30;
        //int barHeight = GetScreenSize().height / 10;

        //base.OnPaint(e);
        //e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

        //using (Brush bg = new SolidBrush(Color.FromArgb(238, 244, 237)))
        //{
        //    e.Graphics.FillRectangle(bg, barArea);
        //}

        //int filledHeight = (barArea.Height * volValue) / 100;

        //Rectangle fillRect = new Rectangle(
        //    barArea.X,
        //    barArea.Bottom - filledHeight,
        //    barArea.Width,
        //    filledHeight
        //);

        //using (Brush fg = new SolidBrush(Color.FromArgb(19, 49, 92)))
        //{
        //    e.Graphics.FillRectangle(fg, fillRect);
        //}

        //string text = volValue + "%";
        //using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
        //using (Brush textBrush = Brushes.White)
        //{
        //    SizeF textSize = e.Graphics.MeasureString(text, font);

        //    float textX = barArea.X + (barArea.Width - textSize.Width) / 2;
        //    float textY = barArea.Y + (barArea.Height - textSize.Height) / 2;

        //    e.Graphics.DrawString(
        //        text,
        //        font,
        //        Brushes.Black,
        //        textX + 1,
        //        textY + 1
        //    );
        //}
    }

    // prevent focus
    protected override bool ShowWithoutActivation => true;


    // always on top
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