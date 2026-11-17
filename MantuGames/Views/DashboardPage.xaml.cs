using System.Collections.ObjectModel;
using Microsoft.Maui.Devices;
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
        SettingsPopup.ProfileSwitchRequested += OnSwitchProfile;
        EditProfilePopup.SkinShopRequested += (s, e) => SkinShop.Show();
        SettingsPopup.CoinShopRequested += OnCoinShopClicked;
        CoinShopPopup.CoinsChanged += OnCoinsChanged;
        SkinService.Changed += (s, e) => RefreshSkin();
        EditProfilePopup.ProfileUpdated += OnProfileUpdated;

        // Restore remove_ads after reinstall and deliver any paid-but-unconsumed coin packs
        CoinService.CoinsChanged += (s, e) => MainThread.BeginInvokeOnMainThread(RefreshCoins);
        _ = IapService.ProcessOutstandingAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PopulateGamesFlex();

        // Show a friendly notice if the previous session crashed
        if (!Preferences.Get("crash_notice_shown", false))
        {
            Preferences.Set("crash_notice_shown", true);
            var report = CrashGuardService.TakePendingReport();
            if (report != null)
            {
                _ = Application.Current!.Windows[0].Page!.DisplayAlertAsync(
                    "Welcome back!",
                    "The app closed unexpectedly last time. Your progress is safe — we've noted the issue and it will be fixed soon.",
                    "OK");
            }
        }

        // Check for app updates (once per session)
        _ = CheckForUpdatesAsync();

        _ = ShowStartGatesAsync();

        SettingsPopup.ProgressReset += OnProgressReset;
        RefreshCoins();

        // Start background music
        AudioService.Instance.StartMusic();
    }

    // First launch only: pick a skin, a name and a year of birth (ads wait for the age).
    private async Task ShowStartGatesAsync()
    {
        try
        {
            if (!ProfileService.IsOnboarded && !Onboarding.IsVisible)
            {
                await Onboarding.ShowAsync();
                RefreshCoins();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in start gates: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Block the back key during the first-launch setup: it can't be skipped.
        if (Onboarding.IsVisible)
            return true;
        if (SkinShop.IsVisible)
        {
            SkinShop.Hide();
            return true;
        }
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
        catch
        {
        }
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
        SettingsPopup.RefreshProfile();

        var active = ProfileService.Active;
        // The player's picture is their skin's character.
        HeaderAvatar.Source = SkinService.Current.Mascot;
        HeaderAvatarRing.BackgroundColor = Color.FromArgb(SkinService.Current.ShellMid);
        HeaderPlayerName.Text = string.IsNullOrWhiteSpace(active?.Name) ? "Player" : active!.Name;
        CoinBalanceLabel.Text = CoinService.Balance.ToString();

        // Progress strip: completed levels and wins across all games. Stars are per game
        // (level screen) and coins are the wallet pill, so neither is repeated here.
        int levels = 0;
        foreach (var g in DashboardViewModel.Games)
            levels += ProgressService.Instance.GetLevels(g.Id, 50).Count(lp => lp.IsCompleted);
        LevelsStat.Text = levels.ToString();
        WinsStat.Text = StatsService.GetTotals().Won.ToString();

        RefreshDaily();
        RefreshSkin();
    }

    // ── Skins ───────────────────────────────────────────────────────────────
    private void RefreshSkin()
    {
        PopulateGamesFlex(SearchEntry.Text?.Trim() ?? "");
        var skin = SkinService.Current;
        HeroMascot.Source = skin.Mascot;
        SkinPillMascot.Source = skin.Mascot;
        HeaderAvatar.Source = skin.Mascot;
        HeaderAvatarRing.BackgroundColor = Color.FromArgb(skin.ShellMid);
        SettingsPopup.RefreshProfile();
    }

    private async void OnSkinsClicked(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        SkinShop.Show();
        try
        {
            await HeroMascot.ScaleToAsync(1.15, 90);
            await HeroMascot.ScaleToAsync(1, 120);
        }
        catch { }
    }

    // ── Daily challenge card ────────────────────────────────────────────────
    private void RefreshDaily()
    {
        var game = DailyService.TodayGame;
        DailyThumb.BackgroundColor = Color.FromArgb(game.CardColor);
        DailyImage.Source = game.ImageName;
        StreakLabel.Text = $"🔥 {DailyService.CurrentStreak}";

        if (DailyService.IsDoneToday)
        {
            DailyCard.BackgroundColor = Color.FromArgb("#CDF6E0");
            DailyTitle.Text = "Done for today! ✓";
            DailySub.Text = "A new challenge arrives tomorrow";
        }
        else
        {
            DailyCard.BackgroundColor = Color.FromArgb("#FFE0C2");
            DailyTitle.Text = $"Win a level in {game.Title}";
            DailySub.Text = DailyService.CurrentStreak > 0
                ? $"+{DailyService.RewardForToday} coins · keep your streak going!"
                : $"+{DailyService.RewardForToday} coins · start a streak!";
        }
    }

    private async void OnDailyTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await DailyCard.ScaleToAsync(0.97, 70);
            await DailyCard.ScaleToAsync(1, 90);
            await Shell.Current.GoToAsync($"levelmap?gameId={DailyService.TodayGame.Id}");
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
    }

    private void OnProgressReset(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnCoinsChanged(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnProfileUpdated(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnEditProfile(object? sender, EventArgs e)
    {
        if (ProfileService.Active != null)
            EditProfilePopup.Show(ProfileService.Active);
    }

    private void OnStatisticsRequested(object? sender, EventArgs e)
    {
        ShowStats();
    }

    private void OnUpdateCheckRequested(object? sender, EventArgs e)
    {
        _ = CheckForUpdatesAsync(force: true);
    }

    // One player per device: the avatar opens the name/skin editor.
    private void OpenProfile()
    {
        if (ProfileService.Active != null)
            EditProfilePopup.Show(ProfileService.Active);
    }

    private void OnSwitchProfile(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        OpenProfile();
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

    private void OnNavHomeClicked(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        _ = GamesScroll.ScrollToAsync(0, 0, true);
    }

    private void OnNavStatsClicked(object? sender, TappedEventArgs e)
    {
        ShowStats();
    }

    private void OnNavSettingsClicked(object? sender, TappedEventArgs e)
    {
        OnSettingsClicked(sender, e);
    }

    private void OnNavProfileClicked(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        OpenProfile();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string q = e.NewTextValue?.Trim() ?? "";
        PopulateGamesFlex(q);
    }

    private void PopulateGamesFlex(string filter = "")
    {
        GamesFlex.Children.Clear();
        var all = DashboardViewModel.Games.ToList();
        var games = all;
        if (!string.IsNullOrEmpty(filter))
        {
            games = games.Where(g =>
                g.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                g.Id.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (games.Count == 0)
        {
            GamesFlex.Children.Add(new Label
            {
                Text = "No game matches that search.",
                Style = (Style)Application.Current!.Resources["PopMutedLabel"],
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20),
            });
            return;
        }

        const int cols = 2;
        const double spacing = 14;
        const double sideMargin = 40; // page padding 20 + 20

        double screenDp = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        double cardW = Math.Min((screenDp - sideMargin - spacing * (cols - 1)) / cols, 240);
        // Thumbnails are 3:4 portrait art with their own title band: show them whole.
        double thumbH = (cardW - 20) * 4 / 3;

        for (int i = 0; i < games.Count; i += cols)
        {
            var row = new Grid { ColumnSpacing = spacing };
            for (int c = 0; c < cols; c++)
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            foreach (var (game, c) in games.Skip(i).Take(cols).Select((g, idx) => (g, idx)))
            {
                int number = all.IndexOf(game) + 1;
                var card = BuildGameCard(game, number, thumbH);
                Grid.SetColumn(card, c);
                row.Children.Add(card);
            }

            GamesFlex.Children.Add(row);
        }
    }

    /// <summary>White card: tilted thumbnail on the game's accent colour, a big
    /// Anton number breaking its corner, then title and tagline.</summary>
    private View BuildGameCard(GameInfo game, int number, double thumbH)
    {
        var accent = Color.FromArgb(game.CardColor);

        var thumb = new Border
        {
            BackgroundColor = accent,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
            HeightRequest = thumbH,
            Padding = 0,
            Shadow = null!,
            Content = new Image { Source = game.ImageName, Aspect = Aspect.AspectFill },
        };

        var numberLabel = new Label
        {
            Text = number.ToString(),
            FontFamily = SkinService.TitleFont,
            FontSize = 46,
            TextColor = Color.FromArgb("#1F2A44"),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(-2, 0, 0, -26),
            Shadow = new Shadow { Brush = Colors.White, Offset = new Point(2, 2), Radius = 0, Opacity = 1 },
        };

        var title = new Controls.FitLabel
        {
            MinFontSize = 11,
            Text = game.Title,
            FontFamily = "Baloo2ExtraBold",
            FontSize = 15.5,
            TextColor = Color.FromArgb("#111114"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            // Clear the big corner number; two digits are wider.
            Margin = new Thickness(number < 10 ? 30 : 46, 4, 2, 0),
        };

        var tagline = new Controls.FitLabel
        {
            MinFontSize = 9,
            Text = game.Description,
            FontFamily = "Baloo2Medium",
            FontSize = 12,
            TextColor = Color.FromArgb("#6E6E7C"),
            MaxLines = 1,
            LineBreakMode = LineBreakMode.TailTruncation,
            Margin = new Thickness(number < 10 ? 30 : 46, -2, 2, 2),
        };

        var thumbHost = new Grid { Children = { thumb, numberLabel } };

        // Chunky green "PLAY" call-to-action, like a candy button with a darker lip.
        var play = new Border
        {
            // Green PLAY for free skins; premium skins use their button colour.
            BackgroundColor = Color.FromArgb(SkinService.Current.Theme != null ? SkinService.Current.Button : "#3DC86A"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 },
            Padding = new Thickness(14, 6, 16, 7),
            Margin = new Thickness(0, 8, 0, 8),
            HorizontalOptions = LayoutOptions.Fill,
            Shadow = new Shadow
            {
                Brush = Color.FromArgb(SkinService.Current.Theme != null ? ThemeColors.Shade(0, "#239448") : "#239448"),
                Offset = new Point(0, 4), Radius = 0, Opacity = 1,
            },
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Image { Source = "ic_play_white.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center },
                    new Label { Text = "PLAY", FontFamily = SkinService.TitleFont, FontSize = 17, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center },
                },
            },
        };

        var card = new Border
        {
            Style = (Style)Application.Current!.Resources["PopCard"],
            Padding = new Thickness(10, 10, 10, 12),
            Content = new VerticalStackLayout { Spacing = 0, Children = { thumbHost, title, tagline, play } },
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await card.ScaleToAsync(0.96, 70);
            await card.ScaleToAsync(1, 90);
            await Shell.Current.GoToAsync($"levelmap?gameId={game.Id}");
        };
        card.GestureRecognizers.Add(tap);
        return card;
    }
}