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
        HeaderAvatar.Source = string.IsNullOrEmpty(active?.AvatarImage) ? "a1.png" : active!.AvatarImage;
        HeaderPlayerName.Text = string.IsNullOrWhiteSpace(active?.Name) ? "Player" : active!.Name;
        CoinBalanceLabel.Text = CoinService.GetTotalCoins().ToString();
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
        var games = DashboardViewModel.Games.ToList();
        if (!string.IsNullOrEmpty(filter))
        {
            games = games.Where(g =>
                g.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                g.Id.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        const int cols = 3;
        const double spacing = 12;
        const double sideMargin = 28; // ScrollView left + right margin (14 + 14)

        double screenDp = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        double cardW = Math.Min((screenDp - sideMargin - spacing * (cols - 1)) / cols, 210);
        double thumbH = cardW * 0.68;
        double cardH = thumbH + 40;

        // Pastel accents shown behind each thumbnail, shuffled every load.
        var pastelPool = new List<string>
        {
            "#FFD6E0", "#C5E8FF", "#D9F2D0", "#FFF3B0", "#E6D6FF",
            "#FFDCC5", "#D6F5F5", "#F3D1F4", "#E2F0CB", "#FFE4B8", "#D9E2FF"
        };
        var rng = new Random();
        var pastels = pastelPool.OrderBy(_ => rng.Next()).ToList();

        for (int i = 0; i < games.Count; i += cols)
        {
            var rowGames = games.Skip(i).Take(cols).ToList();
            int actualCols = rowGames.Count;

            var grid = new Grid
            {
                ColumnSpacing = spacing,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = actualCols * cardW + (actualCols - 1) * spacing
            };
            for (int c = 0; c < actualCols; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition(cardW));

            for (int c = 0; c < actualCols; c++)
            {
                var game = rowGames[c];

                var thumbImg = new Image
                {
                    Source = game.ImageName,
                    Aspect = Aspect.AspectFill,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };

                var thumbBorder = new Border
                {
                    Stroke = new SolidColorBrush(Color.FromArgb("#B3FFFFFF")),
                    StrokeThickness = 2,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                    BackgroundColor = Color.FromArgb(pastels[(i + c) % pastels.Count]),
                    HeightRequest = thumbH,
                    Content = thumbImg
                };

                var nameLabel = new Label
                {
                    Text = game.Title,
                    FontFamily = "MadimiOne",
                    FontSize = 10.5,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#3D2C4E"),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    MaxLines = 2,
                    LineBreakMode = LineBreakMode.WordWrap
                };

                var labelBar = new Grid
                {
                    HeightRequest = 34,
                    Padding = new Thickness(2, 2, 2, 2),
                    Children = { nameLabel }
                };

                var cardContent = new Grid
                {
                    Padding = new Thickness(6, 6, 6, 0),
                    RowDefinitions =
                    {
                        new RowDefinition { Height = thumbH },
                        new RowDefinition { Height = GridLength.Star }
                    }
                };
                Grid.SetRow(thumbBorder, 0);
                cardContent.Children.Add(thumbBorder);
                Grid.SetRow(labelBar, 1);
                cardContent.Children.Add(labelBar);

                var card = new Border
                {
                    Padding = new Thickness(0),
                    Stroke = new SolidColorBrush(Color.FromArgb("#B3FFFFFF")),
                    StrokeThickness = 2,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
                    WidthRequest = cardW,
                    HeightRequest = cardH,
                    BackgroundColor = Color.FromArgb("#FFFDFA"),
                    Content = cardContent,
                    Shadow = new Shadow { Brush = Color.FromArgb("#8A6FA8"), Offset = new Point(0, 8), Opacity = 0.30f, Radius = 14 }
                };

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) =>
                {
                    AudioService.Instance.Play("tap");
                    VibrationHelper.Click();
                    await Shell.Current.GoToAsync($"levelmap?gameId={game.Id}");
                };
                card.GestureRecognizers.Add(tap);
                Grid.SetColumn(card, c);

                grid.Children.Add(card);
            }

            GamesFlex.Children.Add(grid);
        }
    }
}