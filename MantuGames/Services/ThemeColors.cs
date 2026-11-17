namespace MantuGames.Services;

/// <summary>
/// Game colours that follow a premium skin's palette. Every call names the game's
/// original colour as the fallback, so free skins look exactly as before.
/// </summary>
public static class ThemeColors
{
    private static Models.SkinTheme? Theme => SkinService.Current.Theme;

    /// <summary>True when the current skin has a premium theme.</summary>
    public static bool Active => Theme != null;

    /// <summary>Palette colour <paramref name="i"/> as hex, or <paramref name="fallback"/>.</summary>
    public static string Hex(int i, string fallback) => Theme?.Color(i) ?? fallback;

    public static Color Get(int i, string fallback) => Color.FromArgb(Hex(i, fallback));

    /// <summary>Palette colour mixed towards white (0 = colour, 1 = white) for soft tiles.</summary>
    public static string Tint(int i, double towardsWhite, string fallback)
    {
        if (Theme is not { } t) return fallback;
        var c = Color.FromArgb(t.Color(i));
        float k = (float)towardsWhite;
        return new Color(c.Red + (1 - c.Red) * k, c.Green + (1 - c.Green) * k, c.Blue + (1 - c.Blue) * k).ToHex();
    }

    /// <summary>A darker shade of palette colour <paramref name="i"/> (for 3D "lips" under buttons).</summary>
    public static string Shade(int i, string fallback)
    {
        if (Theme is not { } t) return fallback;
        var c = Color.FromArgb(t.Color(i));
        return new Color(c.Red * 0.72f, c.Green * 0.72f, c.Blue * 0.72f).ToHex();
    }
}
