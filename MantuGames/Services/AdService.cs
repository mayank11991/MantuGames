using Plugin.MauiMtAdmob;

namespace MantuGames.Services;

/// <summary>
/// Full-screen ads.
///  - Interstitials are frequency-capped (see <see cref="MaybeShowInterstitialAsync"/>)
///    and never shown to Remove Ads owners.
///  - Rewarded ads are opt-in only: callers must offer them as an explicit choice
///    ("watch an ad to get X") and grant the reward only when this returns true.
///  - Nothing is requested before UMP consent allows it (<see cref="ConsentService"/>).
/// </summary>
public class AdService
{
    // Set to false to disable all ads during testing
    public static bool AdsEnabled = true;

    private const int AdTimeoutSeconds = 12;

    // Interstitial cap: at most one every N result screens, and never within MinGap of the last one.
    private const int InterstitialEveryNResults = 3;
    private static readonly TimeSpan InterstitialMinGap = TimeSpan.FromSeconds(120);

    private int _resultsSinceInterstitial;
    private DateTime _lastInterstitialUtc = DateTime.MinValue;

    private static bool IsConnected =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    private static bool CanShowAds => AdsEnabled && ConsentService.CanRequestAds && IsConnected;

    // ════════════════════════════════════════════════════════════════
    //  REWARDED (opt-in only)
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool>? _rewardTcs;
    private bool _rewardEarned;

    public void PreloadRewarded()
    {
        if (!CanShowAds) return;
        try
        {
            if (!CrossMauiMTAdmob.Current.IsRewardedLoaded())
                CrossMauiMTAdmob.Current.LoadRewarded(AppConfig.RewardedAdUnitId);
        }
        catch { }
    }

    /// <summary>
    /// Shows a rewarded ad the user explicitly asked for.
    /// Returns true only if the user actually earned the reward.
    /// </summary>
    public async Task<bool> ShowRewardedAd()
    {
        if (!CanShowAds) return false;

        _rewardTcs = new TaskCompletionSource<bool>();
        _rewardEarned = false;

        try
        {
            CrossMauiMTAdmob.Current.OnUserEarnedReward += OnRewardEarned;
            CrossMauiMTAdmob.Current.OnRewardedClosed += OnAdClosed;
            CrossMauiMTAdmob.Current.OnRewardedFailedToLoad += OnRewardedFailedToLoad;

            if (CrossMauiMTAdmob.Current.IsRewardedLoaded())
            {
                CrossMauiMTAdmob.Current.ShowRewarded();
            }
            else
            {
                CrossMauiMTAdmob.Current.OnRewardedLoaded += OnAdLoaded;
                CrossMauiMTAdmob.Current.LoadRewarded(AppConfig.RewardedAdUnitId);
            }

            var completed = await Task.WhenAny(_rewardTcs.Task, Task.Delay(TimeSpan.FromSeconds(AdTimeoutSeconds)));
            if (completed == _rewardTcs.Task)
                return _rewardTcs.Task.Result;

            // Timed out waiting for load/close: reward only if it was already earned.
            return _rewardEarned;
        }
        catch
        {
            return _rewardEarned;
        }
        finally
        {
            CleanupRewarded();
        }
    }

    private void OnAdLoaded(object? sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnAdLoaded;
        try { CrossMauiMTAdmob.Current.ShowRewarded(); }
        catch { _rewardTcs?.TrySetResult(false); }
    }

    private void OnRewardEarned(object? sender, EventArgs e) => _rewardEarned = true;

    private void OnAdClosed(object? sender, EventArgs e) => _rewardTcs?.TrySetResult(_rewardEarned);

    private void OnRewardedFailedToLoad(object? sender, EventArgs e) => _rewardTcs?.TrySetResult(false);

    private void CleanupRewarded()
    {
        try
        {
            CrossMauiMTAdmob.Current.OnUserEarnedReward -= OnRewardEarned;
            CrossMauiMTAdmob.Current.OnRewardedClosed -= OnAdClosed;
            CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnAdLoaded;
            CrossMauiMTAdmob.Current.OnRewardedFailedToLoad -= OnRewardedFailedToLoad;
        }
        catch { }
    }

    // ════════════════════════════════════════════════════════════════
    //  INTERSTITIAL (frequency-capped)
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool>? _interTcs;

    public void PreloadInterstitial()
    {
        if (!CanShowAds || IapService.RemoveAdsOwned) return;
        try
        {
            if (!CrossMauiMTAdmob.Current.IsInterstitialLoaded())
                CrossMauiMTAdmob.Current.LoadInterstitial(AppConfig.InterstitialAdUnitId);
        }
        catch { }
    }

    /// <summary>
    /// Call at a natural break (after a result screen). Shows an interstitial only
    /// every <see cref="InterstitialEveryNResults"/> calls and never sooner than
    /// <see cref="InterstitialMinGap"/> after the previous one. Never blocks gameplay
    /// waiting for a load: if none is ready, it skips and preloads for next time.
    /// </summary>
    public async Task MaybeShowInterstitialAsync()
    {
        if (!CanShowAds || IapService.RemoveAdsOwned) return;

        _resultsSinceInterstitial++;
        if (_resultsSinceInterstitial < InterstitialEveryNResults) return;
        if (DateTime.UtcNow - _lastInterstitialUtc < InterstitialMinGap) return;

        bool loaded;
        try { loaded = CrossMauiMTAdmob.Current.IsInterstitialLoaded(); }
        catch { return; }

        if (!loaded)
        {
            PreloadInterstitial();
            return;
        }

        _interTcs = new TaskCompletionSource<bool>();
        try
        {
            CrossMauiMTAdmob.Current.OnInterstitialClosed += OnInterClosed;
            CrossMauiMTAdmob.Current.OnInterstitialFailedToShow += OnInterFailedToShow;

            CrossMauiMTAdmob.Current.ShowInterstitial();
            _resultsSinceInterstitial = 0;
            _lastInterstitialUtc = DateTime.UtcNow;

            // Wait for the user to close it (bounded, so the app can never get stuck).
            await Task.WhenAny(_interTcs.Task, Task.Delay(TimeSpan.FromSeconds(60)));
        }
        catch { }
        finally
        {
            CleanupInterstitial();
            PreloadInterstitial();
        }
    }

    private void OnInterClosed(object? sender, EventArgs e) => _interTcs?.TrySetResult(true);

    private void OnInterFailedToShow(object? sender, EventArgs e) => _interTcs?.TrySetResult(false);

    private void CleanupInterstitial()
    {
        try
        {
            CrossMauiMTAdmob.Current.OnInterstitialClosed -= OnInterClosed;
            CrossMauiMTAdmob.Current.OnInterstitialFailedToShow -= OnInterFailedToShow;
        }
        catch { }
    }
}
