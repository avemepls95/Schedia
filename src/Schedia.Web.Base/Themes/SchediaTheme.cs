using MudBlazor;

namespace Schedia.Web.Base.Themes;

public static class SchediaTheme
{
    public static MudTheme Theme => new()
    {
        PaletteLight = new PaletteLight
        {
            // Основные цвета
            Primary = "#6366F1",              // Indigo-500
            PrimaryContrastText = "#FFFFFF",
            PrimaryDarken = "#4F46E5",        // Indigo-600
            PrimaryLighten = "#818CF8",       // Indigo-400

            Secondary = "#EC4899",             // Pink-500
            SecondaryContrastText = "#FFFFFF",
            SecondaryDarken = "#DB2777",       // Pink-600
            SecondaryLighten = "#F472B6",      // Pink-400

            Tertiary = "#14B8A6",              // Teal-500
            TertiaryContrastText = "#FFFFFF",
            TertiaryDarken = "#0D9488",        // Teal-600
            TertiaryLighten = "#2DD4BF",       // Teal-400

            // Семантические цвета
            Success = "#22C55E",               // Green-500
            SuccessContrastText = "#FFFFFF",
            Warning = "#F59E0B",               // Amber-500
            WarningContrastText = "#FFFFFF",
            Error = "#EF4444",                 // Red-500
            ErrorContrastText = "#FFFFFF",
            Info = "#3B82F6",                  // Blue-500
            InfoContrastText = "#FFFFFF",

            // Фоны
            Background = "#FAFAFA",            // Neutral-50
            BackgroundGray = "#F5F5F5",        // Neutral-100
            Surface = "#FFFFFF",

            // AppBar и Drawer
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1E293B",            // Slate-800
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",            // Slate-700
            DrawerIcon = "#64748B",            // Slate-500

            // Текст
            TextPrimary = "#0F172A",           // Slate-900
            TextSecondary = "#475569",         // Slate-600
            TextDisabled = "#94A3B8",          // Slate-400

            // Действия
            ActionDefault = "#64748B",         // Slate-500
            ActionDisabled = "#CBD5E1",        // Slate-300
            ActionDisabledBackground = "#F1F5F9", // Slate-100

            // Разделители
            Divider = "#E2E8F0",               // Slate-200
            DividerLight = "#F1F5F9",          // Slate-100

            // Hover/Ripple
            HoverOpacity = 0.06,
            RippleOpacity = 0.1,
            RippleOpacitySecondary = 0.1
        },

        PaletteDark = new PaletteDark
        {
            // Основные цвета (более яркие для тёмной темы)
            Primary = "#818CF8",               // Indigo-400
            PrimaryContrastText = "#0F172A",
            PrimaryDarken = "#6366F1",         // Indigo-500
            PrimaryLighten = "#A5B4FC",        // Indigo-300

            Secondary = "#F472B6",              // Pink-400
            SecondaryContrastText = "#0F172A",

            Tertiary = "#2DD4BF",               // Teal-400
            TertiaryContrastText = "#0F172A",

            // Семантические цвета
            Success = "#4ADE80",                // Green-400
            Warning = "#FBBF24",                // Amber-400
            Error = "#F87171",                  // Red-400
            Info = "#60A5FA",                   // Blue-400

            // Фоны
            Background = "#0F172A",             // Slate-900
            BackgroundGray = "#1E293B",         // Slate-800
            Surface = "#1E293B",                // Slate-800

            // AppBar и Drawer
            AppbarBackground = "#1E293B",
            AppbarText = "#F8FAFC",             // Slate-50
            DrawerBackground = "#1E293B",
            DrawerText = "#E2E8F0",             // Slate-200
            DrawerIcon = "#94A3B8",             // Slate-400

            // Текст
            TextPrimary = "#F8FAFC",            // Slate-50
            TextSecondary = "#94A3B8",          // Slate-400
            TextDisabled = "#64748B",           // Slate-500

            // Действия
            ActionDefault = "#94A3B8",
            ActionDisabled = "#475569",
            ActionDisabledBackground = "#334155",

            // Разделители
            Divider = "#334155",                // Slate-700
            DividerLight = "#1E293B",           // Slate-800

            HoverOpacity = 0.08,
            RippleOpacity = 0.12,
            RippleOpacitySecondary = 0.12
        },

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "system-ui", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "sans-serif"],
                FontSize = "0.9375rem",        // 15px
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H1 = new H1Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "2.5rem",
                FontWeight = "700",
                LineHeight = "1.2",
                LetterSpacing = "-0.02em"
            },
            H2 = new H2Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "2rem",
                FontWeight = "700",
                LineHeight = "1.25",
                LetterSpacing = "-0.015em"
            },
            H3 = new H3Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1.75rem",
                FontWeight = "600",
                LineHeight = "1.3",
                LetterSpacing = "-0.01em"
            },
            H4 = new H4Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1.5rem",
                FontWeight = "600",
                LineHeight = "1.35",
                LetterSpacing = "-0.01em"
            },
            H5 = new H5Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1.25rem",
                FontWeight = "600",
                LineHeight = "1.4"
            },
            H6 = new H6Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1.125rem",
                FontWeight = "600",
                LineHeight = "1.45"
            },
            Subtitle1 = new Subtitle1Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1rem",
                FontWeight = "500",
                LineHeight = "1.5"
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "500",
                LineHeight = "1.5"
            },
            Body1 = new Body1Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "1rem",
                FontWeight = "400",
                LineHeight = "1.6"
            },
            Body2 = new Body2Typography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.55"
            },
            Button = new ButtonTypography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "0.975rem",
                FontWeight = "500",
                LineHeight = "1.5",
                LetterSpacing = "0",
                TextTransform = "none" // Убираем UPPERCASE
            },
            Caption = new CaptionTypography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "0.75rem",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            Overline = new OverlineTypography
            {
                FontFamily = ["Inter", "system-ui", "sans-serif"],
                FontSize = "0.625rem",
                FontWeight = "500",
                LineHeight = "1.5",
                LetterSpacing = "0.05em",
                TextTransform = "uppercase"
            }
        },

        LayoutProperties = new LayoutProperties
        {
            // Более современные скругления
            DefaultBorderRadius = "12px",

            // Drawer
            DrawerWidthLeft = "280px",
            DrawerWidthRight = "280px",
            DrawerMiniWidthLeft = "64px",
            DrawerMiniWidthRight = "64px",

            // AppBar
            AppbarHeight = "64px"
        },

        Shadows = new Shadow
        {
            Elevation =
            [
                "none",
                "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
                "0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.1)",
                "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)",
                "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)",
                "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)"
            ]
        }
    };
}