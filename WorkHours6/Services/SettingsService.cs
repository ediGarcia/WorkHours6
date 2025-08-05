using HelperMethods;
using System.Windows.Media;
using WorkHours6.Models;

#pragma warning disable CS8601
#pragma warning disable CS8600
#pragma warning disable CS8618

namespace WorkHours6.Services;

public static class SettingsService
{
    #region Properties

    /// <summary>
    /// Gets the current theme's accent brush.
    /// </summary>
    public static Brush ThemeAccentBrush { get; }

    /// <summary>
    /// Gets the current theme's brush.
    /// </summary>
    public static Brush ThemeBrush { get; }

    /// <summary>
    /// Gets the main window height.
    /// </summary>
    public static double WindowHeight => 100;

    /// <summary>
    /// Gets the main window width.
    /// </summary>
    public static double WindowWidth => 300;

    /// <summary>
    /// Gets the application user settings.
    /// </summary>
    public static UserSettings UserSettings { get; set;  } = new();

    #endregion

    static SettingsService()
    {
        (string ThemeBrush, string ThemeAccentBrush)[] themes =
        [
            ("#ecee81", "#9fa115"), // Yellow
            ("#89f3e4", "#11ac96"), // Green/Blue
            ("#82a4ff", "#0034c0"), // Blue
            ("#edb7ed", "#a829a8"), // Light Purple
            ("#ef9595", "#a81919"), // Red
            ("#efb495", "#a84a19"), // Dark Orange
            ("#94a684", "#49563e"), // Gray Green
            ("#fff3da", "#ec9f00"), // Orange
            ("#c4dfdf", "#498787"), // Light Blue
            ("#eeeeee", "#777777"), // Light Gray
            ("#ddffbb", "#6edd00"), // Light Green
            ("#e8a0bf", "#9d2659"), // Dark Pink
            ("#867070", "#433838"), // Brown
            ("#84b4ff", "#004bc1"), // Light Blue
            ("#b9f3fc", "#08b6d1"), // Very Light Blue
            ("#eae0da", "#906951"), // Brown
            ("#e8f3d6", "#81b033") // Light Green
        ];

        int selectedThemeIndex = NumberMethods.GetRandomInt(0, themes.Length);
        BrushConverter converter = new();

        ThemeBrush = (Brush)converter.ConvertFromString(themes[selectedThemeIndex].ThemeBrush);
        ThemeAccentBrush = (Brush)converter.ConvertFromString(themes[selectedThemeIndex].ThemeAccentBrush);
    }
}