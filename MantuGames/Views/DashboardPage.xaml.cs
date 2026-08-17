using System.Collections.ObjectModel;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
        this.AddBannerAd();
        SettingsPopup.EditProfileRequested += OnEditProfile;
        SettingsPopup.StatisticsRequested += OnStatisticsRequested;
        SettingsPopup.UpdateCheckRequested += OnUpdateCheckRequested;
        CoinShopPopup.CoinsChanged += OnCoinsChanged;
        ProfilePicker.ProfileSelected += OnProfileSelected;
        ProfilePicker.AddProfileRequested += (s, e) => ProfilePopup.Show();
        ProfilePopup.ProfileCreated += (s, p) => ProfilePicker.Show();

        // Restore any previously purchased non-consumables (e.g. remove_ads after reinstall)
        _ = IapService.RestoreOwnedAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Show a friendly notice if the previous session crashed
        if (!Preferences.Get("crash_notice_shown", false))
        {
            Preferences.Set("crash_notice_shown", true);
            var report = CrashGuardService.TakePendingReport();
            if (report != null)
            {
                _ = Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Welcome back!",
                    "The app closed unexpectedly last time. Your progress is safe — we've noted the issue and it will be fixed soon.",
                    "OK");
            }
        }

        // Check for app updates (once per session)
        _ = CheckForUpdatesAsync();

        // Netflix-style gate: a profile must be picked or created — no skip.
        if (ProfileService.Active == null)
        {
            ProfilePicker.Show();
        }

        SettingsPopup.ProgressReset += OnProgressReset;
        RefreshCoins();

        // Show the active player's name
        PlayerNameLabel.Text = ProfileService.Active?.Name ?? "";

        // Start background music
        AudioService.Instance.StartMusic();
    }

    protected override bool OnBackButtonPressed()
    {
        // Block the back key while the profile gate is open — selection is mandatory.
        if (ProfilePicker.IsVisible)
            return true;
        return base.OnBackButtonPressed();
    }

    private static bool _updateCheckStarted;

    private async Task CheckForUpdatesAsync(bool force = false)
    {
        if (_updateCheckStarted && !force) return;
        _updateCheckStarted = true;
        try
        {
            var latest = await UpdateService.FetchLatestAsync();
            if (latest == null) return;
            if (UpdateService.IsUpdateAvailable(latest, out bool forceUpdate))
                UpdatePopup.Show(latest);
        }
        catch { }
    }

    public void ShowStats()
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        StatsPopup.Show();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SettingsPopup.ProgressReset -= OnProgressReset;
        AudioService.Instance.StopMusic();
    }

    private void RefreshCoins()
    {
        CoinBalanceLabel.Text = CoinService.GetTotalCoins() > 0 ? $"{CoinService.GetTotalCoins()}" : "";
    }

    private void OnProgressReset(object sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnCoinsChanged(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnProfileSelected(object? sender, Models.PlayerProfile profile)
    {
        PlayerNameLabel.Text = profile.Name;
        RefreshCoins();
    }

    private void OnEditProfile(object sender, EventArgs e)
    {
        ProfilePicker.Show();
    }

    private void OnStatisticsRequested(object sender, EventArgs e)
    {
        ShowStats();
    }

    private void OnUpdateCheckRequested(object sender, EventArgs e)
    {
        _ = CheckForUpdatesAsync(force: true);
    }

    private void OnCoinShopClicked(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        CoinShopPopup.Show();
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        SettingsPopup.Show();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string q = e.NewTextValue?.Trim() ?? "";
        if (string.IsNullOrEmpty(q))
        {
            GamesCollectionView.ItemsSource = DashboardViewModel.Games;
            return;
        }

        GamesCollectionView.ItemsSource = new ObservableCollection<GameInfo>(
            DashboardViewModel.Games.Where(g =>
                g.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                g.Id.Contains(q, StringComparison.OrdinalIgnoreCase)));
    }

    }