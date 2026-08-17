using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views.Controls;

public partial class CoinShopPopup : ContentView
{
    /// <summary>Raised whenever a coin purchase adds coins to any game.</summary>
    public event EventHandler CoinsChanged;

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
        ShowStatus("");
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
        AnimateIn();
    }

    private void AnimateIn()
    {
        IsVisible = true;
        Overlay.IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
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

    private void ShowStatus(string text, bool ok = true)
    {
        StatusLabel.Text = text;
        StatusLabel.TextColor = ok ? Color.FromArgb("#34D399") : Color.FromArgb("#EF4444");
    }

    // ── Coin packs ─────────────────────────────────────────────────────────
    private void BuildPacks()
    {
        PacksLayout.Children.Clear();
        foreach (var (productId, coins) in IapService.CoinPacks)
        {
            var row = new Border
            {
                BackgroundColor = Color.FromArgb("#131A29"),
                Stroke = new SolidColorBrush(Color.FromArgb("#2EFFFFFF")),
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
                FontFamily = "Orbitron",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E7ECF5"),
                VerticalOptions = LayoutOptions.Center,
            };
            rowGrid.Children.Add(coinLabel);

            var buy = new Border
            {
                BackgroundColor = Color.FromArgb("#FF9F1C"),
                StrokeThickness = 0,
                Padding = new Thickness(12, 5),
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            buy.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };
            buy.Content = new Label
            {
                Text = "BUY",
                FontFamily = "Orbitron",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#160D02"),
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
        ShowStatus("Connecting to Google Play…", ok: true);
        bool purchased = await IapService.PurchaseAsync(productId);

        if (purchased)
        {
            if (_targetGameId != null)
            {
                var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == _targetGameId);
                CoinService.AddCoins(_targetGameId, coins);
                BalanceLabel.Text = $"{info?.Title ?? _targetGameId}: {CoinService.GetCoins(_targetGameId)} coins";
                ShowStatus($"{coins} coins added to {info?.Title ?? _targetGameId}!", ok: true);
                CoinsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _pendingCoins = coins;
                ShowStatus($"{coins} coins purchased! Now tap a game to transfer them.", ok: true);
            }
        }
        else
        {
            ShowStatus("Purchase unavailable right now. Please try again.", ok: false);
        }

        _busy = false;
    }

    private int _pendingCoins;

    // ── Game picker ─────────────────────────────────────────────────────────
    private void BuildGames()
    {
        GamesLayout.Children.Clear();

        foreach (var g in DashboardViewModel.Games)
        {
            var chip = new Border
            {
                BackgroundColor = Color.FromArgb("#131A29"),
                Stroke = new SolidColorBrush(Color.FromArgb("#2EFFFFFF")),
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
                FontFamily = "Inter",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E7ECF5"),
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
                        FontFamily = "Orbitron",
                        FontSize = 11,
                        TextColor = Color.FromArgb("#22D3EE"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    },
                }
            };
            var balance = balanceRow.Children[1] as Label;
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
        if (_pendingCoins <= 0)
        {
            ShowStatus("Buy a coin pack first, then pick a game.", ok: false);
            return;
        }

        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == gameId);
        CoinService.AddCoins(gameId, _pendingCoins);
        balanceLabel.Text = CoinService.GetCoins(gameId).ToString();
        ShowStatus($"{_pendingCoins} coins added to {info?.Title ?? gameId}!", ok: true);
        _pendingCoins = 0;
        CoinsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClose(object sender, TappedEventArgs e) => Hide();
}