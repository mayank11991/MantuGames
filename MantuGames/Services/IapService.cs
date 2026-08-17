using Plugin.InAppBilling;

namespace MantuGames.Services;

public static class IapService
{
    public const string RemoveAdsId = "remove_ads";

    // Product IDs as set up in Google Play Console (Aug 9, 2026)
    public static readonly (string ProductId, int Coins)[] CoinPacks =
    {
        ("coins_150", 150),
        ("coins_250", 250),
        ("coins_450", 450),
        ("coins_1200", 1200),
        ("coins_2000", 2000),
    };

    private const string RemoveAdsOwnedKey = "remove_ads_owned";

    public static bool RemoveAdsOwned => Preferences.Default.Get(RemoveAdsOwnedKey, false);

    public static void SetRemoveAdsOwned(bool value) => Preferences.Default.Set(RemoveAdsOwnedKey, value);

    /// <summary>Purchases a product. Coins packs are consumed; remove_ads is non-consumable.</summary>
    public static async Task<bool> PurchaseAsync(string productId)
    {
        IInAppBilling? billing = null;
        try
        {
            billing = CrossInAppBilling.Current;
            bool connected = await billing.ConnectAsync();
            if (!connected) return false;

            bool consumable = productId != RemoveAdsId;
            var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase,
                obfuscatedAccountId: "mantugames", obfuscatedProfileId: "");

            if (purchase != null && purchase.State == PurchaseState.Purchased)
            {
                if (consumable)
                    await billing.ConsumePurchaseAsync(purchase.PurchaseToken, "mantugames");
                else
                    await billing.FinalizePurchaseAsync(purchase.PurchaseToken);

                if (!consumable)
                    SetRemoveAdsOwned(true);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            try { if (billing != null) await billing.DisconnectAsync(); } catch { }
        }
    }

    /// <summary>Restores previously bought non-consumables (e.g. after reinstall).</summary>
    public static async Task RestoreOwnedAsync()
    {
        IInAppBilling? billing = null;
        try
        {
            billing = CrossInAppBilling.Current;
            bool connected = await billing.ConnectAsync();
            if (!connected) return;

            var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
            if (purchases?.Any(p => p.ProductId == RemoveAdsId && p.State == PurchaseState.Purchased) == true)
                SetRemoveAdsOwned(true);
        }
        catch
        {
        }
        finally
        {
            try { if (billing != null) await billing.DisconnectAsync(); } catch { }
        }
    }
}