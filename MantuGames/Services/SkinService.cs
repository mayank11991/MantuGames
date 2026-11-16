using MantuGames.Models;

namespace MantuGames.Services;

/// <summary>
/// Which skins this device owns and which one each player is using.
/// Ownership follows the Google account (restored from Play on every launch),
/// so it is device-wide; the chosen skin is per player profile.
/// </summary>
public static class SkinService
{
    private const string OwnedKey = "skins_owned";

    /// <summary>Raised on the main thread when the active skin or ownership changes.</summary>
    public static event EventHandler? Changed;

    static SkinService()
    {
        // Each player has their own skin: repaint when the active profile changes.
        ProfileService.ActiveChanged += (s, e) => RaiseChanged();
    }

    private static string SelectedKey => ProfileService.Key("skin_selected");

    public static Skin Current
    {
        get
        {
            var skin = SkinCatalog.Get(Preferences.Default.Get(SelectedKey, SkinCatalog.DefaultId));
            return IsOwned(skin) ? skin : SkinCatalog.Default;
        }
    }

    public static bool BundleOwned => OwnedProducts.Contains(SkinCatalog.BundleProductId);

    public static bool IsOwned(Skin skin) =>
        skin.IsFree || BundleOwned || OwnedProducts.Contains(skin.ProductId!);

    public static bool AllPaidOwned => SkinCatalog.All.All(IsOwned);

    /// <summary>Makes <paramref name="skin"/> the active one for this player, if owned.</summary>
    public static bool Use(Skin skin)
    {
        if (!IsOwned(skin)) return false;
        Preferences.Default.Set(SelectedKey, skin.Id);
        RaiseChanged();
        return true;
    }

    /// <summary>Records a Play purchase of a skin or the bundle (idempotent).</summary>
    public static void MarkOwned(string productId)
    {
        var owned = OwnedProducts;
        if (!owned.Add(productId)) return;
        Preferences.Default.Set(OwnedKey, string.Join(',', owned));
        RaiseChanged();
    }

    private static HashSet<string> OwnedProducts =>
        Preferences.Default.Get(OwnedKey, "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

    private static void RaiseChanged() =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyTheme();
            Changed?.Invoke(null, EventArgs.Empty);
        });

    /// <summary>
    /// Points the app-wide primary button colours at the current skin. Styles read them
    /// through DynamicResource, so every open screen repaints at once.
    /// </summary>
    public static void ApplyTheme()
    {
        var res = Application.Current?.Resources;
        if (res == null) return;
        var button = Color.FromArgb(Current.Button);
        res["SkinButton"] = button;
        res["SkinButtonLipBrush"] = new SolidColorBrush(button.WithLuminosity(Math.Max(0, button.GetLuminosity() - 0.15f)));
    }
}
