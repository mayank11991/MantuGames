using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class LevelMapPage : ContentPage
{
    private string _gameId;
    private string _gameRoute;
    private bool _isNavigating;

    public string GameId
    {
        get => _gameId;
        set
        {
            _gameId = Uri.UnescapeDataString(value ?? "");
            BuildMap();
        }
    }

    public LevelMapPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    private Color _gameAccentColor = Color.FromArgb("#8ECCCC");

    private void BuildMap()
    {
        if (string.IsNullOrEmpty(_gameId)) return;

        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == _gameId);
        _gameRoute = info?.Route ?? _gameId;

        if (info?.CardColor != null)
        {
            _gameAccentColor = Color.FromArgb(info.CardColor);
            BackgroundColor = _gameAccentColor;
        }

        GameTitleLabel.Text = info?.Title ?? _gameId;

        int totalPts = ProgressService.Instance.GetTotalPoints(_gameId);
        PointsLabel.Text = $"⭐ {totalPts} pts";

        LevelGrid.Children.Clear();

        int highest = ProgressService.Instance.GetHighestUnlocked(_gameId);
        int show = highest + 5;

        var levels = ProgressService.Instance.GetLevels(_gameId, show);
        foreach (var lp in levels)
            LevelGrid.Children.Add(CreateLevelTile(lp));
    }

    private static readonly Color[] _palette =
    {
        Color.FromArgb("#D4B896"),
        Color.FromArgb("#A8C4A0"),
        Color.FromArgb("#C4A4D4"),
        Color.FromArgb("#8ECCCC"),
        Color.FromArgb("#D4B896"),
    };

    private View CreateLevelTile(LevelProgress lp)
    {
        Color accent = lp.IsLocked ? Color.FromArgb("#99888888") : _gameAccentColor;

        var container = new StackLayout
        {
            Spacing = 4,
            Margin = new Thickness(6, 8, 6, 6),
            WidthRequest = 82,
            HorizontalOptions = LayoutOptions.Center
        };

        var starsRow = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 1
        };
        if (lp.IsCompleted)
        {
            for (int s = 1; s <= 3; s++)
            {
                starsRow.Children.Add(new Label
                {
                    Text = s <= lp.Stars ? "★" : "☆",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = s <= lp.Stars ? Color.FromArgb("#D4B896") : Colors.White,
                    Opacity = s <= lp.Stars ? 1.0 : 0.45,
                    Margin = new Thickness(0)
                });
            }
        }
        else
        {
            starsRow.Children.Add(new Label { Text = "   ", FontSize = 22 });
        }
        container.Children.Add(starsRow);

        bool isLocked = lp.IsLocked;
        bool isCompleted = lp.IsCompleted;

        Color faceColor = accent;

        View btnContent;
        if (isLocked)
        {
            btnContent = new Image
            {
                Source = "lock",
                HeightRequest = 34,
                WidthRequest = 34,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
        else
        {
            btnContent = new Label
            {
                Text = lp.LevelNumber.ToString(),
                FontFamily = "BrickSans",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        var card = new Grid
        {
            WidthRequest = 74,
            HeightRequest = 74,
            HorizontalOptions = LayoutOptions.Center
        };

        var bg = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            WidthRequest = 74,
            HeightRequest = 74,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        bg.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 };
        if (!isLocked)
        {
            bg.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#882D2A3D")),
                Offset = new Point(0, 4),
                Radius = 8,
                Opacity = 0.45f
            };
        }
        card.Children.Add(bg);

        var face = new Border
        {
            BackgroundColor = faceColor,
            StrokeThickness = 0,
            Opacity = isLocked ? 0.6 : 1.0,
            Margin = new Thickness(5),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        face.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 };
        card.Children.Add(face);

        btnContent.HorizontalOptions = LayoutOptions.Center;
        btnContent.VerticalOptions = LayoutOptions.Center;
        card.Children.Add(btnContent);

        if (!isLocked)
        {
            int level = lp.LevelNumber;
                var tap = new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        if (_isNavigating) return;
                        _isNavigating = true;
                        AudioService.Instance.Play("tap");
                        VibrationHelper.Click();
                        await card.ScaleTo(0.92, 80, Easing.CubicIn);
                        await card.ScaleTo(1.00, 80, Easing.CubicOut);
                        await Shell.Current.GoToAsync($"{_gameRoute}?level={level}");
                        _isNavigating = false;
                    })
                };
            card.GestureRecognizers.Add(tap);
        }

        container.Children.Add(card);
        return container;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildMap();
        AudioService.Instance.StartMusic();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }
}
