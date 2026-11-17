using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class SkinThemeTests
{
    private static string Images => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MantuGames", "Resources", "Images");

    [Fact]
    public void EveryPaidSkin_HasATheme_AndFreeSkinsDoNot()
    {
        foreach (var skin in SkinCatalog.All)
            Assert.Equal(!skin.IsFree, SkinThemes.For(skin) != null);
        Assert.Equal(10, SkinThemes.ById.Count);
    }

    [Fact]
    public void Themes_HaveFiveValidPaletteColours()
    {
        foreach (var (id, t) in SkinThemes.ById)
        {
            Assert.Equal(5, t.Palette.Length);
            Assert.Equal(5, t.Palette.Distinct().Count());
            foreach (var hex in t.Palette.Append(t.DialogBackground).Append(t.DialogBorder).Append(t.DialogShadow))
                Assert.Matches("^#[0-9A-Fa-f]{6}$", hex);
        }
    }

    [Fact]
    public void Themes_UseRegisteredTitleFonts()
    {
        var fonts = new[] { SkinThemes.Lilita, SkinThemes.Anton, SkinThemes.Orbitron, SkinThemes.Madimi, SkinThemes.Baloo };
        foreach (var t in SkinThemes.ById.Values)
            Assert.Contains(t.TitleFont, fonts);
    }

    [Fact]
    public void SceneryImages_Exist()
    {
        foreach (var t in SkinThemes.ById.Values)
        {
            Assert.True(File.Exists(Path.Combine(Images, t.SceneTop)), t.SceneTop);
            Assert.True(File.Exists(Path.Combine(Images, t.SceneBottom)), t.SceneBottom);
        }
    }

    [Fact]
    public void PaletteColour_WrapsAround()
    {
        var t = SkinThemes.ById["lion"];
        Assert.Equal(t.Palette[0], t.Color(5));
        Assert.Equal(t.Palette[4], t.Color(-1));
    }
}
