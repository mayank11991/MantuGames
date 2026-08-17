using MantuGames.Helpers;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class SettingsPopup : ContentView
{
    public event EventHandler EditProfileRequested;
    public event EventHandler ProgressReset;
    public event EventHandler StatisticsRequested;
    public event EventHandler UpdateCheckRequested;

    public SettingsPopup()
    {
        InitializeComponent();
        MusicSwitch.IsToggled = AudioService.Instance.MusicEnabled;
        SfxSwitch.IsToggled = AudioService.Instance.SfxEnabled;
        DarkModeSwitch.IsToggled = ThemeHelper.IsDarkMode;
        VersionLabel.Text = $"Version {AppInfo.VersionString} ({AppInfo.BuildString})";
        UpdateRemoveAdsStatus();
    }

    private void UpdateRemoveAdsStatus()
    {
        RemoveAdsStatus.Text = IapService.RemoveAdsOwned ? "Purchased" : "BUY";
        RemoveAdsStatus.TextColor = IapService.RemoveAdsOwned
            ? Color.FromArgb("#34D399")
            : Color.FromArgb("#FF9F1C");
    }

    public void Show()
    {
        IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        Overlay.IsVisible = true;
        this.FadeTo(1, 200, Easing.CubicOut);
        this.ScaleTo(1, 200, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeTo(0, 150, Easing.CubicIn);
        this.ScaleTo(0.8, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnMusicToggled(object sender, ToggledEventArgs e)
    {
        AudioService.Instance.MusicEnabled = e.Value;
    }

    private void OnSfxToggled(object sender, ToggledEventArgs e)
    {
        AudioService.Instance.SfxEnabled = e.Value;
    }

    private void OnVibrationToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("vibration_enabled", e.Value);
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        ThemeHelper.IsDarkMode = e.Value;
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Consume tap to prevent passing through to views underneath
    }

    private void OnEditProfile(object sender, TappedEventArgs e)
    {
        Hide();
        EditProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnStatistics(object sender, TappedEventArgs e)
    {
        Hide();
        StatisticsRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnCheckForUpdates(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateStatusLabel.Text = "Checking…";
            var latest = await UpdateService.FetchLatestAsync();
            if (latest == null)
            {
                UpdateStatusLabel.Text = "Offline";
                return;
            }

            if (UpdateService.IsUpdateAvailable(latest, out _))
            {
                UpdateStatusLabel.Text = "";
                Hide();
                UpdateCheckRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                UpdateStatusLabel.Text = "Up to date!";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnCheckForUpdates: {ex.Message}");
            UpdateStatusLabel.Text = "Error";
        }
    }

    private async void OnContactSupport(object sender, TappedEventArgs e)
    {
        try
        {
            await Launcher.OpenAsync($"mailto:{AppConfig.SupportEmail}?subject=Mantu Games Support");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening support mail: {ex.Message}");
        }
    }

    private async void OnPrivacyPolicy(object sender, TappedEventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(AppConfig.PrivacyUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening privacy policy: {ex.Message}");
        }
    }

    private async void OnRateUs(object sender, TappedEventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(AppConfig.PlayStoreUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening Play Store: {ex.Message}");
        }
    }

    private async void OnRemoveAds(object sender, TappedEventArgs e)
    {
        try
        {
            if (IapService.RemoveAdsOwned) return;

            RemoveAdsStatus.Text = "…";
            bool purchased = await IapService.PurchaseAsync(IapService.RemoveAdsId);
            if (purchased)
            {
                BannerHelper.RemoveBanner();
                UpdateRemoveAdsStatus();
            }
            else
            {
                RemoveAdsStatus.Text = "BUY";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnRemoveAds: {ex.Message}");
        }
    }

    private async void OnResetProgress(object sender, TappedEventArgs e)
    {
        try
        {
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Reset Progress", "This will erase all your progress for all games. Are you sure?", "Reset", "Cancel");
            if (!confirm) return;
          
            var games = ViewModels.DashboardViewModel.Games;
            foreach (var g in games)
            {
                ProgressService.Instance.ResetGame(g.Id);
            }
            CoinService.ResetAll();
            StatsService.ResetAll();

            ProgressReset?.Invoke(this, EventArgs.Empty);
            Hide();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnResetProgress: {ex.Message}");
        }
    }

    private void OnClose(object sender, TappedEventArgs e)
    {
        Hide();
    }
}