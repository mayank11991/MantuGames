using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class ProfilePickerPopup : ContentView
{
    public event EventHandler<PlayerProfile>? ProfileSelected;
    public event EventHandler? AddProfileRequested;

    // Avatar images A1-A7
    private static readonly string[] CharacterImages =
    {
        "a1.png", "a2.png", "a3.png", "a4.png", "a5.png", "a6.png", "a7.png"
    };

    private static readonly string[] PillColors =
    {
        "#7C5CFF", "#FF8FB1", "#4FD1B5", "#FFB86B", "#7FD8FF",
        "#A78BFA", "#FFD166"
    };

    public ProfilePickerPopup()
    {
        InitializeComponent();
    }

    public void Show()
    {
        ProfileService.EnsureLoaded();
        Rebuild();
        IsVisible = true;
        Overlay.IsVisible = true;
        Sheet.Opacity = 0;
        Sheet.Scale = 0.85;

        this.Dispatcher.Dispatch(() =>
        {
            if (!IsVisible) return;
            Sheet.Opacity = 1;
            Sheet.ScaleTo(1.0, 250, Easing.CubicOut);
        });
    }

    public void Hide()
    {
        Sheet.ScaleTo(0.85, 200, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void Rebuild()
    {
        ProfilesLayout.Children.Clear();

        int idx = 0;
        foreach (var p in ProfileService.Profiles)
        {
            ProfilesLayout.Children.Add(BuildProfileTile(p, idx));
            idx++;
        }

        ProfilesLayout.Children.Add(BuildAddTile());
    }

    private View BuildProfileTile(PlayerProfile p, int index)
    {
        string img = !string.IsNullOrEmpty(p.AvatarImage) ? p.AvatarImage : CharacterImages[index % CharacterImages.Length];
        string pillColor = PillColors[index % PillColors.Length];
        bool isActive = ProfileService.Active?.Id == p.Id;

        // Outer container with padding for overflow elements
        var outer = new Grid
        {
            WidthRequest = 110,
            HeightRequest = 140,
            Margin = new Thickness(6)
        };

        // Card (square) - centered vertically in outer grid
        var tile = new Border
        {
            WidthRequest = 100,
            HeightRequest = 100,
            BackgroundColor = Color.FromArgb("#FFFDFA"),
            StrokeThickness = isActive ? 3 : 2,
            Stroke = isActive ? new SolidColorBrush(Color.FromArgb("#7C5CFF")) : new SolidColorBrush(Color.FromArgb("#B3FFFFFF")),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        tile.Shadow = new Shadow { Brush = Color.FromArgb("#8A6FA8"), Offset = new Point(0, 6), Opacity = 0.26f, Radius = 10 };

        var inner = new Grid();

        // Avatar circle
        var avatarBorder = new Border
        {
            WidthRequest = 72,
            HeightRequest = 72,
            BackgroundColor = Color.FromArgb(pillColor),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 36 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var avatarImg = new Image
        {
            Source = img,
            Aspect = Aspect.AspectFit,
            WidthRequest = 56,
            HeightRequest = 56,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        avatarBorder.Content = avatarImg;
        inner.Children.Add(avatarBorder);
        tile.Content = inner;

        // Name pill - half on card bottom border, half outside
        // Card bottom is at ~120px in outer (70 center + 50 half-height)
        // Pill center should be at card bottom edge
        var pill = new Border
        {
            BackgroundColor = Color.FromArgb(pillColor),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(8, 2, 8, 2),
            HeightRequest = 24,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 58, 0, 0)  // 58 = half of card (50) + half pill (12) - small gap
        };
        pill.Content = new Label
        {
            Text = p.Name,
            FontFamily = "MadimiOne",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#3D2C4E"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            MaxLines = 1,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        // Green tick - half on card top border, half outside
        var check = new Border
        {
            WidthRequest = 24,
            HeightRequest = 24,
            BackgroundColor = Color.FromArgb("#3D2C4E"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, -58, -4, 0)  // -58 = opposite direction from pill
        };
        check.Content = new Label
        {
            Text = "✓",
            FontSize = 13,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Add children to outer grid
        outer.Children.Add(tile);
        outer.Children.Add(pill);
        if (isActive)
            outer.Children.Add(check);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OnProfileTapped(p);
        outer.GestureRecognizers.Add(tap);

        return outer;
    }

    private View BuildAddTile()
    {
        var outer = new Grid
        {
            WidthRequest = 110,
            HeightRequest = 140,
            Margin = new Thickness(6)
        };

        var tile = new Border
        {
            WidthRequest = 100,
            HeightRequest = 100,
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        tile.Shadow = new Shadow { Brush = Color.FromArgb("#8A6FA8"), Offset = new Point(0, 6), Opacity = 0.26f, Radius = 10 };

        var avatarBorder = new Border
        {
            WidthRequest = 72,
            HeightRequest = 72,
            BackgroundColor = Color.FromArgb("#F4E9FF"),
            StrokeThickness = 2,
            StrokeDashArray = new double[] { 5, 3 },
            Stroke = new SolidColorBrush(Color.FromArgb("#7C5CFF")),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 36 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        avatarBorder.Content = new Label
        {
            Text = "+",
            FontFamily = "MadimiOne",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#7C5CFF"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        tile.Content = avatarBorder;

        // "Add" label - half on card bottom border, half outside
        var pill = new Border
        {
            BackgroundColor = Color.FromArgb("#D8CAE8"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(8, 2, 8, 2),
            HeightRequest = 24,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 58, 0, 0)
        };
        pill.Content = new Label
        {
            Text = "Add",
            FontFamily = "MadimiOne",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#3D2C4E"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        outer.Children.Add(tile);
        outer.Children.Add(pill);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnAddTapped;
        outer.GestureRecognizers.Add(tap);

        return outer;
    }

    private void OnProfileTapped(PlayerProfile p)
    {
        AudioService.Instance.Play("tap");
        ProfileService.Activate(p);
        Hide();
        ProfileSelected?.Invoke(this, p);
    }

    private void OnAddTapped(object? s, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
        AddProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Consume tap — selection is mandatory
    }

    private void OnSheetTapped(object sender, TappedEventArgs e)
    {
        // Consume tap — selection is mandatory
    }
}
