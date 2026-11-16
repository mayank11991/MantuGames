using System.Globalization;
using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class SkinCatalogTests
{
    [Fact]
    public void Catalog_HasTwoFreeAndTenPaidSkins()
    {
        Assert.Equal(12, SkinCatalog.All.Count);
        Assert.Equal(2, SkinCatalog.All.Count(s => s.IsFree));
        Assert.Equal(10, SkinCatalog.PaidProductIds.Count());
    }

    [Fact]
    public void Default_IsTheFreeClassicSkin()
    {
        Assert.Equal(SkinCatalog.DefaultId, SkinCatalog.Default.Id);
        Assert.True(SkinCatalog.Default.IsFree);
        Assert.Same(SkinCatalog.Default, SkinCatalog.Get("no-such-skin"));
        Assert.Same(SkinCatalog.Default, SkinCatalog.Get(null));
    }

    [Fact]
    public void IdsAndProductIds_AreUnique()
    {
        Assert.Equal(SkinCatalog.All.Count, SkinCatalog.All.Select(s => s.Id).Distinct().Count());
        var products = SkinCatalog.PaidProductIds.Append(SkinCatalog.BundleProductId).ToList();
        Assert.Equal(products.Count, products.Distinct().Count());
    }

    [Fact]
    public void ProductIds_AreValidPlayIds()
    {
        // Play product ids: lowercase letters, digits, underscores and dots, starting with a letter or digit.
        foreach (var id in SkinCatalog.PaidProductIds.Append(SkinCatalog.BundleProductId))
            Assert.Matches("^[a-z0-9][a-z0-9_.]*$", id);
    }

    [Fact]
    public void ProductLookup_FindsSkinsAndBundle()
    {
        Assert.Equal("lion", SkinCatalog.ForProduct("skin_lion")!.Id);
        Assert.True(SkinCatalog.IsSkinProduct("skin_lion"));
        Assert.True(SkinCatalog.IsSkinProduct(SkinCatalog.BundleProductId));
        Assert.False(SkinCatalog.IsSkinProduct("remove_ads"));
        Assert.False(SkinCatalog.IsSkinProduct("coins_150"));
    }

    [Fact]
    public void Colours_AreValidHex()
    {
        foreach (var s in SkinCatalog.All)
            foreach (var hex in new[] { s.ShellTop, s.ShellMid, s.GameTop, s.GameMid, s.GameBottom, s.Accent })
            {
                Assert.Matches("^#[0-9A-Fa-f]{6}$", hex);
                Assert.True(int.TryParse(hex[1..], NumberStyles.HexNumber, null, out _));
            }
    }
}
