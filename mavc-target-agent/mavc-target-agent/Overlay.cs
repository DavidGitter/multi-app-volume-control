using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Overlay : Form
{
    #region Fields

    // Background rectangle opacity (0..255)
    private byte _backgroundOpacity = 180;

    // Simple update gate: prevents calling UpdateLayeredWindow too frequently.
    private const int UpdateOverlayAfterMs = 40;    // 25 FPS (match esp32 delay)
    private static readonly System.Timers.Timer GateTimer = new(UpdateOverlayAfterMs);
    private static bool _gateOpen = true;

    // Overlay state: "Overlay ready" until first real update arrives from the agent.
    private bool _hasData = false;
    private string _label = "";
    private int _value = 0;

    // Optional auto-hide after updates.
    private bool _autoHideActive;
    private readonly int _autoHideAfterSec;
    private CancellationTokenSource _hideCts;

    // WS_EX_LAYERED enables per-pixel alpha; WS_EX_TRANSPARENT makes it click-through.
    private const int WS_EX_TRANSPARENT = 0x20, WS_EX_LAYERED = 0x80000, WS_EX_TOOLWINDOW = 0x80;

    // Keep window above other windows without stealing focus.
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOMOVE = 0x0002, SWP_NOSIZE = 0x0001, SWP_NOACTIVATE = 0x0010;

    // UpdateLayeredWindow with ULW_ALPHA uses BLENDFUNCTION/alpha channel.
    private const int ULW_ALPHA = 0x00000002;
    private const byte AC_SRC_OVER = 0x00, AC_SRC_ALPHA = 0x01;

    // 32bpp DIB section settings.
    private const uint BI_RGB = 0, DIB_RGB_COLORS = 0;

    #endregion

    #region Constructor

    /**
     * Creates the fullscreen overlay window.
     *
     * @param autoHideAfterSecs  Seconds to wait before hiding after an update (if auto-hide is enabled).
     */
    public Overlay(int autoHideAfterSecs)
    {
        _autoHideAfterSec = autoHideAfterSecs;

        // Borderless, not in taskbar, topmost overlay.
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;

        // Keep for Barchart option later
        BackColor = Color.Black;

        // Small badge window instead of fullscreen
        StartPosition = FormStartPosition.Manual;
        Size = new Size(320, 60);   // adjustable later if needed


        // Re-open update gate periodically.
        GateTimer.Elapsed += (_, _) => _gateOpen = true;
        GateTimer.Start();
    }

    #endregion

    #region Public Methods

    public void SetAutoHideActive(bool active) => _autoHideActive = active;

    public void SetBackgroundOpacity(byte opacity)
    {
        _backgroundOpacity = opacity;
        RenderAndApplyLayer();
    }
    public void SetOverlayPosition(int x, int y)
    {
        // Move the overlay window itself
        Left = x;
        Top = y;

        if (IsHandleCreated)
        {
            BeginInvoke(new Action(() =>
            {
                Show();
                RenderAndApplyLayer();
            }));
        }
    }

    /**
     * Updates the overlay text and value and schedules auto-hide (if enabled).
     *
     * @param label  Text shown before the numeric value (e.g., "Knob 1").
     * @param value  Value shown after the label.
     */
    public void setUpdatedVolume(string label, int value)
    {
        // Throttle paint calls to reduce flicker/CPU when the knob sends many values quickly.
        if (!_gateOpen) return;
        _gateOpen = false;

        _hasData = true;
        _label = label;
        _value = value;

        // Ensure UI work runs on the UI thread.
        BeginInvoke((Action)(() =>
        {
            Show();
            Update();

            // Re-assert topmost without activating/focusing the window.
            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

            // Draw into a bitmap and push to the layered window.
            RenderAndApplyLayer();
        }));

        // Cancel previous hide timer and start a new one after each update.
        if (_autoHideActive)
        {
            _hideCts?.Cancel();
            _hideCts = new CancellationTokenSource();
            AutoHideAsync(_autoHideAfterSec, _hideCts.Token);
        }
    }

    #endregion

    #region Protected Overrides

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;

            // Layered + transparent (click-through) + toolwindow (hides from Alt-Tab).
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    // Avoid stealing focus when shown.
    protected override bool ShowWithoutActivation => true;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Ensure always-on-top immediately after showing.
        SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

        // Push initial "Ready" bitmap.
        RenderAndApplyLayer();
    }


    #endregion

    #region Private Methods

    private async void AutoHideAsync(int sec, CancellationToken token)
    {
        try
        {
            await Task.Delay(sec * 1000, token);
            BeginInvoke((Action)(Hide));
        }
        catch (TaskCanceledException) { }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    private void RenderAndApplyLayer()
    {
        if (!IsHandleCreated) return;

        // Draw into a 32bpp ARGB bitmap; then upload the full bitmap to Windows.
        using var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        g.Clear(Color.Transparent);
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawText(g);
        ApplyBitmapToLayeredWindow(bmp);
    }

    private void DrawText(Graphics g)
    {
        // "Ready" until knob data, then "Knob X: N".
        string text = _hasData ? $"{_label}: {_value}" : "Ready";

        // Use a Windows UI font; bold improves legibility over games/video.
        using var font = new Font("Segoe UI", 16f, FontStyle.Bold);

        // Measure to size the rounded background.
        SizeF size = g.MeasureString(text, font);

        float x = 10f, y = 10f, padX = 6f, padY = 3f, radius = 8f;
        RectangleF bgRect = new(x - padX, y - padY, size.Width + padX * 2, size.Height + padY * 2);

        // Draw translucent dark rounded rectangle behind the text.
        using (var bg = new SolidBrush(Color.FromArgb(_backgroundOpacity, 20, 20, 20)))
        using (var path = RoundedRect(bgRect, radius))
            g.FillPath(bg, path);

        // Draw the foreground text (green).
        using var fg = new SolidBrush(Color.FromArgb(255, 3, 159, 2));
        g.DrawString(text, font, fg, x, y);
    }

    private static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        float r = Math.Max(0, radius);
        float d = Math.Min(Math.Min(rect.Width, rect.Height), r * 2);
        var arc = new RectangleF(rect.X, rect.Y, d, d);

        var path = new GraphicsPath();
        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - d; path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - d; path.AddArc(arc, 0, 90);
        arc.X = rect.Left; path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void ApplyBitmapToLayeredWindow(Bitmap bitmap)
    {
        // GDI handles/objects for UpdateLayeredWindow.
        IntPtr screenDc = IntPtr.Zero, memDc = IntPtr.Zero, hDib = IntPtr.Zero, oldBmp = IntPtr.Zero, pBits = IntPtr.Zero;

        try
        {
            // Screen DC + memory DC for the source bitmap.
            screenDc = GetDC(IntPtr.Zero);
            memDc = CreateCompatibleDC(screenDc);

            // Create a 32bpp DIB section so we can write pixels directly.
            var bmi = CreateBmi(bitmap.Width, bitmap.Height);
            hDib = CreateDIBSection(memDc, ref bmi, DIB_RGB_COLORS, out pBits, IntPtr.Zero, 0);
            if (hDib == IntPtr.Zero || pBits == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "CreateDIBSection failed.");

            // Select the DIB into the memory DC.
            oldBmp = SelectObject(memDc, hDib);

            // UpdateLayeredWindow expects premultiplied alpha when using AC_SRC_ALPHA.
            CopyPremultiplied(bitmap, pBits);

            // Position/size info for the window and bitmap source.
            var size = new SIZE(bitmap.Width, bitmap.Height);
            var src = new POINT(0, 0);
            var dst = new POINT(Left, Top);

            // Per-pixel alpha blend. ULW_ALPHA tells Windows to use this blend function.
            var blend = new BLENDFUNCTION
            {
                BlendOp = AC_SRC_OVER,
                BlendFlags = 0,
                SourceConstantAlpha = 255,   // keep per-pixel alpha "as is"
                AlphaFormat = AC_SRC_ALPHA   // use the alpha channel
            };

            if (!UpdateLayeredWindow(Handle, screenDc, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "UpdateLayeredWindow failed.");
        }
        finally
        {
            // Restore and free GDI resources (avoid leaks).
            if (oldBmp != IntPtr.Zero) SelectObject(memDc, oldBmp);
            if (hDib != IntPtr.Zero) DeleteObject(hDib);
            if (memDc != IntPtr.Zero) DeleteDC(memDc);
            if (screenDc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    private static BITMAPINFO CreateBmi(int width, int height) => new BITMAPINFO
    {
        bmiHeader = new BITMAPINFOHEADER
        {
            biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth = width,
            biHeight = -height, // negative => top-down DIB
            biPlanes = 1,
            biBitCount = 32,
            biCompression = BI_RGB,
            biSizeImage = (uint)(width * height * 4)
        },
        bmiColors = new uint[1]
    };

    private static void CopyPremultiplied(Bitmap src, IntPtr dstBits)
    {
        // Copy ARGB pixels out of GDI+ bitmap, premultiply RGB by alpha, then write into DIB bits.
        Rectangle rect = new Rectangle(0, 0, src.Width, src.Height);
        BitmapData data = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            int w = src.Width, h = src.Height;
            int srcStride = data.Stride, dstStride = w * 4;

            byte[] s = new byte[srcStride * h];
            Marshal.Copy(data.Scan0, s, 0, s.Length);

            byte[] d = new byte[dstStride * h];

            for (int y = 0; y < h; y++)
            {
                int sRow = y * srcStride, dRow = y * dstStride;
                for (int x = 0; x < w; x++)
                {
                    int si = sRow + x * 4, di = dRow + x * 4;

                    // Format32bppArgb is BGRA in memory.
                    byte b = s[si + 0], g = s[si + 1], r = s[si + 2], a = s[si + 3];

                    d[di + 0] = (byte)((b * a) / 255);
                    d[di + 1] = (byte)((g * a) / 255);
                    d[di + 2] = (byte)((r * a) / 255);
                    d[di + 3] = a;
                }
            }

            Marshal.Copy(d, 0, dstBits, d.Length);
        }
        finally
        {
            src.UnlockBits(data);
        }
    }

    #endregion

    #region Interop

    // Win32 / GDI interop
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset); [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X, Y; public POINT(int x, int y) { X = x; Y = y; } }
    [StructLayout(LayoutKind.Sequential)] private struct SIZE { public int cx, cy; public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; } }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
    [StructLayout(LayoutKind.Sequential)] private struct BITMAPINFO { public BITMAPINFOHEADER bmiHeader; [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public uint[] bmiColors; }
    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize; public int biWidth, biHeight; public ushort biPlanes, biBitCount;
        public uint biCompression, biSizeImage; public int biXPelsPerMeter, biYPelsPerMeter;
        public uint biClrUsed, biClrImportant;
    }

    #endregion

    #region Parked: Bar-chart overlay (keep for later)

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
    #endregion
}

