using MantuGames.Helpers;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class GameResultPopup : ContentView
{
    public event EventHandler NextLevelRequested;
    public event EventHandler RetryRequested;

    private bool _isWin;
    private bool _isProcessing;
    private string _gameId;
    private int _levelNumber;
    private int _completionStars;
    private int _completionPoints;
    private AdService _adService;

    public GameResultPopup()
    {
        InitializeComponent();
    }

    public async Task Show(bool isWin, int levelNumber, int elapsedSeconds,
                           int totalSeconds = 210, int stars = 0, int points = 0,
                           string reason = null, string gameId = null)
    {
        _isWin = isWin;
        _gameId = gameId;
        _levelNumber = levelNumber;
        _completionStars = stars;
        _completionPoints = points;

        StatsService.RecordGame(gameId, isWin, points);

        AudioService.Instance.Play(isWin ? "win" : "lose");
        if (isWin) VibrationHelper.LongPress();
        else VibrationHelper.Click();

        if (isWin)
        {
            HeroIcon.Source = "icon_trophy";

            StarsRow.IsVisible = true;
            StarImg1.Source = stars >= 1 ? "star_kawaii_filled" : "star_kawaii_empty";
            StarImg2.Source = stars >= 2 ? "star_kawaii_filled" : "star_kawaii_empty";
            StarImg3.Source = stars >= 3 ? "star_kawaii_filled" : "star_kawaii_empty";

            TitleLabel.Text      = "You Won!";
            TitleLabel.TextColor = Color.FromArgb("#34D399");

            if (points > 0)
            {
                PointsBadge.IsVisible = true;
                PointsLabel.Text      = $"+{points} pts";
            }
            else
                PointsBadge.IsVisible = false;

            MessageLabel.Text = $"Level {levelNumber} solved in {elapsedSeconds / 60}:{elapsedSeconds % 60:D2}";

            PrimaryBorder.BackgroundColor = Color.FromArgb("#34D399");
            PrimaryIcon.Source            = "icon_next";
            PrimaryLabel.Text             = "Next Level";
            SecondaryBorder.IsVisible     = true;

            PreloadAd();
        }
        else
        {
            HeroIcon.Source = reason != null ? "icon_trophy" : "icon_timeout";

            StarsRow.IsVisible    = false;
            PointsBadge.IsVisible = false;

            TitleLabel.Text      = reason ?? "Time's Up!";
            TitleLabel.TextColor = Color.FromArgb("#EF4444");

            MessageLabel.Text = "Don't worry — you can do it!\nGive it another try!";

            PrimaryBorder.BackgroundColor = Color.FromArgb("#EF4444");
            PrimaryIcon.Source            = "icon_retry";
            PrimaryLabel.Text             = "Try Again";
            SecondaryBorder.IsVisible     = false;
        }

        await Task.Delay(800);

        IsVisible           = true;
        PopupCard.Scale     = 0.5;
        PopupCard.Opacity   = 0;

        await Task.WhenAll(
            PopupCard.ScaleTo(1.0, 380, Easing.SpringOut),
            PopupCard.FadeTo(1.0, 260)
        );

        _ = MaybeRateNudgeAsync();
    }

    public void Hide() => IsVisible = false;

    // ── RATE US NUDGE ─────────────────────────────────────────────
    // Shown after every 5th completed game until the player rates or
    // dismisses it twice. Never interrupts wins that award points.
    private async System.Threading.Tasks.Task MaybeRateNudgeAsync()
    {
        try
        {
            if (!_isWin || _completionPoints == 0) return;
            if (Preferences.Get("rate_done", false)) return;

            int completed = Preferences.Get("games_completed", 0) + 1;
            Preferences.Set("games_completed", completed);
            if (completed % 5 != 0) return;

            int dismissed = Preferences.Get("rate_dismissed", 0);
            if (dismissed >= 2) { Preferences.Set("rate_done", true); return; }

            bool rate = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Enjoying Mantu Games?",
                "Your rating helps other players discover the games. Rate us on Google Play!",
                "Rate Now", "Not Now");

            if (rate)
            {
                Preferences.Set("rate_done", true);
                await Launcher.OpenAsync(AppConfig.PlayStoreUrl);
            }
            else
            {
                Preferences.Set("rate_dismissed", dismissed + 1);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MaybeRateNudgeAsync: {ex.Message}");
        }
    }

    private void PreloadAd()
    {
        try
        {
            if (IapService.RemoveAdsOwned) return;
            _adService ??= IPlatformApplication.Current!.Services.GetRequiredService<AdService>();
            _adService.PreloadRewarded();
            _adService.PreloadInterstitial();
        }
        catch { }
    }

    private void CompleteLevelAndProceed()
    {
        if (!string.IsNullOrEmpty(_gameId))
            ProgressService.Instance.CompleteLevel(_gameId, _levelNumber, _completionStars, _completionPoints);
        NextLevelRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnPrimaryClicked(object sender, TappedEventArgs e)
    {
        try
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();

            Hide();

            _adService ??= IPlatformApplication.Current!.Services.GetRequiredService<AdService>();

            if (_isWin)
            {
                if (IapService.RemoveAdsOwned)
                {
                    CompleteLevelAndProceed();
                    _isProcessing = false;
                    return;
                }

                bool rewarded = await _adService.ShowRewardedAd();
                if (!rewarded)
                {
                    _isProcessing = false;
                    IsVisible = true;
                    return;
                }
                await _adService.ShowInterstitial();
                CompleteLevelAndProceed();
            }
            else
            {
                if (!IapService.RemoveAdsOwned)
                    await _adService.ShowInterstitial();
                RetryRequested?.Invoke(this, EventArgs.Empty);
            }

            _isProcessing = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnPrimaryClicked: {ex.Message}");
            _isProcessing = false;
        }
    }

    private async void OnSecondaryClicked(object sender, TappedEventArgs e)
    {
        try
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            Hide();

            _adService ??= IPlatformApplication.Current!.Services.GetRequiredService<AdService>();
            await _adService.ShowInterstitial();

            RetryRequested?.Invoke(this, EventArgs.Empty);
            _isProcessing = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnSecondaryClicked: {ex.Message}");
            _isProcessing = false;
        }
    }
}
