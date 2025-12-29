using System.Drawing;

namespace mavc_target_ui_win
{
    public static class ThemeColors
    {
        public static class Dark
        {
            // Main colors
            public static readonly Color Background = Color.FromArgb(32, 34, 37);
            public static readonly Color Text = Color.FromArgb(185, 187, 190);
            
            // Component colors
            public static readonly Color ListBackground = Color.FromArgb(35, 39, 42);
            public static readonly Color ButtonBackground = Color.FromArgb(64, 68, 75);
            public static readonly Color MenuBackground = Color.FromArgb(32, 34, 37);
            public static readonly Color Border = Color.FromArgb(24, 25, 28);
            public static readonly Color Separator = Color.FromArgb(148, 149, 156);
            
            // Hover/Selection colors
            public static readonly Color MenuItemSelected = Color.FromArgb(47, 49, 54);
            public static readonly Color MenuItemPressed = Color.FromArgb(32, 34, 37);
            
            // Title bar (Windows DWM API controls this)
        }

        public static class Light
        {
            // Main colors
            public static readonly Color Background = SystemColors.Control;
            public static readonly Color Text = SystemColors.ControlText;
            
            // Component colors
            public static readonly Color ListBackground = Color.White;
            public static readonly Color ButtonBackground = SystemColors.Control;
            public static readonly Color MenuBackground = SystemColors.Control;
            public static readonly Color Border = SystemColors.ControlDark;
            public static readonly Color Separator = SystemColors.ControlDark;
            
            // Hover/Selection colors
            public static readonly Color MenuItemSelected = SystemColors.MenuHighlight;
            public static readonly Color MenuItemPressed = SystemColors.MenuHighlight;
            
            // Title bar (System default)
        }

        public static Color GetBackground(bool isDark) => isDark ? Dark.Background : Light.Background;
        public static Color GetText(bool isDark) => isDark ? Dark.Text : Light.Text;
        public static Color GetListBackground(bool isDark) => isDark ? Dark.ListBackground : Light.ListBackground;
        public static Color GetButtonBackground(bool isDark) => isDark ? Dark.ButtonBackground : Light.ButtonBackground;
        public static Color GetMenuBackground(bool isDark) => isDark ? Dark.MenuBackground : Light.MenuBackground;
        public static Color GetBorder(bool isDark) => isDark ? Dark.Border : Light.Border;
        public static Color GetSeparator(bool isDark) => isDark ? Dark.Separator : Light.Separator;
        public static Color GetMenuItemSelected(bool isDark) => isDark ? Dark.MenuItemSelected : Light.MenuItemSelected;
        public static Color GetMenuItemPressed(bool isDark) => isDark ? Dark.MenuItemPressed : Light.MenuItemPressed;
    }
}
