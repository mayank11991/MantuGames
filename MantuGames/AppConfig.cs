namespace MantuGames;

public static class AppConfig
{
    // ─── REPLACE with your real privacy policy URL before publishing ───
    public const string PrivacyUrl = "https://mayank11991.github.io/privacy.html";
    public const string PlayStoreUrl = "https://play.google.com/store/apps/details?id=com.mantugames.app";
    public const string SupportEmail = "support@mantugames.com";
    public const string UpdateCheckUrl = "https://mayank11991.github.io/latest.json";

#if DEBUG
    public const string AdMobAppId = "ca-app-pub-7489032711560079~7098519092";
    public const string RewardedAdUnitId = "ca-app-pub-7489032711560079/7541372222";
    public const string InterstitialAdUnitId = "ca-app-pub-7489032711560079/4136745618";
    public const string BannerAdUnitId = "ca-app-pub-7489032711560079/6024542356";
#else
    // ─── REPLACE WITH YOUR REAL IDs BEFORE PUBLISHING ───
    public const string AdMobAppId = "ca-app-pub-7489032711560079~7098519092";
    public const string RewardedAdUnitId = "ca-app-pub-7489032711560079/7541372222";
    public const string InterstitialAdUnitId = "ca-app-pub-7489032711560079/4136745618";
    public const string BannerAdUnitId = "ca-app-pub-7489032711560079/6024542356";
#endif
}