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
        SettingsPopup.CoinShopRequested += OnCoinShopClicked;
        CoinShopPopup.CoinsChanged += OnCoinsChanged;
        ProfilePicker.ProfileSelected += OnProfileSelected;
        ProfilePicker.AddProfileRequested += (s, e) => ProfilePopup.Show();
        ProfilePopup.ProfileCreated += (s, p) => ProfilePicker.Show();
        EditProfilePopup.ProfileUpdated += OnProfileUpdated;

        // Restore remove_ads after reinstall and deliver any paid-but-unconsumed coin packs
        IapService.UnassignedCoinsChanged += (s, e) => MainThread.BeginInvokeOnMainThread(RefreshCoins);
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

        // Netflix-style gate: a profile must be picked or created — no skip.
        if (ProfileService.Active == null)
        {
            ProfilePicker.Show();
        }

        SettingsPopup.ProgressReset += OnProgressReset;
        RefreshCoins();

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
        string avatar = string.IsNullOrEmpty(active?.AvatarImage) ? "a1.png" : active!.AvatarImage;
        HeaderAvatar.Source = avatar;
        HeroAvatar.Source = avatar;
        HeaderPlayerName.Text = string.IsNullOrWhiteSpace(active?.Name) ? "Player" : active!.Name;
        CoinBalanceLabel.Text = CoinService.GetTotalCoins().ToString();

        // Progress strip: stars, wins and completed levels across all games
        int stars = 0, levels = 0;
        foreach (var g in DashboardViewModel.Games)
        {
            foreach (var lp in ProgressService.Instance.GetLevels(g.Id, 50))
            {
                if (!lp.IsCompleted) continue;
                levels++;
                stars += lp.Stars;
            }
        }
        StarsStat.Text = stars.ToString();
        LevelsStat.Text = levels.ToString();
        WinsStat.Text = StatsService.GetTotals().Won.ToString();
    }

    private void OnProgressReset(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnCoinsChanged(object? sender, EventArgs e)
    {
        RefreshCoins();
    }

    private void OnProfileSelected(object? sender, Models.PlayerProfile profile)
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

    private void OnSwitchProfile(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        ProfilePicker.Show();
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
        ProfilePicker.Show();
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
            FontFamily = "Display",
            FontSize = 46,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(-2, 0, 0, -26),
            Shadow = new Shadow { Brush = Color.FromArgb("#1F2A44"), Offset = new Point(2, 3), Radius = 0, Opacity = 1 },
        };

        var title = new Label
        {
            Text = game.Title,
            FontFamily = "Baloo2ExtraBold",
            FontSize = 17,
            TextColor = Color.FromArgb("#111114"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            Margin = new Thickness(30, 4, 2, 0),
        };

        var tagline = new Label
        {
            Text = game.Description,
            FontFamily = "Baloo2Medium",
            FontSize = 12.5,
            TextColor = Color.FromArgb("#6E6E7C"),
            MaxLines = 2,
            LineBreakMode = LineBreakMode.TailTruncation,
            Margin = new Thickness(30, -2, 2, 2),
        };

        var thumbHost = new Grid { Children = { thumb, numberLabel } };

        // Chunky green "PLAY" call-to-action, like a candy button with a darker lip.
        var play = new Border
        {
            BackgroundColor = Color.FromArgb("#3DC86A"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 },
            Padding = new Thickness(14, 6, 16, 7),
            Margin = new Thickness(0, 8, 0, 4),
            HorizontalOptions = LayoutOptions.Fill,
            Shadow = new Shadow { Brush = Color.FromArgb("#239448"), Offset = new Point(0, 4), Radius = 0, Opacity = 1 },
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Image { Source = "ic_play_white.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center },
                    new Label { Text = "PLAY", FontFamily = "Display", FontSize = 17, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center },
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