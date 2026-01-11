using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    public static class ThemeColors
    {
        // ================== PALETTES ==================

        public static class Dark
        {
            // Text
            public static readonly Color TextPrimary = Color.FromArgb(220, 221, 224);
            public static readonly Color TextSecondary = Color.FromArgb(180, 182, 188);
            public static readonly Color TextHeading = Color.FromArgb(255, 255, 255);

            // Backgrounds
            public static readonly Color BgPrimary = Color.FromArgb(32, 34, 37);
            public static readonly Color BgSecondary = Color.FromArgb(45, 47, 52);
            public static readonly Color BgInput = Color.FromArgb(40, 42, 47);

            // Borders
            public static readonly Color BorderPrimary = Color.FromArgb(70, 72, 78);
            public static readonly Color BorderSecondary = Color.FromArgb(50, 52, 58);

            // Separators
            public static readonly Color SeparatorDark = Color.FromArgb(45, 47, 52);
            public static readonly Color SeparatorLight = Color.FromArgb(55, 57, 62);

            // Interactive
            public static readonly Color InteractivePrimary = Color.FromArgb(55, 57, 63);
            public static readonly Color InteractiveHover = Color.FromArgb(70, 72, 78);
            public static readonly Color InteractivePressed = Color.FromArgb(40, 42, 47);

            // Controls
            public static readonly Color CheckboxBorder = Color.FromArgb(140, 142, 148);
            // Fix: Lighter background for checked state to ensure visibility
            public static readonly Color CheckboxChecked = Color.FromArgb(128, 128, 128);
            public static readonly Color CheckboxText = Color.FromArgb(230, 230, 230);
        }

        public static class Light
        {
            // Text
            public static readonly Color TextPrimary = Color.Black;
            public static readonly Color TextSecondary = Color.FromArgb(64, 64, 64);
            public static readonly Color TextHeading = Color.Black;

            // Backgrounds
            public static readonly Color BgPrimary = SystemColors.Control;
            public static readonly Color BgSecondary = Color.White;
            public static readonly Color BgInput = SystemColors.Window;

            // Borders
            public static readonly Color BorderPrimary = SystemColors.ControlDark;
            public static readonly Color BorderSecondary = SystemColors.ControlLight;

            // Separators
            public static readonly Color SeparatorDark = SystemColors.ControlDark;
            public static readonly Color SeparatorLight = SystemColors.ControlLightLight;

            // Interactive
            public static readonly Color InteractivePrimary = SystemColors.ControlLight;
            public static readonly Color InteractiveHover = Color.FromArgb(210, 225, 245);
            public static readonly Color InteractivePressed = SystemColors.ControlDark;

            // Controls
            public static readonly Color CheckboxBorder = SystemColors.ControlDarkDark;
            public static readonly Color CheckboxChecked = SystemColors.ControlLight;
            public static readonly Color CheckboxText = SystemColors.ControlText;
        }

        // ================== SEMANTIC ACCESS ==================

        public static Color TextPrimary(bool dark) => dark ? Dark.TextPrimary : Light.TextPrimary;
        public static Color TextSecondary(bool dark) => dark ? Dark.TextSecondary : Light.TextSecondary;
        public static Color TextHeading(bool dark) => dark ? Dark.TextHeading : Light.TextHeading;

        public static Color BgPrimary(bool dark) => dark ? Dark.BgPrimary : Light.BgPrimary;
        public static Color BgSecondary(bool dark) => dark ? Dark.BgSecondary : Light.BgSecondary;
        public static Color BgInput(bool dark) => dark ? Dark.BgInput : Light.BgInput;

        public static Color BorderPrimary(bool dark) => dark ? Dark.BorderPrimary : Light.BorderPrimary;
        public static Color InteractiveHover(bool dark) => dark ? Dark.InteractiveHover : Light.InteractiveHover;
        public static Color InteractivePressed(bool dark) => dark ? Dark.InteractivePressed : Light.InteractivePressed;

        public static Color SeparatorDark(bool dark) => dark ? Dark.SeparatorDark : Light.SeparatorDark;
        public static Color SeparatorLight(bool dark) => dark ? Dark.SeparatorLight : Light.SeparatorLight;

        public static Color CheckboxBorder(bool dark) => dark ? Dark.CheckboxBorder : Light.CheckboxBorder;
        public static Color CheckboxChecked(bool dark) => dark ? Dark.CheckboxChecked : Light.CheckboxChecked;
        public static Color CheckboxText(bool dark) => dark ? Dark.CheckboxText : Light.CheckboxText;


        // ================== TITLE BAR HELPER ==================

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void SetTitleBarTheme(IntPtr handle, bool isDark)
        {
            int darkMode = isDark ? 1 : 0;
            try { DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)); } catch { }
        }


        // ================== MAIN APPLY LOGIC ==================

        public static void ApplyTheme(Form form, bool isDark)
        {
            form.SuspendLayout();

            try
            {
                form.BackColor = BgPrimary(isDark);
                form.ForeColor = TextPrimary(isDark);
                SetTitleBarTheme(form.Handle, isDark);
                UpdateControlTheme(form, isDark);
            }
            finally
            {
                form.ResumeLayout(true);
                form.Invalidate(true);
            }
        }

        public static void UpdateControlTheme(Control parent, bool isDark)
        {
            foreach (Control c in parent.Controls)
            {
                // -- MENUS --
                if (c is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = BgPrimary(isDark);
                    menuStrip.ForeColor = TextPrimary(isDark);
                    menuStrip.Renderer = new ToolStripProfessionalRenderer(new DarkModeColorTable(isDark));
                    UpdateMenuItems(menuStrip.Items, isDark);
                }
                else if (c is ContextMenuStrip contextMenu)
                {
                    contextMenu.BackColor = BgPrimary(isDark);
                    contextMenu.ForeColor = TextPrimary(isDark);
                    contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkModeColorTable(isDark));
                    UpdateMenuItems(contextMenu.Items, isDark);
                }
                // -- DROPDOWNS --
                else if (c is ComboBox combo)
                {
                    combo.DrawItem -= ComboBox_DrawItem;

                    if (isDark)
                    {
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.DrawMode = DrawMode.OwnerDrawFixed;
                        combo.BackColor = Dark.BgInput;
                        combo.ForeColor = Dark.TextPrimary;
                        combo.DrawItem += ComboBox_DrawItem;
                    }
                    else
                    {
                        combo.FlatStyle = FlatStyle.Standard;
                        combo.DrawMode = DrawMode.Normal;
                        combo.BackColor = Light.BgInput;
                        combo.ForeColor = Light.TextPrimary;
                    }
                }
                // -- LISTBOXES --
                else if (c is ListBox list)
                {
                    list.BackColor = BgInput(isDark);
                    list.ForeColor = TextPrimary(isDark);
                    list.BorderStyle = BorderStyle.FixedSingle;
                }
                // -- TEXTBOXES --
                else if (c is TextBox txt)
                {
                    txt.BackColor = BgInput(isDark);
                    txt.ForeColor = TextPrimary(isDark);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                // -- BUTTONS --
                else if (c is Button btn)
                {
                    btn.BackColor = BgPrimary(isDark);
                    btn.ForeColor = TextPrimary(isDark);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = BorderPrimary(isDark);
                }
                // -- CHECKBOXES --
                else if (c is CheckBox chk)
                {
                    chk.BackColor = BgPrimary(isDark);
                    chk.ForeColor = CheckboxText(isDark);
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.FlatAppearance.BorderSize = 1;
                    chk.FlatAppearance.BorderColor = CheckboxBorder(isDark);

                    // Fix: Set explicit colors for Checked and Mouse interactions
                    chk.FlatAppearance.CheckedBackColor = CheckboxChecked(isDark);
                    chk.FlatAppearance.MouseDownBackColor = InteractivePressed(isDark);
                    chk.FlatAppearance.MouseOverBackColor = InteractiveHover(isDark);
                }
                // -- GROUPBOXES --
                else if (c is GroupBox gb)
                {
                    gb.ForeColor = TextHeading(isDark);
                    gb.FlatStyle = FlatStyle.Flat;
                }
                // -- LABELS --
                else if (c is Label lbl)
                {
                    lbl.ForeColor = ((lbl.Tag as string) == "heading") ? TextHeading(isDark) : TextPrimary(isDark);
                }
                // -- CONTAINERS --
                else if (c is Panel || c is TabControl || c is TabPage)
                {
                    c.BackColor = BgPrimary(isDark);
                    c.ForeColor = TextPrimary(isDark);
                }

                if (c.HasChildren)
                    UpdateControlTheme(c, isDark);

                if (c.ContextMenuStrip != null)
                    UpdateControlTheme(c.ContextMenuStrip, isDark);
            }
        }

        // ================== MENU ITEM RECURSION ==================

        private static void UpdateMenuItems(ToolStripItemCollection items, bool isDark)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripControlHost host)
                {
                    Color bgColor = BgInput(isDark);
                    Color fgColor = TextPrimary(isDark);

                    host.BackColor = bgColor;
                    host.ForeColor = fgColor;

                    if (host.Control != null)
                    {
                        host.Control.BackColor = bgColor;
                        host.Control.ForeColor = fgColor;
                        if (host.Control is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else
                {
                    item.BackColor = BgPrimary(isDark);
                    item.ForeColor = TextPrimary(isDark);
                }

                if (item is ToolStripDropDownItem dropDownItem && dropDownItem.HasDropDownItems)
                {
                    UpdateMenuItems(dropDownItem.DropDownItems, isDark);
                }
            }
        }

        // ================== CUSTOM DRAWING ==================

        private static void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ComboBox combo = sender as ComboBox;

            Color backgroundColor = Dark.BgInput;
            Color textColor = Dark.TextPrimary;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backgroundColor = Dark.InteractiveHover;
                textColor = Dark.TextHeading;
            }

            using (SolidBrush bgBrush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            string text = combo.Items[e.Index].ToString();
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                float yOffset = (e.Bounds.Height - e.Font.Height) / 2;
                e.Graphics.DrawString(text, combo.Font, textBrush, new PointF(e.Bounds.X + 2, e.Bounds.Y + yOffset));
            }
        }

        // ================== MENU RENDERER ==================

        public class DarkModeColorTable : ProfessionalColorTable
        {
            private readonly bool _dark;
            public DarkModeColorTable(bool dark) { _dark = dark; UseSystemColors = false; }

            public override Color MenuItemSelected => InteractiveHover(_dark);
            public override Color MenuItemSelectedGradientBegin => InteractiveHover(_dark);
            public override Color MenuItemSelectedGradientEnd => InteractiveHover(_dark);
            public override Color MenuItemPressedGradientBegin => InteractivePressed(_dark);
            public override Color MenuItemPressedGradientEnd => InteractivePressed(_dark);

            public override Color ToolStripDropDownBackground => BgPrimary(_dark);
            public override Color MenuStripGradientBegin => BgPrimary(_dark);
            public override Color MenuStripGradientEnd => BgPrimary(_dark);

            public override Color MenuBorder => BorderPrimary(_dark);
            public override Color MenuItemBorder => BorderPrimary(_dark);
            public override Color ToolStripBorder => BorderPrimary(_dark);

            public override Color ImageMarginGradientBegin => BgPrimary(_dark);
            public override Color ImageMarginGradientMiddle => BgPrimary(_dark);
            public override Color ImageMarginGradientEnd => BgPrimary(_dark);

            // Using full class name to avoid naming conflict with static class properties
            public override Color SeparatorDark => ThemeColors.SeparatorDark(_dark);
            public override Color SeparatorLight => ThemeColors.SeparatorLight(_dark);
        }
    }
}