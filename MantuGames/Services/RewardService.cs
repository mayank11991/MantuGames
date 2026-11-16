namespace MantuGames.Services;

/// <summary>
/// Opt-in rewarded ads: the player taps a clearly labelled "watch an ad" button
/// and gets the reward only if the ad reports it was earned. Daily caps per
/// profile keep it from turning into an ad wall.
/// </summary>
public static class RewardService
{
    public const int FreeCoinsAmount = 10;
    public const int FreeCoinsPerDay = 5;
    public const int DoublesPerDay = 10;

    private static AdService Ads =>
        IPlatformApplication.Current!.Services.GetRequiredService<AdService>();

    /// <summary>True when a rewarded ad can be requested right now (consent given, online).</summary>
    public static bool CanOffer => AdService.CanShowRewarded;

    public static int FreeCoinsLeftToday => FreeCoinsPerDay - UsedToday("free_coins");
    public static bool CanDoubleToday => UsedToday("double_coins") < DoublesPerDay;

    public static void Preload()
    {
        try { Ads.PreloadRewarded(); } catch { }
    }

    /// <summary>Shows a rewarded ad and, if earned, adds <see cref="FreeCoinsAmount"/> coins.</summary>
    public static async Task<bool> WatchForFreeCoinsAsync()
    {
        if (FreeCoinsLeftToday <= 0) return false;
        if (!await Ads.ShowRewardedAd()) return false;
        CountUse("free_coins");
        CoinService.AddCoins(FreeCoinsAmount);
        return true;
    }

    /// <summary>Shows a rewarded ad and, if earned, adds the level's coins a second time.</summary>
    public static async Task<bool> WatchToDoubleAsync(int coins)
    {
        if (coins <= 0 || !CanDoubleToday) return false;
        if (!await Ads.ShowRewardedAd()) return false;
        CountUse("double_coins");
        CoinService.AddCoins(coins);
        return true;
    }

    private static string Today => DateTime.Now.ToString("yyyy-MM-dd");

    private static int UsedToday(string kind)
    {
        string dayKey = ProfileService.Key($"reward_{kind}_day");
        if (Preferences.Default.Get(dayKey, "") != Today) return 0;
        return Preferences.Default.Get(ProfileService.Key($"reward_{kind}_count"), 0);
    }

    private static void CountUse(string kind)
    {
        int used = UsedToday(kind) + 1;
        Preferences.Default.Set(ProfileService.Key($"reward_{kind}_day"), Today);
        Preferences.Default.Set(ProfileService.Key($"reward_{kind}_count"), used);
    }
}
