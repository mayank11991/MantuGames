using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class CoinShopPopup : ContentView
{
    /// <summary>Raised whenever a coin purchase adds coins to any game.</summary>
    public event EventHandler? CoinsChanged;

    private bool _busy;

    public CoinShopPopup()
    {
        InitializeComponent();
        ThemeStyler.RegisterDialog(DialogCard);
        BuildPacks();
    }

    /// <summary>Shop opened from the dashboard / settings.</summary>
    public void Show()
    {
        MessageLabel.Text = "Coins work in every game.\nUse them for Show Solution when you're stuck.";
        RefreshBalance();
        ShowStatus("");
        AnimateIn();
    }

    /// <summary>Opened from "Show Solution" when the player can't afford it.</summary>
    public void ShowForGame(string gameId)
    {
        MessageLabel.Text = $"You need {CoinService.SolutionCost} coins for Show Solution.\nBeat levels, watch an ad, or grab a pack:";
        RefreshBalance();
        ShowStatus("");
        AnimateIn();
    }

    private void RefreshBalance()
    {
        BalanceLabel.Text = $"You have {CoinService.Balance} coins";
        RefreshFreeCoins();
    }

    // ── Free coins (opt-in rewarded ad) ─────────────────────────────────────
    private void RefreshFreeCoins()
    {
        int left = RewardService.FreeCoinsLeftToday;
        FreeCoinsRow.IsVisible = RewardService.CanOffer;
        FreeCoinsRow.Opacity = left > 0 ? 1 : 0.5;
        FreeCoinsTitle.Text = $"+{RewardService.FreeCoinsAmount} free coins";
        FreeCoinsHint.Text = left > 0
            ? $"Watch a short ad · {left} left today"
            : "All used today · come back tomorrow";
        if (FreeCoinsRow.IsVisible && left > 0) RewardService.Preload();
    }

    private async void OnFreeCoinsTapped(object? sender, TappedEventArgs e)
    {
        if (_busy || RewardService.FreeCoinsLeftToday <= 0) return;
        _busy = true;
        try
        {
            AudioService.Instance.Play("tap");
            ShowStatus("Loading ad…", ok: true);
            if (await RewardService.WatchForFreeCoinsAsync())
            {
                RefreshBalance();
                ShowStatus($"+{RewardService.FreeCoinsAmount} coins added!", ok: true);
                CoinsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ShowStatus("No reward this time. Please try again in a bit.", ok: false);
            }
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
            ShowStatus("No reward this time. Please try again in a bit.", ok: false);
        }
        finally
        {
            _busy = false;
        }
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

    private void ShowStatus(string text, bool ok = true)
    {
        StatusLabel.Text = text;
        StatusLabel.TextColor = ok ? Color.FromArgb("#20B26B") : Color.FromArgb("#FF4F6D");
    }

    // ── Coin packs ─────────────────────────────────────────────────────────
    private void BuildPacks()
    {
        PacksLayout.Children.Clear();
        foreach (var (productId, coins) in IapService.CoinPacks)
        {
            var row = new Border
            {
                BackgroundColor = Color.FromArgb("#F5F6FA"),
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
                TextColor = Color.FromArgb("#111114"),
                VerticalOptions = LayoutOptions.Center,
            };
            rowGrid.Children.Add(coinLabel);

            var buy = new Border
            {
                BackgroundColor = Color.FromArgb("#3DC86A"),
                Stroke = Colors.White,
                StrokeThickness = 2,
                Padding = new Thickness(16, 6),
                Margin = new Thickness(0, 0, 0, 4),
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                Shadow = new Shadow { Brush = Color.FromArgb("#239448"), Offset = new Point(0, 4), Radius = 0, Opacity = 1 },
            };
            buy.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 };
            buy.Content = new Label
            {
                Text = "BUY",
                FontFamily = SkinService.TitleFont,
                FontSize = 15,
                TextColor = Colors.White,
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
                // IapService has already credited the wallet.
                RefreshBalance();
                ShowStatus($"+{coins} coins added!", ok: true);
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

    private void OnClose(object? sender, TappedEventArgs e) => Hide();
}
