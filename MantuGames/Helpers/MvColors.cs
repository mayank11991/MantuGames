namespace MantuGames.Helpers;

public static class MvColors
{
    // ── Monument Valley palette ──────────────────────────────────
    // Reference: ustwo games — soft, dreamy, architectural pastels

    // Backgrounds
    public const string BgLight = "#FBF4ED";       // warm cream
    public const string BgDark  = "#2D2A3D";       // deep muted purple-navy
    public const string SurfaceLight = "#FFFFFF";
    public const string SurfaceDark  = "#3A364A";

    // Text
    public const string TextPrimaryLight = "#4A3728";
    public const string TextPrimaryDark  = "#F0E6D8";
    public const string TextSecondaryLight = "#8B7B6A";
    public const string TextSecondaryDark  = "#B0A696";

    // Accents
    public const string Rose   = "#E8B4B4";
    public const string Teal   = "#8ECCCC";
    public const string Sand   = "#DCC8A8";
    public const string Lavender = "#C4A4D4";
    public const string Sage   = "#A8C4A0";
    public const string Gold   = "#D4B896";

    // UI
    public const string BorderLight = "#DCC8A8";
    public const string BorderDark  = "#5A5670";
    public const string Overlay     = "#882D2A3D";

    // Gradient helpers
    public static readonly (string, string) DashboardGradientDark  = ("#2D2A3D", "#3A364A");
    public static readonly (string, string) DashboardGradientLight = ("#FBF4ED", "#F5EDE4");
}
