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
    // Background rectangle opacity (0..255)
    private byte _backgroundOpacity = 150;

    // Simple update gate: prevents calling UpdateLayeredWindow too frequently.
    private const int UpdateOverlayAfterMs = 40;
    private static readonly System.Timers.Timer GateTimer = new(UpdateOverlayAfterMs);
    private static bool _gateOpen = true;

    // Overlay state: "Overlay ready" until first real update arrives from the agent.
    private bool _hasData = false;
    private string _label = "Overlay ready";
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

    /**
     * Creates the fullscreen overlay window.
     *
     * <param name="autoHideAfterSecs">Seconds to wait before hiding after an update (if auto-hide is enabled).</param>
     */
    public Overlay(int autoHideAfterSecs)
    {
        _autoHideAfterSec = autoHideAfterSecs;

        // Borderless, not in taskbar, topmost overlay.
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;

        // BackColor is irrelevant for layered updates (we push our own bitmap),
        // but keeping it explicit avoids surprises in other paint paths.
        BackColor = Color.Black;

        // Fullscreen overlay; we render only a small badge inside it.
        Bounds = Screen.PrimaryScreen.Bounds;
        StartPosition = FormStartPosition.Manual;

        // Re-open the update gate periodically.
        GateTimer.Elapsed += (_, _) => _gateOpen = true;
        GateTimer.Start();
    }

    public void SetAutoHideActive(bool active) => _autoHideActive = active;

    public void SetBackgroundOpacity(byte opacity)
    {
        _backgroundOpacity = opacity;
        RenderAndApplyLayer();
    }

    /**
     * Updates the overlay text and value and schedules auto-hide (if enabled).
     *
     * <param name="label">Text shown before the numeric value (e.g., "Knob 1").</param>k
     * <param name="value">Value shown after the label.</param>
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

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;

            // Layered + transparent (click-through) + toolwindow (usually hides from Alt-Tab).
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

    // For layered windows, normal WM_PAINT isn't used because we draw via UpdateLayeredWindow.
    protected override void OnPaint(PaintEventArgs e) { }

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
        // "Ready" until we got actual knob data, then "Knob X: N".
        string text = _hasData ? $"{_label}: {_value}" : "Ready";

        // Use a Windows UI font; bold improves legibility over games/video.
        using var font = new Font("Segoe UI", 16f, FontStyle.Bold);

        // Measure to size the rounded background.
        SizeF size = g.MeasureString(text, font);

        const float x = 10f, y = 10f, padX = 6f, padY = 3f, radius = 8f;
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

    #region Parked: Bar-chart overlay (keep for later)
    // (unchanged, kept for later)
    #endregion

    // Win32 / GDI interop
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

    [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X, Y; public POINT(int x, int y) { X = x; Y = y; } }
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
}
