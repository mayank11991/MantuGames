using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MantuGames.Views.Controls;

/// <summary>
/// Skin picker and shop. Free and owned skins can be switched on with one tap;
/// paid skins (and the all-skins bundle) are bought through Google Play.
/// </summary>
public partial class SkinShopPopup : ContentView
{
    private const string FallbackPrice = "BUY";

    private Dictionary<string, string> _prices = new();
    private bool _busy;

    public SkinShopPopup()
    {
        InitializeComponent();
        SkinService.Changed += (s, e) => { if (IsVisible) Rebuild(); };
    }

    public void Show()
    {
        StatusLabel.Text = "";
        Rebuild();
        IsVisible = true;
        Card.Opacity = 0;
        Card.Scale = 0.85;
        _ = Card.FadeToAsync(1, 180);
        _ = Card.ScaleToAsync(1, 260, Easing.SpringOut);
        _ = LoadPricesAsync();
    }

    public void Hide() => IsVisible = false;

    private async Task LoadPricesAsync()
    {
        var ids = SkinCatalog.PaidProductIds.Append(SkinCatalog.BundleProductId);
        var prices = await IapService.GetPricesAsync(ids);
        if (prices.Count == 0) return;
        _prices = prices;
        if (IsVisible) Rebuild();
    }

    private string PriceFor(string productId) =>
        _prices.TryGetValue(productId, out var p) ? p : FallbackPrice;

    // ── Layout ─────────────────────────────────────────────────────────────
    private void Rebuild()
    {
        BundleRow.IsVisible = !SkinService.AllPaidOwned;
        BundlePrice.Text = PriceFor(SkinCatalog.BundleProductId);

        SkinsLayout.Children.Clear();
        var skins = SkinCatalog.All;
        for (int i = 0; i < skins.Count; i += 2)
        {
            var row = new Grid { ColumnSpacing = 12 };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int c = 0; c < 2 && i + c < skins.Count; c++)
            {
                var tile = BuildTile(skins[i + c]);
                Grid.SetColumn(tile, c);
                row.Children.Add(tile);
            }
            SkinsLayout.Children.Add(row);
        }
    }

    private View BuildTile(Skin skin)
    {
        bool owned = SkinService.IsOwned(skin);
        bool inUse = SkinService.Current.Id == skin.Id;

        var preview = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HeightRequest = 96,
            Background = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(skin.ShellTop), 0f),
                new GradientStop(Color.FromArgb(skin.ShellMid), 1f),
            }, new Point(0, 0), new Point(0, 1)),
            Content = new Grid
            {
                Children =
                {
                    new Image { Source = skin.Mascot, HeightRequest = 86, WidthRequest = 86,
                                HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
                    // Accent chip: the colour used for game pieces.
                    new Border
                    {
                        BackgroundColor = Color.FromArgb(skin.Accent),
                        StrokeThickness = 2, Stroke = Colors.White,
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        WidthRequest = 18, HeightRequest = 18,
                        HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End,
                        Margin = new Thickness(0, 0, 8, 8),
                    },
                    new Image { Source = "ic_lock_ink.png", WidthRequest = 18, HeightRequest = 18,
                                IsVisible = !owned, Opacity = 0.75,
                                HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start,
                                Margin = new Thickness(8) },
                },
            },
        };

        var name = new FitLabel
        {
            Text = skin.Name, MinFontSize = 10, MaxLines = 1,
            FontFamily = "Baloo2ExtraBold", FontSize = 14, TextColor = Color.FromArgb("#111114"),
            HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 6, 0, 2),
        };

        (string text, string style, bool dark) = inUse
            ? ("IN USE", "CtaPillGreen", false)
            : owned
                ? (skin.IsFree ? "FREE · USE" : "USE", "CtaPillBlue", false)
                : (PriceFor(skin.ProductId!), "CtaPillYellow", true);

        var action = new Border
        {
            Style = (Style)Application.Current!.Resources[style],
            Padding = new Thickness(10, 5),
            HorizontalOptions = LayoutOptions.Fill,
            InputTransparent = true,
            Content = new Label
            {
                Text = text,
                Style = (Style)Application.Current!.Resources[dark ? "CtaPillLabelDark" : "CtaPillLabel"],
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
            },
        };

        var tile = new Border
        {
            BackgroundColor = Color.FromArgb("#F5F6FA"),
            StrokeThickness = inUse ? 3 : 0,
            Stroke = Color.FromArgb("#3DC86A"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(8, 8, 8, 6),
            Content = new VerticalStackLayout { Spacing = 0, Children = { preview, name, action } },
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await OnSkinTappedAsync(skin);
        tile.GestureRecognizers.Add(tap);
        return tile;
    }

    // ── Actions ────────────────────────────────────────────────────────────
    private async Task OnSkinTappedAsync(Skin skin)
    {
        if (_busy) return;
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();

        if (SkinService.IsOwned(skin))
        {
            if (SkinService.Use(skin))
                ShowStatus($"{skin.Name} is on!", ok: true);
            return;
        }

        await BuyAsync(skin.ProductId!, () =>
        {
            SkinService.Use(skin);
            return $"{skin.Name} unlocked!";
        });
    }

    private async void OnBundleTapped(object? sender, TappedEventArgs e)
    {
        if (_busy) return;
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        await BuyAsync(SkinCatalog.BundleProductId, () => "All skins unlocked! Pick your favourite.");
    }

    private async Task BuyAsync(string productId, Func<string> onPurchased)
    {
        _busy = true;
        try
        {
            ShowStatus("Connecting to Google Play…", ok: true);
            var outcome = await IapService.PurchaseAsync(productId);
            switch (outcome)
            {
                case PurchaseOutcome.Purchased:
                    // Mark owned here too, in case Play reported "already owned".
                    SkinService.MarkOwned(productId);
                    ShowStatus(onPurchased(), ok: true);
                    AudioService.Instance.Play("win");
                    break;
                case PurchaseOutcome.Pending:
                    ShowStatus("Payment pending. The skin unlocks once Google Play confirms it.", ok: true);
                    break;
                default:
                    ShowStatus("Purchase not completed. You were not charged.", ok: false);
                    break;
            }
            Rebuild();
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
            ShowStatus("Purchase not completed. You were not charged.", ok: false);
        }
        finally
        {
            _busy = false;
        }
    }

    private void ShowStatus(string text, bool ok)
    {
        StatusLabel.Text = text;
        StatusLabel.TextColor = Color.FromArgb(ok ? "#20B26B" : "#FF4F6D");
    }

    private void OnClose(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
    }
}
