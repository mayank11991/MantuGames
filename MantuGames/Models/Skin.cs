namespace MantuGames.Models;

/// <summary>
/// A look for the whole app: backdrop colours, the character (shown on the home
/// screen and as the maze player; 3D renders of Kenney's CC0 "Mini Characters") and the accent used for game pieces such as
/// the backs of memory cards. Colours are hex strings so this stays MAUI-free.
/// </summary>
public sealed record Skin(
    string Id,
    string Name,
    string Mascot,
    string? ProductId,
    string ShellTop,
    string ShellMid,
    string GameTop,
    string GameMid,
    string GameBottom,
    string Accent)
{
    public bool IsFree => ProductId == null;

    /// <summary>Colour of the primary buttons. The classic skin keeps the original blue.</summary>
    public string Button => Id == SkinCatalog.DefaultId ? "#2F8CFF" : Accent;
}

public static class SkinCatalog
{
    public const string DefaultId = "classic";

    /// <summary>One Google Play product that unlocks every paid skin.</summary>
    public const string BundleProductId = "skins_all";

    public static readonly IReadOnlyList<Skin> All = new[]
    {
        //       id           name                 character           product          shell top  shell mid  game top   game mid   game bottom accent
        new Skin("classic",   "Smart Sam",         "char_male_a.png",   null,            "#8FE3B4", "#DDF8E8", "#6EC6FF", "#BFE8FF", "#E6F8D8", "#7C5CFF"),
        new Skin("penguin",   "Sunny Mia",         "char_female_b.png", null,            "#9FD8FF", "#E3F3FF", "#7FB8FF", "#D6E8FF", "#F2F8FF", "#2F6FD6"),
        new Skin("lion",      "Builder Bob",       "char_worker.png",   "skin_lion",     "#FFC27A", "#FFEBD2", "#FFB36B", "#FFE2BF", "#FFF6E6", "#E8792B"),
        new Skin("panda",     "Brave Knight",      "char_knight.png",   "skin_panda",    "#A8E6A1", "#E6F8E2", "#8BD88A", "#D8F3D3", "#F4FFF0", "#2E3440"),
        new Skin("owl",       "Professor Pip",     "char_male_e.png",   "skin_owl",      "#8CE0B9", "#E0F7EC", "#5FCF9F", "#CFF2E1", "#F0FFF8", "#2FA66A"),
        new Skin("octopus",   "DJ Riya",           "char_female_c.png", "skin_octopus",  "#FFA3CF", "#FFE4F1", "#FF8FC0", "#FFD6EA", "#FFF3F9", "#E8458B"),
        new Skin("dino",      "Captain Patch",     "char_male_g.png",   "skin_dino",     "#C3EE6A", "#EEFBD0", "#A6E04A", "#E2F6BE", "#FAFFEE", "#3BA55C"),
        new Skin("bee",       "Officer Ollie",     "char_male_c.png",   "skin_bee",      "#FFE16B", "#FFF6C9", "#FFD43B", "#FFF0B0", "#FFFBEA", "#F2A900"),
        new Skin("sheep",     "Zib the Alien",     "char_alien.png",    "skin_sheep",    "#C9B8FF", "#EEE8FF", "#B39CFF", "#E4DBFF", "#F8F5FF", "#8A6BEF"),
        new Skin("sloth",     "Explorer Ella",     "char_female_f.png", "skin_sloth",    "#7FE0D6", "#DDF7F4", "#5ACFC3", "#CCF1ED", "#F0FFFD", "#1F9E93"),
        new Skin("elephant",  "Robo Buddy",        "char_robot.png",    "skin_elephant", "#9CC4FF", "#E2EDFF", "#7AA8FF", "#D3E3FF", "#F3F7FF", "#4C6FFF"),
        new Skin("butterfly", "Boss Sara",         "char_female_d.png", "skin_butterfly","#FFA6D1", "#C7B3FF", "#FF9AC8", "#B9A8FF", "#E6FFF4", "#A35CFF"),
    };

    public static Skin Default => All[0];

    /// <summary>Pieces for the classic skin: the original animals.</summary>
    private static readonly string[] ClassicPieces =
    {
        "cat.png", "elephant.png", "lion.png", "owl.png", "butterfly.png",
        "panda.png", "penguin.png", "sheep.png", "sloth.png", "dino.png",
        "hedge.png", "crab.png", "chick.png", "chicken.png", "bee.png",
        "bat.png", "octopus.png", "giraffe.png",
    };

    /// <summary>Number of themed pieces every other skin has (pc_&lt;id&gt;_1..10.png).</summary>
    public const int PiecesPerSkin = 10;

    /// <summary>
    /// The pictures a skin uses inside the games (Sudoku uses the first 5, Puzzle Pets
    /// the first 7, Memory Match up to 10). They are ordered most-distinct first.
    /// </summary>
    public static IReadOnlyList<string> PiecesFor(Skin skin) =>
        skin.Id == DefaultId
            ? ClassicPieces
            : Enumerable.Range(1, PiecesPerSkin).Select(i => $"pc_{skin.Id}_{i}.png").ToArray();

    public static Skin Get(string? id) => All.FirstOrDefault(s => s.Id == id) ?? Default;

    public static Skin? ForProduct(string productId) => All.FirstOrDefault(s => s.ProductId == productId);

    public static IEnumerable<string> PaidProductIds => All.Where(s => !s.IsFree).Select(s => s.ProductId!);

    public static bool IsSkinProduct(string productId) =>
        productId == BundleProductId || ForProduct(productId) != null;
}
