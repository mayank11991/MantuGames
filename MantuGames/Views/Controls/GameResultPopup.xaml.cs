using MantuGames.Helpers;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class GameResultPopup : ContentView
{
    public event EventHandler? NextLevelRequested;
    public event EventHandler? RetryRequested;

    private bool _isWin;
    private bool _isProcessing;
    private string? _gameId;
    private int _levelNumber;
    private int _completionStars;
    private int _completionCoins;
    private AdService? _adService;

    public GameResultPopup()
    {
        InitializeComponent();
    }

    public async Task Show(bool isWin, int levelNumber, int elapsedSeconds,
                           int totalSeconds = 210, int stars = 0, int coins = 0,
                           string? reason = null, string? gameId = null)
    {
        // A paid "Show Solution" during this attempt means no stars or coins.
        bool assisted = !string.IsNullOrEmpty(gameId) && ProgressService.TakeAssisted(gameId);
        if (assisted) stars = 0;

        // Save progress immediately on win, before any ad or button tap,
        // so closing the app on this screen can never lose a solved level.
        // Coins shown are the coins actually awarded (only for new best stars),
        // not whatever the game page estimated.
        coins = 0;
        if (isWin && !string.IsNullOrEmpty(gameId))
            coins = ProgressService.Instance.CompleteLevel(gameId, levelNumber, stars);

        _isWin = isWin;
        _gameId = gameId;
        _levelNumber = levelNumber;
        _completionStars = stars;
        _completionCoins = coins;

        StatsService.RecordGame(gameId, isWin, coins);

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
            TitleLabel.TextColor = Color.FromArgb("#20B26B");

            if (coins > 0)
            {
                PointsBadge.IsVisible = true;
                PointsLabel.Text      = $"+{coins} coins";
            }
            else
                PointsBadge.IsVisible = false;

            if (assisted)
                MessageLabel.Text = $"Level {levelNumber} solved with Show Solution.\nSolve it yourself to earn stars!";
            else if (elapsedSeconds > 0)
                MessageLabel.Text = $"Level {levelNumber} solved in {elapsedSeconds / 60}:{elapsedSeconds % 60:D2}";
            else
                MessageLabel.Text = $"Level {levelNumber} solved!";

            PrimaryBorder.Style           = (Style)Application.Current!.Resources["CtaPillGreen"];
            PrimaryIcon.Source            = "ic_next_white.png";
            PrimaryLabel.Text             = "NEXT LEVEL";
            SecondaryBorder.IsVisible     = true;

            PreloadAd();
        }
        else
        {
            HeroIcon.Source = "icon_retry";

            StarsRow.IsVisible    = false;
            PointsBadge.IsVisible = false;

            TitleLabel.Text      = reason ?? "Time's Up!";
            TitleLabel.TextColor = Color.FromArgb("#FF4F6D");

            MessageLabel.Text = "Don't worry — you can do it!\nGive it another try!";

            PrimaryBorder.Style           = (Style)Application.Current!.Resources["CtaPillOrange"];
            PrimaryIcon.Source            = "ic_refresh_white.png";
            PrimaryLabel.Text             = "TRY AGAIN";
            SecondaryBorder.IsVisible     = false;

            PreloadAd();
        }

        await Task.Delay(800);

        IsVisible           = true;
        PopupCard.Scale     = 0.5;
        PopupCard.Opacity   = 0;

        await Task.WhenAll(
            PopupCard.ScaleToAsync(1.0, 380, Easing.SpringOut),
            PopupCard.FadeToAsync(1.0, 260)
        );

        _ = MaybeRateNudgeAsync();
    }

    public void Hide()
    {
        IsVisible = false;
    }

    // ── RATE US NUDGE ─────────────────────────────────────────────
    // Shown after every 5th completed game until the player rates or
    // dismisses it twice. Never interrupts wins that award coins.
    private async System.Threading.Tasks.Task MaybeRateNudgeAsync()
    {
        try
        {
            if (!_isWin || _completionCoins == 0) return;
            if (Preferences.Get("rate_done", false)) return;

            int completed = Preferences.Get("games_completed", 0) + 1;
            Preferences.Set("games_completed", completed);
            if (completed % 5 != 0) return;

            int dismissed = Preferences.Get("rate_dismissed", 0);
            if (dismissed >= 2) { Preferences.Set("rate_done", true); return; }

            bool rate = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
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
            Console.WriteLine($"Error in MaybeRateNudgeAsync: {ex.Message}");
        }
    }

    private void PreloadAd()
    {
        try
        {
            if (IapService.RemoveAdsOwned) return;
            _adService ??= IPlatformApplication.Current!.Services.GetRequiredService<AdService>();
            _adService.PreloadInterstitial();
        }
        catch { }
    }

    // Natural break between levels: a frequency-capped interstitial (never forced
    // rewarded ads, never for Remove Ads owners — AdService enforces both).
    private async Task MaybeShowInterstitialAsync()
    {
        try
        {
            _adService ??= IPlatformApplication.Current!.Services.GetRequiredService<AdService>();
            await _adService.MaybeShowInterstitialAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Interstitial error: {ex.Message}");
        }
    }

    private async void OnPrimaryClicked(object? sender, TappedEventArgs e)
    {
        try
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();

            Hide();
            await MaybeShowInterstitialAsync();

            if (_isWin)
                NextLevelRequested?.Invoke(this, EventArgs.Empty);
            else
                RetryRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnPrimaryClicked: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async void OnSecondaryClicked(object? sender, TappedEventArgs e)
    {
        try
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            Hide();

            await MaybeShowInterstitialAsync();
            RetryRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnSecondaryClicked: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
