using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views.Controls;

public partial class StatsPopup : ContentView
{
    public StatsPopup()
    {
        InitializeComponent();
    }

    public void Show()
    {
        var totals = StatsService.GetTotals();
        TotalPlayedLabel.Text = $"{totals.Played} played";
        TotalWonLabel.Text = $"{totals.Won} won";
        TotalRateLabel.Text = $"{StatsService.WinRate(totals.Played, totals.Won):0}% win rate";

        StatsGrid.Children.Clear();
        StatsGrid.RowDefinitions.Clear();
        int row = 0;
        foreach (var g in DashboardViewModel.Games)
        {
            var s = StatsService.GetStats(g.Id);

            StatsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new HorizontalStackLayout
            {
                Spacing = 8,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Image
                    {
                        Source = g.ImageName,
                        WidthRequest = 22,
                        HeightRequest = 22,
                        VerticalOptions = LayoutOptions.Center,
                    },
                    new Label
                    {
                        Text = g.Title,
                        FontFamily = "MadimiOne",
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#111114"),
                        VerticalOptions = LayoutOptions.Center,
                    },
                }
            };
            StatsGrid.Children.Add(title);
            Grid.SetRow((BindableObject)StatsGrid.Children[^1], row);

            StatsGrid.Children.Add(new Label
            {
                Text = $"{s.Won}/{s.Played}",
                FontFamily = "MadimiOne",
                FontSize = 12,
                TextColor = Color.FromArgb("#6E6E7C"),
                VerticalOptions = LayoutOptions.Center,
            });
            Grid.SetRow((BindableObject)StatsGrid.Children[^1], row);
            Grid.SetColumn((BindableObject)StatsGrid.Children[^1], 1);

            StatsGrid.Children.Add(new Label
            {
                Text = StatsService.WinRate(s.Played, s.Won) > 0 ? $"{StatsService.WinRate(s.Played, s.Won):0}%" : "—",
                FontFamily = "MadimiOne",
                FontSize = 12,
                TextColor = Color.FromArgb("#20B26B"),
                VerticalOptions = LayoutOptions.Center,
            });
            Grid.SetRow((BindableObject)StatsGrid.Children[^1], row);
            Grid.SetColumn((BindableObject)StatsGrid.Children[^1], 2);

            row++;
        }

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

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
    }

    private void OnClose(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
    }
}