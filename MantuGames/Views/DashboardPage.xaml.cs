using System.Collections.ObjectModel;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

public partial class DashboardPage : ContentPage
{
    private bool _isNebulaAnimating = false;
    private CancellationTokenSource _nebulaCts;

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
        EditProfilePopup.ProfileUpdated += OnProfileUpdated;

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

        // Start nebula animation
        StartNebulaAnimation();
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
        StopNebulaAnimation();
    }

    private void StartNebulaAnimation()
    {
        if (_isNebulaAnimating) return;
        _isNebulaAnimating = true;
        _nebulaCts = new CancellationTokenSource();
        var token = _nebulaCts.Token;

        // Start the three nebula animations with different speeds
        _ = Task.Run(async () =>
        {
            try
            {
                double phase1 = 0, phase2 = 0, phase3 = 0;
                while (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (Nebula1 != null && Nebula2 != null && Nebula3 != null)
                        {
                            Nebula1.TranslationX = 30 * Math.Sin(phase1);
                            Nebula1.TranslationY = -20 * Math.Cos(phase1);
                            Nebula1.Scale = 1 + 0.05 * Math.Sin(phase1);

                            Nebula2.TranslationX = -20 * Math.Sin(phase2);
                            Nebula2.TranslationY = 25 * Math.Cos(phase2);
                            Nebula2.Scale = 1 + 0.05 * Math.Cos(phase2);

                            Nebula3.TranslationX = -15 * Math.Sin(phase3);
                            Nebula3.TranslationY = -30 * Math.Cos(phase3);
                            Nebula3.Scale = 1 + 0.03 * Math.Sin(phase3);
                        }
                    });

                    phase1 += 0.02;
                    phase2 += 0.018;
                    phase3 += 0.022;
                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }, token);
    }

    private void StopNebulaAnimation()
    {
        if (!_isNebulaAnimating) return;
        _isNebulaAnimating = false;
        _nebulaCts?.Cancel();
        _nebulaCts?.Dispose();
        _nebulaCts = null;
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

    private void OnProfileUpdated(object? sender, EventArgs e)
    {
        PlayerNameLabel.Text = ProfileService.Active?.Name ?? "";
    }

    private void OnEditProfile(object sender, EventArgs e)
    {
        if (ProfileService.Active != null)
            EditProfilePopup.Show(ProfileService.Active);
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