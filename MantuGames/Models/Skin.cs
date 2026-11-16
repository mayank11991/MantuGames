namespace MantuGames.Models;

/// <summary>
/// A look for the whole app: backdrop colours, the mascot (shown on the home
/// screen and as the maze player) and the accent used for game pieces such as
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
}

public static class SkinCatalog
{
    public const string DefaultId = "classic";

    /// <summary>One Google Play product that unlocks every paid skin.</summary>
    public const string BundleProductId = "skins_all";

    public static readonly IReadOnlyList<Skin> All = new[]
    {
        //       id           name                 mascot           product          shell top  shell mid  game top   game mid   game bottom accent
        new Skin("classic",   "Classic Cat",       "cat.png",       null,            "#8FE3B4", "#DDF8E8", "#6EC6FF", "#BFE8FF", "#E6F8D8", "#7C5CFF"),
        new Skin("penguin",   "Arctic Penguin",    "penguin.png",   null,            "#9FD8FF", "#E3F3FF", "#7FB8FF", "#D6E8FF", "#F2F8FF", "#2F6FD6"),
        new Skin("lion",      "Jungle Lion",       "lion.png",      "skin_lion",     "#FFC27A", "#FFEBD2", "#FFB36B", "#FFE2BF", "#FFF6E6", "#E8792B"),
        new Skin("panda",     "Bamboo Panda",      "panda.png",     "skin_panda",    "#A8E6A1", "#E6F8E2", "#8BD88A", "#D8F3D3", "#F4FFF0", "#2E3440"),
        new Skin("owl",       "Forest Owl",        "owl.png",       "skin_owl",      "#8CE0B9", "#E0F7EC", "#5FCF9F", "#CFF2E1", "#F0FFF8", "#2FA66A"),
        new Skin("octopus",   "Candy Octopus",     "octopus.png",   "skin_octopus",  "#FFA3CF", "#FFE4F1", "#FF8FC0", "#FFD6EA", "#FFF3F9", "#E8458B"),
        new Skin("dino",      "Dino Valley",       "dino.png",      "skin_dino",     "#C3EE6A", "#EEFBD0", "#A6E04A", "#E2F6BE", "#FAFFEE", "#3BA55C"),
        new Skin("bee",       "Honey Bee",         "bee.png",       "skin_bee",      "#FFE16B", "#FFF6C9", "#FFD43B", "#FFF0B0", "#FFFBEA", "#F2A900"),
        new Skin("sheep",     "Cotton Sheep",      "sheep.png",     "skin_sheep",    "#C9B8FF", "#EEE8FF", "#B39CFF", "#E4DBFF", "#F8F5FF", "#8A6BEF"),
        new Skin("sloth",     "Rainforest Sloth",  "sloth.png",     "skin_sloth",    "#7FE0D6", "#DDF7F4", "#5ACFC3", "#CCF1ED", "#F0FFFD", "#1F9E93"),
        new Skin("elephant",  "Sky Elephant",      "elephant.png",  "skin_elephant", "#9CC4FF", "#E2EDFF", "#7AA8FF", "#D3E3FF", "#F3F7FF", "#4C6FFF"),
        new Skin("butterfly", "Rainbow Butterfly", "butterfly.png", "skin_butterfly","#FFA6D1", "#C7B3FF", "#FF9AC8", "#B9A8FF", "#E6FFF4", "#A35CFF"),
    };

    public static Skin Default => All[0];

    public static Skin Get(string? id) => All.FirstOrDefault(s => s.Id == id) ?? Default;

    public static Skin? ForProduct(string productId) => All.FirstOrDefault(s => s.ProductId == productId);

    public static IEnumerable<string> PaidProductIds => All.Where(s => !s.IsFree).Select(s => s.ProductId!);

    public static bool IsSkinProduct(string productId) =>
        productId == BundleProductId || ForProduct(productId) != null;
}
