using MantuGames.Models;
using MantuGames.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MantuGames.Views.Controls;

/// <summary>All badges: earned ones in colour, the rest greyed out with progress (e.g. 3/10).</summary>
public partial class BadgesPopup : ContentView
{
    public event EventHandler? StatisticsRequested;

    public BadgesPopup()
    {
        InitializeComponent();
        ThemeStyler.RegisterDialog(Card);
    }

    public void Show()
    {
        var earned = BadgeService.Earned;
        var snap = BadgeService.Snapshot();
        CountLabel.Text = $"{earned.Count} of {BadgeCatalog.All.Count} earned · +{BadgeCatalog.Reward} coins each";

        BadgesLayout.Children.Clear();
        var badges = BadgeCatalog.All.OrderByDescending(b => earned.Contains(b.Id)).ToList();
        for (int i = 0; i < badges.Count; i += 3)
        {
            var row = new Grid { ColumnSpacing = 10 };
            for (int c = 0; c < 3; c++) row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int c = 0; c < 3 && i + c < badges.Count; c++)
            {
                var tile = BuildTile(badges[i + c], earned.Contains(badges[i + c].Id), snap);
                Grid.SetColumn(tile, c);
                row.Children.Add(tile);
            }
            BadgesLayout.Children.Add(row);
        }

        IsVisible = true;
        Card.Opacity = 0;
        Card.Scale = 0.85;
        _ = Card.FadeToAsync(1, 180);
        _ = Card.ScaleToAsync(1, 260, Easing.SpringOut);
    }

    public void Hide() => IsVisible = false;

    private static View BuildTile(Badge badge, bool earned, PlayerSnapshot snap)
    {
        int progress = Math.Min(badge.Progress(snap), badge.Target);
        var accent = Color.FromArgb(ThemeColors.Hex(0, "#FFC83D"));

        var medal = new Border
        {
            WidthRequest = 64, HeightRequest = 64,
            StrokeShape = new RoundRectangle { CornerRadius = 32 },
            StrokeThickness = 3,
            Stroke = earned ? accent : Color.FromArgb("#D5D7E0"),
            BackgroundColor = earned ? accent.WithAlpha(0.18f) : Color.FromArgb("#F1F2F6"),
            HorizontalOptions = LayoutOptions.Center,
            Content = new Image { Source = badge.Icon, Margin = new Thickness(10), Opacity = earned ? 1 : 0.35 },
        };

        return new Border
        {
            BackgroundColor = Color.FromArgb("#F7F8FB"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(6, 10, 6, 8),
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    medal,
                    new FitLabel
                    {
                        Text = badge.Title, MinFontSize = 9, MaxLines = 1, Margin = new Thickness(0, 4, 0, 0),
                        FontFamily = SkinService.TitleFont, FontSize = 13,
                        TextColor = Color.FromArgb(earned ? "#111114" : "#8A8D99"),
                        HorizontalOptions = LayoutOptions.Center,
                    },
                    new Label
                    {
                        Text = earned ? "Earned!" : $"{progress}/{badge.Target}",
                        FontFamily = "Baloo2Medium", FontSize = 11,
                        TextColor = Color.FromArgb(earned ? "#168A50" : "#8A8D99"),
                        HorizontalOptions = LayoutOptions.Center,
                    },
                    new Label
                    {
                        Text = badge.Description, FontFamily = "Baloo2Medium", FontSize = 10,
                        TextColor = Color.FromArgb("#6E6E7C"), HorizontalTextAlignment = TextAlignment.Center,
                        MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation,
                    },
                },
            },
        };
    }

    private void OnStats(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
        StatisticsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnClose(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
    }
}
