using Plugin.MauiMtAdmob;

namespace MantuGames.Services;

public class AdService
{
    // Set to false to disable all ads during testing
    public static bool AdsEnabled = true;

    private const int AdTimeoutSeconds = 12;
    private const int FakeAdDuration = 30;

    private static bool IsConnected =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    // ════════════════════════════════════════════════════════════════
    //  REWARDED
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool> _rewardTcs;
    private bool _rewardEarned;

    public void PreloadRewarded()
    {
        if (!AdsEnabled) return;
        try
        {
            if (IsConnected && !CrossMauiMTAdmob.Current.IsRewardedLoaded())
                CrossMauiMTAdmob.Current.LoadRewarded(AppConfig.RewardedAdUnitId);
        }
        catch { }
    }

    public async Task<bool> ShowRewardedAd()
    {
        if (!AdsEnabled) return true;

        // No internet — show fake ad for 30 seconds
        if (!IsConnected)
        {
            await ShowFakeAd();
            return true;
        }

        _rewardTcs = new TaskCompletionSource<bool>();
        _rewardEarned = false;

        CrossMauiMTAdmob.Current.OnUserEarnedReward += OnRewardEarned;
        CrossMauiMTAdmob.Current.OnRewardedClosed += OnAdClosed;
        CrossMauiMTAdmob.Current.OnRewardedFailedToLoad += OnRewardedFailedToLoad;

        try
        {
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

            if (completed != _rewardTcs.Task || !_rewardTcs.Task.Result)
                return true;

            return true;
        }
        catch
        {
            return true;
        }
        finally
        {
            CleanupRewarded();
        }
    }

    private void OnAdLoaded(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnAdLoaded;
        try { CrossMauiMTAdmob.Current.ShowRewarded(); }
        catch { _rewardTcs.TrySetResult(false); }
    }

    private void OnRewardEarned(object sender, EventArgs e) => _rewardEarned = true;

    private void OnAdClosed(object sender, EventArgs e) => _rewardTcs.TrySetResult(_rewardEarned);

    private void OnRewardedFailedToLoad(object sender, EventArgs e) => _rewardTcs.TrySetResult(false);

    private void CleanupRewarded()
    {
        CrossMauiMTAdmob.Current.OnUserEarnedReward -= OnRewardEarned;
        CrossMauiMTAdmob.Current.OnRewardedClosed -= OnAdClosed;
        CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnAdLoaded;
        CrossMauiMTAdmob.Current.OnRewardedFailedToLoad -= OnRewardedFailedToLoad;
    }

    // ════════════════════════════════════════════════════════════════
    //  INTERSTITIAL
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool> _interTcs;

    public void PreloadInterstitial()
    {
        if (!AdsEnabled) return;
        try
        {
            if (IsConnected && !CrossMauiMTAdmob.Current.IsInterstitialLoaded())
                CrossMauiMTAdmob.Current.LoadInterstitial(AppConfig.InterstitialAdUnitId);
        }
        catch { }
    }

    public async Task<bool> ShowInterstitial()
    {
        if (!AdsEnabled) return true;

        // No internet — skip interstitial entirely (rewarded ad already handled the wait)
        if (!IsConnected) return true;

        _interTcs = new TaskCompletionSource<bool>();

        CrossMauiMTAdmob.Current.OnInterstitialClosed += OnInterClosed;
        CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad += OnInterstitialFailedToLoad;

        try
        {
            if (CrossMauiMTAdmob.Current.IsInterstitialLoaded())
            {
                CrossMauiMTAdmob.Current.ShowInterstitial();
            }
            else
            {
                CrossMauiMTAdmob.Current.OnInterstitialLoaded += OnInterLoaded;
                CrossMauiMTAdmob.Current.LoadInterstitial(AppConfig.InterstitialAdUnitId);
            }

            var completed = await Task.WhenAny(_interTcs.Task, Task.Delay(TimeSpan.FromSeconds(AdTimeoutSeconds)));

            return true;
        }
        catch
        {
            return true;
        }
        finally
        {
            CleanupInterstitial();
        }
    }

    private void OnInterLoaded(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnInterstitialLoaded -= OnInterLoaded;
        try { CrossMauiMTAdmob.Current.ShowInterstitial(); }
        catch { _interTcs.TrySetResult(true); }
    }

    private void OnInterClosed(object sender, EventArgs e) => _interTcs.TrySetResult(true);

    private void OnInterstitialFailedToLoad(object sender, EventArgs e) => _interTcs.TrySetResult(true);

    private void CleanupInterstitial()
    {
        CrossMauiMTAdmob.Current.OnInterstitialClosed -= OnInterClosed;
        CrossMauiMTAdmob.Current.OnInterstitialLoaded -= OnInterLoaded;
        CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad -= OnInterstitialFailedToLoad;
    }

    // ════════════════════════════════════════════════════════════════
    //  FAKE AD FALLBACK
    // ════════════════════════════════════════════════════════════════

    private async Task ShowFakeAd()
    {
        var tcs = new TaskCompletionSource<bool>();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var fakeAd = new MantuGames.Views.FakeAdPage(FakeAdDuration);

            fakeAd.FakeAdClosed += async (s, e) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        var window = Application.Current?.Windows.FirstOrDefault();
                        if (window?.Page != null)
                            await window.Page.Navigation.PopModalAsync();
                    }
                    catch { }
                    tcs.TrySetResult(true);
                });
            };

            var win = Application.Current?.Windows.FirstOrDefault();
            if (win?.Page != null)
            {
                await win.Page.Navigation.PushModalAsync(fakeAd, false);
                fakeAd.Start();
            }
            else
            {
                tcs.TrySetResult(true);
            }
        });

        await tcs.Task;
    }
}
