using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views.Controls;

public partial class CoinShopPopup : ContentView
{
    /// <summary>Raised whenever a coin purchase adds coins to any game.</summary>
    public event EventHandler? CoinsChanged;

    private string? _targetGameId;
    private bool _busy;

    public CoinShopPopup()
    {
        InitializeComponent();
        BuildPacks();
        BuildGames();
    }

    /// <summary>Shop mode — buy a pack, then pick which game receives the coins.</summary>
    public void Show()
    {
        _targetGameId = null;
        MessageLabel.Text = "Buy coins and transfer them to any game.\nEach game keeps its own coin balance.";
        BalanceLabel.Text = $"Total: {CoinService.GetTotalCoins()} coins";
        PickerTitle.IsVisible = true;
        GamesLayout.IsVisible = true;
        BuildGames();
        ShowUnassignedHint();
        AnimateIn();
    }

    /// <summary>Insufficient-coins mode — used by "Show Solution". Purchase adds coins directly to the given game.</summary>
    public void ShowForGame(string gameId)
    {
        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == gameId);
        string gameName = info?.Title ?? gameId;
        _targetGameId = gameId;
        MessageLabel.Text = $"Not enough coins to use Show Solution in {gameName}.\nShow Solution costs {CoinService.SolutionCost} coins.";
        BalanceLabel.Text = $"{gameName}: {CoinService.GetCoins(gameId)} coins";
        PickerTitle.IsVisible = false;
        GamesLayout.IsVisible = false;
        ShowStatus("");

        // Purchased coins still waiting in the bank go straight to the game that needs them.
        int moved = IapService.AssignUnassignedCoins(gameId);
        if (moved > 0)
        {
            BalanceLabel.Text = $"{gameName}: {CoinService.GetCoins(gameId)} coins";
            ShowStatus($"{moved} purchased coins added to {gameName}!", ok: true);
            CoinsChanged?.Invoke(this, EventArgs.Empty);
        }

        AnimateIn();
    }

    private void AnimateIn()
    {
        IsVisible = true;
        Overlay.IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        this.FadeToAsync(1, 200, Easing.CubicOut);
        this.ScaleToAsync(1, 200, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeToAsync(0, 150, Easing.CubicIn);
        this.ScaleToAsync(0.8, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void ShowUnassignedHint()
    {
        int waiting = IapService.UnassignedCoins;
        if (waiting > 0)
            ShowStatus($"{waiting} purchased coins are waiting. Tap a game to add them.", ok: true);
        else
            ShowStatus("");
    }

    private void ShowStatus(string text, bool ok = true)
    {
        StatusLabel.Text = text;
        StatusLabel.TextColor = ok ? Color.FromArgb("#1F8A70") : Color.FromArgb("#E0566F");
    }

    // ── Coin packs ─────────────────────────────────────────────────────────
    private void BuildPacks()
    {
        PacksLayout.Children.Clear();
        foreach (var (productId, coins) in IapService.CoinPacks)
        {
            var row = new Border
            {
                BackgroundColor = Color.FromArgb("#FFF4E8"),
                Stroke = new SolidColorBrush(Color.FromArgb("#B3FFFFFF")),
                StrokeThickness = 1,
                Padding = new Thickness(14, 8),
            };
            row.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 };

            var rowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };

            var coinLabel = new Label
            {
                Text = $"{coins} Coins",
                FontFamily = "MadimiOne",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3D2C4E"),
                VerticalOptions = LayoutOptions.Center,
            };
            rowGrid.Children.Add(coinLabel);

            var buy = new Border
            {
                BackgroundColor = Color.FromArgb("#FFA94D"),
                StrokeThickness = 0,
                Padding = new Thickness(12, 5),
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            buy.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };
            buy.Content = new Label
            {
                Text = "BUY",
                FontFamily = "MadimiOne",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3D2C4E"),
                HorizontalOptions = LayoutOptions.Center,
            };
            Grid.SetColumn(buy, 1);
            rowGrid.Children.Add(buy);

            row.Content = rowGrid;

            string capturedProductId = productId;
            int capturedCoins = coins;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await OnPackTappedAsync(capturedProductId, capturedCoins);
            row.GestureRecognizers.Add(tap);

            PacksLayout.Children.Add(row);
        }
    }

    private async Task OnPackTappedAsync(string productId, int coins)
    {
        if (_busy) return;
        _busy = true;
        try
        {
            ShowStatus("Connecting to Google Play…", ok: true);
            var outcome = await IapService.PurchaseAsync(productId);

            if (outcome == PurchaseOutcome.Purchased)
            {
                // Coins are already saved in the unassigned bank by IapService.
                if (_targetGameId != null)
                {
                    var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == _targetGameId);
                    int moved = IapService.AssignUnassignedCoins(_targetGameId);
                    BalanceLabel.Text = $"{info?.Title ?? _targetGameId}: {CoinService.GetCoins(_targetGameId)} coins";
                    ShowStatus($"{moved} coins added to {info?.Title ?? _targetGameId}!", ok: true);
                }
                else
                {
                    BuildGames();
                    ShowStatus($"{IapService.UnassignedCoins} coins ready! Tap a game to add them.", ok: true);
                }
                CoinsChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (outcome == PurchaseOutcome.Pending)
            {
                ShowStatus("Payment pending. Coins will be added automatically once Google Play confirms it.", ok: true);
            }
            else
            {
                ShowStatus("Purchase not completed. You were not charged for this attempt.", ok: false);
            }
        }
        finally
        {
            _busy = false;
        }
    }

    // ── Game picker ─────────────────────────────────────────────────────────
    private void BuildGames()
    {
        GamesLayout.Children.Clear();

        foreach (var g in DashboardViewModel.Games)
        {
            var chip = new Border
            {
                BackgroundColor = Color.FromArgb("#FFF4E8"),
                Stroke = new SolidColorBrush(Color.FromArgb("#B3FFFFFF")),
                StrokeThickness = 1,
                Padding = new Thickness(10, 8),
                Margin = new Thickness(3),
            };
            chip.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
            FlexLayout.SetBasis(chip, new Microsoft.Maui.Layouts.FlexBasis(0.48f, true));

            var stack = new VerticalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center,
            };
            stack.Children.Add(new Label
            {
                Text = g.Title,
                FontFamily = "MadimiOne",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3D2C4E"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
            });
            var balanceRow = new HorizontalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Image
                    {
                        Source = "coin.png",
                        WidthRequest = 13,
                        HeightRequest = 13,
                        VerticalOptions = LayoutOptions.Center,
                    },
                    new Label
                    {
                        Text = CoinService.GetCoins(g.Id).ToString(),
                        FontFamily = "MadimiOne",
                        FontSize = 11,
                        TextColor = Color.FromArgb("#6D4AFF"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    },
                }
            };
            var balance = (Label)balanceRow.Children[1];
            stack.Children.Add(balanceRow);
            chip.Content = stack;

            string capturedId = g.Id;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => OnGameTapped(capturedId, balance);
            chip.GestureRecognizers.Add(tap);

            GamesLayout.Children.Add(chip);
        }
    }

    private void OnGameTapped(string gameId, Label balanceLabel)
    {
        if (IapService.UnassignedCoins <= 0)
        {
            ShowStatus("Buy a coin pack first, then pick a game.", ok: false);
            return;
        }

        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == gameId);
        int moved = IapService.AssignUnassignedCoins(gameId);
        balanceLabel.Text = CoinService.GetCoins(gameId).ToString();
        ShowStatus($"{moved} coins added to {info?.Title ?? gameId}!", ok: true);
        CoinsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClose(object? sender, TappedEventArgs e) => Hide();
}