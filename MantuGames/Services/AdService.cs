using Plugin.MauiMtAdmob;

namespace MantuGames.Services;

public class AdService
{
    // ════════════════════════════════════════════════════════════════
    //  REWARDED
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool> _rewardTcs;
    private bool _rewardEarned;

    public void PreloadRewarded()
    {
        if (!CrossMauiMTAdmob.Current.IsRewardedLoaded())
            CrossMauiMTAdmob.Current.LoadRewarded(AppConfig.RewardedAdUnitId);
    }

    public Task<bool> ShowRewardedAd()
    {
        _rewardTcs = new TaskCompletionSource<bool>();
        _rewardEarned = false;

        CrossMauiMTAdmob.Current.OnUserEarnedReward += OnRewardEarned;
        CrossMauiMTAdmob.Current.OnRewardedClosed += OnAdClosed;

        if (CrossMauiMTAdmob.Current.IsRewardedLoaded())
        {
            CrossMauiMTAdmob.Current.ShowRewarded();
        }
        else
        {
            CrossMauiMTAdmob.Current.OnRewardedLoaded += OnAdLoaded;
            CrossMauiMTAdmob.Current.LoadRewarded(AppConfig.RewardedAdUnitId);
        }

        return _rewardTcs.Task;
    }

    private void OnAdLoaded(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnAdLoaded;
        CrossMauiMTAdmob.Current.ShowRewarded();
    }

    private void OnRewardEarned(object sender, EventArgs e) => _rewardEarned = true;

    private void OnAdClosed(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnUserEarnedReward -= OnRewardEarned;
        CrossMauiMTAdmob.Current.OnRewardedClosed -= OnAdClosed;
        _rewardTcs.TrySetResult(_rewardEarned);
    }

    // ════════════════════════════════════════════════════════════════
    //  INTERSTITIAL
    // ════════════════════════════════════════════════════════════════

    private TaskCompletionSource<bool> _interTcs;

    public void PreloadInterstitial()
    {
        if (!CrossMauiMTAdmob.Current.IsInterstitialLoaded())
            CrossMauiMTAdmob.Current.LoadInterstitial(AppConfig.InterstitialAdUnitId);
    }

    public Task<bool> ShowInterstitial()
    {
        _interTcs = new TaskCompletionSource<bool>();

        CrossMauiMTAdmob.Current.OnInterstitialClosed += OnInterClosed;

        if (CrossMauiMTAdmob.Current.IsInterstitialLoaded())
        {
            CrossMauiMTAdmob.Current.ShowInterstitial();
        }
        else
        {
            CrossMauiMTAdmob.Current.OnInterstitialLoaded += OnInterLoaded;
            CrossMauiMTAdmob.Current.LoadInterstitial(AppConfig.InterstitialAdUnitId);
        }

        return _interTcs.Task;
    }

    private void OnInterLoaded(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnInterstitialLoaded -= OnInterLoaded;
        CrossMauiMTAdmob.Current.ShowInterstitial();
    }

    private void OnInterClosed(object sender, EventArgs e)
    {
        CrossMauiMTAdmob.Current.OnInterstitialClosed -= OnInterClosed;
        _interTcs.TrySetResult(true);
    }
}
