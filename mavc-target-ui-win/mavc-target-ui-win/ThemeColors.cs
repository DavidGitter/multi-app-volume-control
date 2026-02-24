using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    /**
     * Provides a 5-color dark/light palette and applies it recursively to every
     * control on a Form.  Includes owner-draw logic for rounded GroupBox borders,
     * rounded Button regions, and dark-mode ComboBox items.
     */
    public static class ThemeColors
    {
        // ================== DARK PALETTE ==================

        /**
         * Dark-mode color tokens.
         */
        public static class Dark
        {
            /** Main background color (form, containers). */
            public static readonly Color Base = Color.FromArgb(30, 30, 30);
            /** Raised-surface color (inputs, lists, buttons at rest). */
            public static readonly Color Surface = Color.FromArgb(45, 45, 45);
            /** Border / separator / pressed-state color. */
            public static readonly Color Border = Color.FromArgb(80, 80, 80);
            /** Mouse-over highlight color. */
            public static readonly Color Hover = Color.FromArgb(65, 65, 65);
            /** Primary text color. */
            public static readonly Color Text = Color.FromArgb(212, 212, 212);
        }

        // ================== LIGHT PALETTE (system defaults) ==================

        /**
         * Light-mode color tokens (delegates to SystemColors).
         */
        public static class Light
        {
            /** Main background color. */
            public static readonly Color Base = SystemColors.Control;
            /** Input / list / button surface color. */
            public static readonly Color Surface = SystemColors.Window;
            /** Border color. */
            public static readonly Color Border = SystemColors.ControlDark;
            /** Mouse-over highlight color. */
            public static readonly Color Hover = Color.FromArgb(210, 225, 245);
            /** Primary text color. */
            public static readonly Color Text = SystemColors.ControlText;
        }

        // ================== SEMANTIC ACCESS ==================

        /** Returns colors for the requested theme. */
        public static Color Base(bool dark) => dark ? Dark.Base : Light.Base;

        public static Color Surface(bool dark) => dark ? Dark.Surface : Light.Surface;

        public static Color Border(bool dark) => dark ? Dark.Border : Light.Border;

        public static Color Hover(bool dark) => dark ? Dark.Hover : Light.Hover;

        public static Color Text(bool dark) => dark ? Dark.Text : Light.Text;

        /** Shared corner radius (px) for Win11-style rounding. */
        private const int Radius = 8;

        // ================== CACHED GDI OBJECTS (dark mode) ==================

        private static readonly SolidBrush _darkSurfaceBrush = new SolidBrush(Dark.Surface);
        private static readonly SolidBrush _darkHoverBrush = new SolidBrush(Dark.Hover);
        private static readonly SolidBrush _darkTextBrush = new SolidBrush(Dark.Text);
        private static readonly SolidBrush _darkBaseBrush = new SolidBrush(Dark.Base);
        private static readonly Pen _darkBorderPen = new Pen(Dark.Border, 1);

        // ================== TITLE BAR HELPER ==================

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /**
         * Sets the Windows title-bar to dark or light mode via DWM.
         *
         * @param handle  the window handle (Form.Handle)
         * @param isDark  true to enable immersive dark mode on the title bar
         */
        public static void SetTitleBarTheme(IntPtr handle, bool isDark)
        {
            int darkMode = isDark ? 1 : 0;
            try { DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)); } catch { }
        }

        // ================== ROUNDED RECT HELPER ==================

        /**
         * Creates a GraphicsPath describing a rectangle with uniformly rounded corners.
         *
         * @param r       the bounding rectangle
         * @param radius  corner radius in pixels
         * @return a closed GraphicsPath with four rounded corners
         */
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ================== MAIN APPLY LOGIC ==================

        /**
         * Applies the dark or light theme to an entire form and all its child controls.
         *
         * @param form    the target form
         * @param isDark  true for dark mode, false for light mode
         */
        public static void ApplyTheme(Form form, bool isDark)
        {
            form.SuspendLayout();
            try
            {
                form.BackColor = Base(isDark);
                form.ForeColor = Text(isDark);
                SetTitleBarTheme(form.Handle, isDark);
                UpdateControlTheme(form, isDark);
            }
            finally
            {
                form.ResumeLayout(true);
                form.Invalidate(true);
            }
        }

        /**
         * Recursively themes every child control of the given parent.
         *
         * Handles menus, combo boxes, list boxes, text boxes, buttons,
         * checkboxes, group boxes, labels, and generic containers.
         *
         * @param parent  the parent control whose children will be themed
         * @param isDark  true for dark mode, false for light mode
         */
        public static void UpdateControlTheme(Control parent, bool isDark)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = Base(isDark);
                    menuStrip.ForeColor = Text(isDark);
                    menuStrip.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(isDark));
                    UpdateMenuItems(menuStrip.Items, isDark);
                }
                else if (c is ContextMenuStrip contextMenu)
                {
                    contextMenu.BackColor = Base(isDark);
                    contextMenu.ForeColor = Text(isDark);
                    contextMenu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(isDark));
                    UpdateMenuItems(contextMenu.Items, isDark);
                }
                else if (c is ComboBox combo)
                {
                    combo.DrawItem -= ComboBox_DrawItem;
                    if (isDark)
                    {
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.DrawMode = DrawMode.OwnerDrawFixed;
                        combo.BackColor = Dark.Surface;
                        combo.ForeColor = Dark.Text;
                        combo.DrawItem += ComboBox_DrawItem;
                        ComboBoxPainter.Attach(combo);
                    }
                    else
                    {
                        ComboBoxPainter.Detach(combo);
                        combo.DrawMode = DrawMode.Normal;
                        combo.BackColor = Light.Surface;
                        combo.ForeColor = Light.Text;
                        combo.FlatStyle = FlatStyle.Standard;
                    }
                }
                else if (c is ListBox list)
                {
                    list.BackColor = Surface(isDark);
                    list.ForeColor = Text(isDark);
                    list.BorderStyle = isDark ? BorderStyle.None : BorderStyle.FixedSingle;
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = Surface(isDark);
                    nud.ForeColor = Text(isDark);
                }
                else if (c is TextBox txt)
                {
                    txt.BackColor = Surface(isDark);
                    txt.ForeColor = Text(isDark);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is Button btn)
                {
                    btn.Paint -= Button_Paint;
                    btn.Resize -= Control_SetRoundRegion;
                    if (isDark)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.BackColor = Surface(isDark);
                        btn.ForeColor = Text(isDark);
                        btn.FlatAppearance.MouseOverBackColor = Hover(isDark);
                        btn.FlatAppearance.MouseDownBackColor = Border(isDark);
                        btn.Paint += Button_Paint;
                        btn.Resize += Control_SetRoundRegion;
                        SetRoundRegion(btn, Radius);
                    }
                    else
                    {
                        btn.FlatStyle = FlatStyle.Standard;
                        btn.BackColor = Light.Base;
                        btn.ForeColor = Light.Text;
                        btn.Region = null;
                        btn.UseVisualStyleBackColor = true;
                    }
                }
                else if (c is CheckBox chk)
                {
                    chk.BackColor = Base(isDark);
                    chk.ForeColor = Text(isDark);
                    chk.Paint -= CheckBox_Paint;
                    if (isDark)
                    {
                        chk.FlatStyle = FlatStyle.Standard;
                        chk.Appearance = Appearance.Normal;
                        chk.Paint += CheckBox_Paint;
                    }
                    else
                    {
                        chk.FlatStyle = FlatStyle.Standard;
                    }
                }
                else if (c is GroupBox gb)
                {
                    gb.Paint -= GroupBox_Paint;
                    if (isDark)
                    {
                        gb.ForeColor = Text(isDark);
                        gb.Paint += GroupBox_Paint;
                    }
                    else
                    {
                        gb.ForeColor = Text(isDark);
                    }
                    gb.Invalidate();
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = Text(isDark);
                }
                else if (c is TableLayoutPanel || c is Panel || c is TabControl || c is TabPage)
                {
                    c.BackColor = Base(isDark);
                    c.ForeColor = Text(isDark);
                }

                if (c.HasChildren)
                    UpdateControlTheme(c, isDark);

                if (c.ContextMenuStrip != null)
                    UpdateControlTheme(c.ContextMenuStrip, isDark);
            }
        }

        // ================== ROUNDED REGION HELPERS ==================

        /**
         * Clips the control to a rounded rectangle so its background and hover
         * states follow the rounded shape.
         *
         * @param c       the control to clip
         * @param radius  corner radius in pixels
         */
        private static void SetRoundRegion(Control c, int radius)
        {
            if (c.Width > 0 && c.Height > 0)
            {
                using (var path = RoundedRect(new Rectangle(0, 0, c.Width, c.Height), radius))
                    c.Region = new Region(path);
            }
        }

        /**
         * Resize event handler that re-applies the rounded region after layout changes.
         *
         * @param sender  the control that was resized
         * @param e       event arguments (unused)
         */
        private static void Control_SetRoundRegion(object sender, EventArgs e)
        {
            if (sender is Control c) SetRoundRegion(c, Radius);
        }

        // ================== BUTTON PAINT (rounded border) ==================

        /**
         * Paint handler that draws a rounded border on a Button.
         *
         * @param sender  the button being painted
         * @param e       paint event arguments containing the Graphics surface
         */
        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
            using (var path = RoundedRect(rect, Radius))
                g.DrawPath(_darkBorderPen, path);
        }

        // ================== CHECKBOX PAINT (owner-drawn for dark mode) ==================

        /**
         * Paint handler that draws a custom checkbox in dark mode.
         *
         * Draws a box with the Border color, fills it with an accent blue when
         * checked, and draws a white checkmark on top.
         *
         * @param sender  the CheckBox being painted
         * @param e       paint event arguments containing the Graphics surface
         */
        private static void CheckBox_Paint(object sender, PaintEventArgs e)
        {
            var chk = (CheckBox)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(chk.BackColor);

            int boxSize = 14;
            int boxY = (chk.Height - boxSize) / 2;
            var boxRect = new Rectangle(1, boxY, boxSize, boxSize);

            if (chk.Checked)
            {
                using (var fill = new SolidBrush(Color.FromArgb(60, 130, 210)))
                    g.FillRectangle(fill, boxRect);
                using (var pen = new Pen(Color.FromArgb(80, 150, 230), 1))
                    g.DrawRectangle(pen, boxRect);

                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawLine(pen, boxRect.X + 3, boxRect.Y + boxSize / 2,
                                    boxRect.X + boxSize / 2 - 1, boxRect.Y + boxSize - 4);
                    g.DrawLine(pen, boxRect.X + boxSize / 2 - 1, boxRect.Y + boxSize - 4,
                                    boxRect.X + boxSize - 3, boxRect.Y + 3);
                }
            }
            else
            {
                g.FillRectangle(_darkSurfaceBrush, boxRect);
                g.DrawRectangle(_darkBorderPen, boxRect);
            }

            var textRect = new Rectangle(boxSize + 6, 0, chk.Width - boxSize - 6, chk.Height);
            TextRenderer.DrawText(g, chk.Text, chk.Font, textRect, Dark.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        // ================== GROUPBOX PAINT (rounded border + title) ==================

        /**
         * Paint handler that owner-draws a GroupBox with a rounded border
         * and properly colored title text.
         *
         * @param sender  the GroupBox being painted
         * @param e       paint event arguments containing the Graphics surface
         */
        private static void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            var gb = (GroupBox)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isDark = gb.FindForm()?.BackColor == Dark.Base;

            g.Clear(Base(isDark));

            var titleSize = g.MeasureString(gb.Text, gb.Font);
            int titleH = (int)Math.Ceiling(titleSize.Height);
            int titleW = (int)Math.Ceiling(titleSize.Width);
            int titleX = 8;
            int titleY = 0;

            var borderRect = new Rectangle(0, titleH / 2, gb.Width - 1, gb.Height - titleH / 2 - 1);
            using (var pen = new Pen(Border(isDark), 1))
            using (var path = RoundedRect(borderRect, Radius))
                g.DrawPath(pen, path);

            using (var bgBrush = new SolidBrush(Base(isDark)))
                g.FillRectangle(bgBrush, titleX - 2, titleY, titleW + 4, titleH);

            using (var textBrush = new SolidBrush(Text(isDark)))
                g.DrawString(gb.Text, gb.Font, textBrush, titleX, titleY);
        }

        // ================== MENU ITEM RECURSION ==================

        /**
         * Recursively themes all items in a ToolStripItemCollection.
         *
         * @param items   the collection of menu items to theme
         * @param isDark  true for dark mode, false for light mode
         */
        private static void UpdateMenuItems(ToolStripItemCollection items, bool isDark)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripControlHost host)
                {
                    host.BackColor = Surface(isDark);
                    host.ForeColor = Text(isDark);
                    if (host.Control != null)
                    {
                        host.Control.BackColor = Surface(isDark);
                        host.Control.ForeColor = Text(isDark);
                        if (host.Control is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else
                {
                    item.BackColor = Base(isDark);
                    item.ForeColor = Text(isDark);
                }

                if (item is ToolStripDropDownItem dropDownItem && dropDownItem.HasDropDownItems)
                    UpdateMenuItems(dropDownItem.DropDownItems, isDark);
            }
        }

        // ================== CUSTOM COMBOBOX DRAWING ==================

        /**
         * NativeWindow subclass that fully owner-draws a ComboBox in dark mode.
         *
         * Takes complete control of WM_PAINT via BeginPaint/EndPaint so the default
         * white rendering never reaches the screen, and intercepts WM_ERASEBKGND to
         * prevent background flash.  Uses cached GDI objects to minimize allocations.
         */
        private class ComboBoxPainter : NativeWindow
        {
            private const int WM_PAINT = 0x000F;
            private const int WM_ERASEBKGND = 0x0014;
            private readonly ComboBox _combo;

            [StructLayout(LayoutKind.Sequential)]
            private struct PAINTSTRUCT
            {
                public IntPtr hdc;
                public bool fErase;
                public RECT rcPaint;
                public bool fRestore;
                public bool fIncUpdate;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
                public byte[] rgbReserved;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left, Top, Right, Bottom;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

            [DllImport("user32.dll")]
            private static extern bool EndPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);

            private ComboBoxPainter(ComboBox combo)
            {
                _combo = combo;
                AssignHandle(combo.Handle);
                combo.HandleDestroyed += OnHandleDestroyed;
            }

            private void OnHandleDestroyed(object sender, EventArgs e)
            {
                ReleaseHandle();
            }

            /**
             * Handles WM_PAINT and WM_ERASEBKGND for dark-mode ComboBox rendering.
             *
             * Uses cached brushes/pens and TextRenderer for fast text output.
             *
             * @param m  the Windows message to process
             */
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_ERASEBKGND)
                {
                    m.Result = (IntPtr)1;
                    return;
                }

                if (m.Msg == WM_PAINT)
                {
                    PAINTSTRUCT ps;
                    IntPtr hdc = BeginPaint(m.HWnd, out ps);
                    try
                    {
                        using (var g = Graphics.FromHdc(hdc))
                        {
                            int w = _combo.Width;
                            int h = _combo.Height;
                            int btnWidth = SystemInformation.VerticalScrollBarWidth;

                            g.FillRectangle(_darkSurfaceBrush, 0, 0, w, h);

                            var textRect = new Rectangle(3, 0, w - btnWidth - 6, h);
                            string text = _combo.SelectedItem?.ToString() ?? "";
                            TextRenderer.DrawText(g, text, _combo.Font, textRect, Dark.Text,
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

                            int arrowSize = 4;
                            int btnX = w - btnWidth;
                            int arrowX = btnX + (btnWidth - arrowSize * 2) / 2;
                            int arrowY = (h - arrowSize) / 2;
                            var arrowPoints = new Point[]
                            {
                                new Point(arrowX, arrowY),
                                new Point(arrowX + arrowSize * 2, arrowY),
                                new Point(arrowX + arrowSize, arrowY + arrowSize)
                            };
                            g.FillPolygon(_darkTextBrush, arrowPoints);

                            g.DrawRectangle(_darkBorderPen, 0, 0, w - 1, h - 1);
                        }
                    }
                    finally
                    {
                        EndPaint(m.HWnd, ref ps);
                    }
                    m.Result = IntPtr.Zero;
                    return;
                }

                base.WndProc(ref m);
            }

            private static readonly System.Collections.Generic.Dictionary<ComboBox, ComboBoxPainter> _painters
                = new System.Collections.Generic.Dictionary<ComboBox, ComboBoxPainter>();

            /**
             * Attaches a ComboBoxPainter to the given ComboBox.
             * @param combo  the ComboBox to attach dark-mode painting to
             */
            public static void Attach(ComboBox combo)
            {
                if (_painters.ContainsKey(combo)) return;
                if (!combo.IsHandleCreated)
                {
                    // Defer until the handle exists
                    combo.HandleCreated += (s, e) =>
                    {
                        if (!_painters.ContainsKey(combo))
                        {
                            _painters[combo] = new ComboBoxPainter(combo);
                            combo.Invalidate();
                        }
                    };
                    return;
                }
                _painters[combo] = new ComboBoxPainter(combo);
                combo.Invalidate();
            }

            /**
             * Detaches the ComboBoxPainter from the given ComboBox.
             * @param combo  the ComboBox to detach dark-mode painting from
             */
            public static void Detach(ComboBox combo)
            {
                if (_painters.TryGetValue(combo, out var painter))
                {
                    painter._combo.HandleDestroyed -= painter.OnHandleDestroyed;
                    painter.ReleaseHandle();
                    _painters.Remove(combo);
                    combo.Invalidate();
                }
            }
        }

        /**
         * Owner-draw handler for ComboBox dropdown items in dark mode.
         *
         * Uses TextRenderer for fast GDI text output and cached brushes for
         * background fills.
         *
         * @param sender  the ComboBox whose item is being drawn
         * @param e       draw-item event arguments (index, bounds, state)
         */
        private static void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ComboBox combo = sender as ComboBox;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            e.Graphics.FillRectangle(selected ? _darkHoverBrush : _darkSurfaceBrush, e.Bounds);

            string text = combo.Items[e.Index].ToString();
            TextRenderer.DrawText(e.Graphics, text, combo.Font, e.Bounds, Dark.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        // ================== MENU RENDERER ==================

        /**
         * Custom ProfessionalColorTable that feeds theme-aware colors into the
         * ToolStripProfessionalRenderer for menus.
         */
        public class ThemeColorTable : ProfessionalColorTable
        {
            private readonly bool _dark;

            /**
             * Creates a new ThemeColorTable.
             *
             * @param dark  true for dark-mode colors, false for light-mode
             */
            public ThemeColorTable(bool dark) { _dark = dark; UseSystemColors = false; }

            public override Color MenuItemSelected => Hover(_dark);
            public override Color MenuItemSelectedGradientBegin => Hover(_dark);
            public override Color MenuItemSelectedGradientEnd => Hover(_dark);
            public override Color MenuItemPressedGradientBegin => Border(_dark);
            public override Color MenuItemPressedGradientEnd => Border(_dark);

            public override Color ToolStripDropDownBackground => Base(_dark);
            public override Color MenuStripGradientBegin => Base(_dark);
            public override Color MenuStripGradientEnd => Base(_dark);

            public override Color MenuBorder => Border(_dark);
            public override Color MenuItemBorder => Border(_dark);
            public override Color ToolStripBorder => Border(_dark);

            public override Color ImageMarginGradientBegin => Base(_dark);
            public override Color ImageMarginGradientMiddle => Base(_dark);
            public override Color ImageMarginGradientEnd => Base(_dark);

            public override Color SeparatorDark => Border(_dark);
            public override Color SeparatorLight => Border(_dark);
        }
    }
}