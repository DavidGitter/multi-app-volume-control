using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    public static class ThemeColors
    {
        public static class Dark
        {
            // Text colors
            public static readonly Color TextPrimary = Color.FromArgb(185, 187, 190);
            public static readonly Color TextSecondary = Color.FromArgb(148, 149, 156);
            
            // Background colors
            public static readonly Color BgPrimary = Color.FromArgb(32, 34, 37);
            
            // Border colors (for lines/separators)
            public static readonly Color BorderPrimary = Color.FromArgb(130, 131, 139);
            
            // Interactive colors
            public static readonly Color InteractivePrimary = Color.FromArgb(47, 49, 54);
        }

        public static class Light
        {
            // Text colors
            public static readonly Color TextPrimary = SystemColors.ControlText;
            public static readonly Color TextSecondary = SystemColors.ControlDark;
            
            // Background colors
            public static readonly Color BgPrimary = SystemColors.Control;
            
            // Border colors (for lines/separators)
            public static readonly Color BorderPrimary = SystemColors.ControlDark;
            
            // Interactive colors
            public static readonly Color InteractivePrimary = SystemColors.MenuHighlight;
        }

        // Semantic helper methods
        public static Color GetTextPrimary(bool isDark) => isDark ? Dark.TextPrimary : Light.TextPrimary;
        public static Color GetTextSecondary(bool isDark) => isDark ? Dark.TextSecondary : Light.TextSecondary;
        public static Color GetBgPrimary(bool isDark) => isDark ? Dark.BgPrimary : Light.BgPrimary;
        public static Color GetBorderPrimary(bool isDark) => isDark ? Dark.BorderPrimary : Light.BorderPrimary;
        public static Color GetInteractivePrimary(bool isDark) => isDark ? Dark.InteractivePrimary : Light.InteractivePrimary;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void SetTitleBarTheme(IntPtr handle, bool isDark)
        {
            int darkMode = isDark ? 1 : 0;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }

        public static void ApplyTheme(Form form, bool isDark)
        {
            Color backColor = GetBgPrimary(isDark);

            SetTitleBarTheme(form.Handle, isDark);
            form.BackColor = backColor;

            UpdateControlTheme(form, isDark);
        }

        public static void UpdateControlTheme(Control parent, bool isDark)
        {
            Color back = GetBgPrimary(isDark);
            Color text = GetTextPrimary(isDark);
            Color border = GetBorderPrimary(isDark);
            Color groupBoxBorderColor = GetBorderPrimary(isDark);

            foreach (Control c in parent.Controls)
            {
                if (c is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = back;
                    menuStrip.ForeColor = text;
                    menuStrip.Renderer = new ToolStripProfessionalRenderer(new DarkModeColorTable(isDark));
                    foreach (ToolStripMenuItem menuItem in menuStrip.Items)
                    {
                        ApplyThemeToMenuItem(menuItem, isDark);
                    }
                }
                else if (c is ComboBox combo)
                {
                    combo.BackColor = back;
                    combo.ForeColor = text;
                    combo.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (c is ListBox)
                {
                    c.BackColor = back;
                    c.ForeColor = text;
                }
                else if (c is TextBox txt)
                {
                    txt.BackColor = back;
                    txt.ForeColor = text;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = back;
                    btn.ForeColor = text;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = border;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = text;
                    chk.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (c is Label)
                {
                    c.ForeColor = text;
                }
                else if (c is GroupBox gb)
                {
                    gb.ForeColor = groupBoxBorderColor;
                    gb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is Panel || c is TabControl || c is TabPage)
                {
                    c.BackColor = back;
                    c.ForeColor = text;
                }

                if (c.HasChildren) UpdateControlTheme(c, isDark);
            }
        }

        private static void ApplyThemeToMenuItem(ToolStripMenuItem item, bool isDark)
        {
            Color backColor = GetBgPrimary(isDark);
            Color textColor = GetTextPrimary(isDark);

            item.BackColor = backColor;
            item.ForeColor = textColor;
            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                subItem.BackColor = backColor;
                subItem.ForeColor = textColor;
                if (subItem is ToolStripMenuItem subMenuItem)
                {
                    ApplyThemeToMenuItem(subMenuItem, isDark);
                }
            }
        }

        private class DarkModeColorTable : ProfessionalColorTable
        {
            private readonly bool _isDark;

            public DarkModeColorTable(bool isDark) => _isDark = isDark;

            public override Color MenuItemSelected => GetInteractivePrimary(_isDark);
            public override Color MenuItemSelectedGradientBegin => GetInteractivePrimary(_isDark);
            public override Color MenuItemSelectedGradientEnd => GetInteractivePrimary(_isDark);
            public override Color MenuItemBorder => GetBorderPrimary(_isDark);
            public override Color MenuBorder => GetBorderPrimary(_isDark);
            public override Color MenuItemPressedGradientBegin => GetBgPrimary(_isDark);
            public override Color MenuItemPressedGradientEnd => GetBgPrimary(_isDark);
            public override Color ImageMarginGradientBegin => GetBgPrimary(_isDark);
            public override Color ImageMarginGradientMiddle => GetBgPrimary(_isDark);
            public override Color ImageMarginGradientEnd => GetBgPrimary(_isDark);
            public override Color ToolStripDropDownBackground => GetBgPrimary(_isDark);
        }
    }
}
