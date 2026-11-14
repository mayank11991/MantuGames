namespace MantuGames;

public static class AppConfig
{
    // ─── REPLACE with your real privacy policy URL before publishing ───
    public const string PrivacyUrl = "https://mayank11991.github.io/privacy.html";
    public const string PlayStoreUrl = "https://play.google.com/store/apps/details?id=com.mantugames.app";
    public const string SupportEmail = "infoatkhoj@gmail.com";
    public const string UpdateCheckUrl = "https://mayank11991.github.io/latest.json";

#if DEBUG
    // Google Official Test Ad Unit IDs (for debugging - always show test ads)
    public const string AdMobAppId = "ca-app-pub-3940256099942544~3347511713";
    public const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    public const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    public const string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
    // Production Ad Unit IDs (replace when ready to publish)
    public const string AdMobAppId = "ca-app-pub-7489032711560079~7098519092";
    public const string RewardedAdUnitId = "ca-app-pub-7489032711560079/7541372222";
    public const string InterstitialAdUnitId = "ca-app-pub-7489032711560079/4136745618";
    public const string BannerAdUnitId = "ca-app-pub-7489032711560079/6024542356";
#endif
}