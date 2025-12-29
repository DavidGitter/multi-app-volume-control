using System.Drawing;

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
    }
}
