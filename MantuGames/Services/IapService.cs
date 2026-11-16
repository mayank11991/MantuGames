using MantuGames.Models;
using Plugin.InAppBilling;

namespace MantuGames.Services;

public enum PurchaseOutcome
{
    Purchased,
    /// <summary>Payment not confirmed yet (e.g. cash / slow card / family approval). Delivered on a later launch.</summary>
    Pending,
    Failed,
}

/// <summary>
/// Google Play purchases.
///
/// Coin packs are made loss-proof:
///   1. On a Purchased state, coins are credited to a persisted "unassigned" bank
///      exactly once per purchase token (token is remembered).
///   2. Only then is the purchase consumed. If the app dies in between, the next
///      launch finds the unconsumed purchase, skips the (already done) credit and
///      retries the consume — no lost coins, no double credit.
///   3. Pending purchases are picked up by <see cref="ProcessOutstandingAsync"/>
///      once Play reports them as Purchased.
/// The player then moves unassigned coins into a game of their choice.
/// </summary>
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
    private const string UnassignedCoinsKey = "iap_unassigned_coins";
    private const string CreditedTokensKey = "iap_credited_tokens";

    private static readonly SemaphoreSlim _billingLock = new(1, 1);
    private static readonly object _creditLock = new();

    public static bool RemoveAdsOwned => Preferences.Default.Get(RemoveAdsOwnedKey, false);

    public static void SetRemoveAdsOwned(bool value) => Preferences.Default.Set(RemoveAdsOwnedKey, value);

    /// <summary>Coins bought by older builds that were never moved into a game. Read once by the wallet migration.</summary>
    internal static int TakeLegacyUnassignedCoins()
    {
        int amount = Preferences.Default.Get(UnassignedCoinsKey, 0);
        Preferences.Default.Remove(UnassignedCoinsKey);
        return amount;
    }

    private static int CoinsFor(string productId) =>
        CoinPacks.FirstOrDefault(p => p.ProductId == productId).Coins;

    /// <summary>Credits a coin purchase once per token. Returns true if it was newly credited.</summary>
    private static bool CreditOnce(string token, int coins)
    {
        if (string.IsNullOrEmpty(token) || coins <= 0) return false;
        lock (_creditLock)
        {
            var tokens = Preferences.Default.Get(CreditedTokensKey, "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (tokens.Contains(token)) return false;

            CoinService.AddCoins(coins);
            tokens.Add(token);
            // Keep the list bounded; consumed tokens never come back from Play.
            if (tokens.Count > 200) tokens = tokens.Skip(tokens.Count - 200).ToList();
            Preferences.Default.Set(CreditedTokensKey, string.Join('\n', tokens));
        }
        return true;
    }

    /// <summary>Delivers a purchase that Play reports as Purchased (credit + consume / acknowledge).</summary>
    private static async Task DeliverAsync(IInAppBilling billing, InAppBillingPurchase purchase)
    {
        if (purchase.State != PurchaseState.Purchased) return;

        if (SkinCatalog.IsSkinProduct(purchase.ProductId))
        {
            // Skins are non-consumable: remember ownership and acknowledge once.
            SkinService.MarkOwned(purchase.ProductId);
            if (purchase.IsAcknowledged != true)
                await billing.FinalizePurchaseAsync(new[] { purchase.PurchaseToken });
            return;
        }

        if (purchase.ProductId == RemoveAdsId)
        {
            SetRemoveAdsOwned(true);
            if (purchase.IsAcknowledged != true)
                await billing.FinalizePurchaseAsync(new[] { purchase.PurchaseToken });
            return;
        }

        int coins = CoinsFor(purchase.ProductId);
        if (coins <= 0) return; // unknown product

        CreditOnce(purchase.PurchaseToken, coins);
        await billing.ConsumePurchaseAsync(purchase.ProductId, purchase.PurchaseToken);
    }

    /// <summary>Buys a product. Coin packs go to the unassigned bank; remove_ads is non-consumable.</summary>
    public static async Task<PurchaseOutcome> PurchaseAsync(string productId)
    {
        await _billingLock.WaitAsync();
        IInAppBilling? billing = null;
        try
        {
            billing = CrossInAppBilling.Current;
            if (!await billing.ConnectAsync()) return PurchaseOutcome.Failed;

            var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase);
            if (purchase == null) return PurchaseOutcome.Failed;

            if (purchase.State == PurchaseState.PaymentPending || purchase.State == PurchaseState.Deferred)
                return PurchaseOutcome.Pending;

            if (purchase.State != PurchaseState.Purchased) return PurchaseOutcome.Failed;

            try
            {
                await DeliverAsync(billing, purchase);
            }
            catch (Exception ex)
            {
                // Coins are already credited; consume/ack is retried on next launch.
                CrashGuardService.Log(ex);
            }
            return PurchaseOutcome.Purchased;
        }
        catch (InAppBillingPurchaseException ex) when (ex.PurchaseError == PurchaseError.AlreadyOwned)
        {
            // An earlier purchase of this pack was never consumed: deliver it now.
            int before = CoinService.Balance;
            try { await ProcessOutstandingCoreAsync(billing); }
            catch (Exception inner) { CrashGuardService.Log(inner); return PurchaseOutcome.Failed; }
            bool nonConsumable = productId == RemoveAdsId || SkinCatalog.IsSkinProduct(productId);
            return nonConsumable || CoinService.Balance > before ? PurchaseOutcome.Purchased : PurchaseOutcome.Failed;
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
            return PurchaseOutcome.Failed;
        }
        finally
        {
            try { if (billing != null) await billing.DisconnectAsync(); } catch { }
            _billingLock.Release();
        }
    }

    /// <summary>
    /// Localised prices from Google Play (e.g. "₹49.00"), keyed by product id.
    /// Returns an empty map when Play can't be reached; callers show a fallback.
    /// </summary>
    public static async Task<Dictionary<string, string>> GetPricesAsync(IEnumerable<string> productIds)
    {
        var prices = new Dictionary<string, string>();
        await _billingLock.WaitAsync();
        IInAppBilling? billing = null;
        try
        {
            billing = CrossInAppBilling.Current;
            if (!await billing.ConnectAsync()) return prices;
            var products = await billing.GetProductInfoAsync(ItemType.InAppPurchase, productIds.ToArray());
            foreach (var p in products ?? Enumerable.Empty<InAppBillingProduct>())
                if (!string.IsNullOrEmpty(p.ProductId) && !string.IsNullOrEmpty(p.LocalizedPrice))
                    prices[p.ProductId] = p.LocalizedPrice;
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
        finally
        {
            try { if (billing != null) await billing.DisconnectAsync(); } catch { }
            _billingLock.Release();
        }
        return prices;
    }

    /// <summary>
    /// Run at startup: restores remove_ads after reinstall and delivers any coin
    /// purchases that were paid but not consumed (crash, pending payment, old bug).
    /// </summary>
    public static async Task ProcessOutstandingAsync()
    {
        await _billingLock.WaitAsync();
        IInAppBilling? billing = null;
        try
        {
            billing = CrossInAppBilling.Current;
            if (!await billing.ConnectAsync()) return;
            await ProcessOutstandingCoreAsync(billing);
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
        finally
        {
            try { if (billing != null) await billing.DisconnectAsync(); } catch { }
            _billingLock.Release();
        }
    }

    private static async Task ProcessOutstandingCoreAsync(IInAppBilling? billing)
    {
        if (billing == null) return;
        var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);
        if (purchases == null) return;

        foreach (var p in purchases)
        {
            try { await DeliverAsync(billing, p); }
            catch (Exception ex) { CrashGuardService.Log(ex); }
        }
    }
}
