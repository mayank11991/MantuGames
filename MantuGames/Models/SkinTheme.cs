namespace MantuGames.Models;

/// <summary>
/// The full look of a premium skin beyond its backdrop colours: title font, dialog
/// style, a five-colour palette for game pieces, and scenery art. Free skins have no
/// theme and keep the app's original look. Colours are hex strings (MAUI-free).
/// </summary>
public sealed record SkinTheme(
    string TitleFont,          // registered font alias for titles and big labels
    string DialogBackground,
    string DialogBorder,
    double DialogBorderWidth,
    double DialogCornerRadius,
    string DialogShadow,       // hard offset shadow colour under dialogs
    string[] Palette,          // five colours used by the games, most distinct first
    string SceneTop,           // image across the top of every backdrop
    string SceneBottom)        // image along the bottom (hills, skyline, waves...)
{
    /// <summary>Palette colour <paramref name="i"/>, wrapping round.</summary>
    public string Color(int i) => Palette[((i % Palette.Length) + Palette.Length) % Palette.Length];
}

public static class SkinThemes
{
    // Title font aliases registered in MauiProgram (all SIL Open Font License).
    public const string Lilita = "TitleLilita";
    public const string Anton = "TitleAnton";
    public const string Orbitron = "TitleOrbitron";
    public const string Madimi = "TitleMadimi";
    public const string Baloo = "TitleBaloo";

    /// <summary>Themes keyed by skin id. Skins not listed keep the original look.</summary>
    public static readonly IReadOnlyDictionary<string, SkinTheme> ById = new Dictionary<string, SkinTheme>
    {
        // Builder Bob — construction site: safety orange, asphalt, hazard stripes.
        ["lion"] = new(Anton, "#FFF8EE", "#2B2B30", 4, 12, "#FFC83D",
            new[] { "#FF8A1F", "#FFD23F", "#4A4E57", "#D9483B", "#3E7CB1" },
            "scene_top_lion.png", "scene_bottom_lion.png"),

        // Brave Knight — royal castle: parchment dialogs, gold trim.
        ["panda"] = new(Lilita, "#FFF4DC", "#B8862B", 4, 18, "#5B3E8C",
            new[] { "#6A4BC4", "#E0B23A", "#2E8B57", "#C0392B", "#9AA5B1" },
            "scene_top_panda.png", "scene_bottom_panda.png"),

        // Professor Pip — science lab: clean white, dashed-lab teal.
        ["owl"] = new(Baloo, "#FFFFFF", "#1F9E7A", 3, 22, "#9BE3C3",
            new[] { "#2FA66A", "#3A8DDE", "#E5484D", "#F5C518", "#8E5BD9" },
            "scene_top_owl.png", "scene_bottom_owl.png"),

        // DJ Riya — candy party: bubbly pink, glowing.
        ["octopus"] = new(Madimi, "#FFF0F7", "#E8458B", 3, 36, "#B98CFF",
            new[] { "#FF4FA3", "#B07CFF", "#3DDBB0", "#FFE14D", "#4FB8FF" },
            "scene_top_octopus.png", "scene_bottom_octopus.png"),

        // Captain Patch — pirate island: sea, sand and wood.
        ["dino"] = new(Lilita, "#FFF3D6", "#7A4A22", 4, 14, "#2BA6C9",
            new[] { "#2BA6C9", "#E8B86B", "#FF6F59", "#4CAF50", "#8B5A2B" },
            "scene_top_dino.png", "scene_bottom_dino.png"),

        // Officer Ollie — city patrol: police blue, siren red, taxi yellow.
        ["bee"] = new(Anton, "#FFFFFF", "#1F3F99", 4, 16, "#E53935",
            new[] { "#1F4FBF", "#E53935", "#FFC107", "#607D8B", "#43A047" },
            "scene_top_bee.png", "scene_bottom_bee.png"),

        // Zib the Alien — outer space: cosmic violet, neon.
        ["sheep"] = new(Baloo, "#F6F1FF", "#7B4DFF", 3, 26, "#28D7F5",
            new[] { "#8A5BFF", "#28D7F5", "#9BE13C", "#FF4FD8", "#FFD84D" },
            "scene_top_sheep.png", "scene_bottom_sheep.png"),

        // Explorer Ella — rainforest: jungle teal and leaf green.
        ["sloth"] = new(Madimi, "#F2FFF7", "#1F8A70", 4, 28, "#7ACB5A",
            new[] { "#1F9E93", "#62C044", "#FF6FA8", "#FFC93C", "#8B5E3C" },
            "scene_top_sloth.png", "scene_bottom_sloth.png"),

        // Robo Buddy — race-track tech: electric blue and chrome.
        ["elephant"] = new(Anton, "#F3F6FA", "#3A4A63", 3, 10, "#4C6FFF",
            new[] { "#2F6BFF", "#9AA6B8", "#FF3B30", "#E6FF3B", "#1E2533" },
            "scene_top_elephant.png", "scene_bottom_elephant.png"),

        // Boss Sara — winter festival: icy lilac, holly red, pine green.
        ["butterfly"] = new(Baloo, "#FFFFFF", "#A35CFF", 3, 30, "#7FD3FF",
            new[] { "#9B59F5", "#E53950", "#2E9E5B", "#F2B84B", "#5BC0F0" },
            "scene_top_butterfly.png", "scene_bottom_butterfly.png"),
    };

    public static SkinTheme? For(Skin skin) => ById.TryGetValue(skin.Id, out var t) ? t : null;
}
