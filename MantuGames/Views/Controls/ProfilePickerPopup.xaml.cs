using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

/// <summary>
/// Netflix-style profile picker. Shown from the bottom of the screen;
/// the user must select an existing profile or create a new one —
/// there is no skip.
/// </summary>
public partial class ProfilePickerPopup : ContentView
{
    public event EventHandler<PlayerProfile>? ProfileSelected;
    public event EventHandler? AddProfileRequested;

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
        Opacity = 1;
        Sheet.TranslationY = 0;
        Sheet.Opacity = 0;

        // Wait for layout so the sheet has a real height, then slide it up.
        this.Dispatcher.Dispatch(() =>
        {
            if (!IsVisible) return;
            Sheet.TranslationY = Math.Max(Sheet.Height, 400);
            Sheet.Opacity = 1;
            Sheet.TranslateTo(0, 0, 320, Easing.CubicOut);
        });
    }

    public void Hide()
    {
        Sheet.TranslateTo(0, 500, 240, Easing.CubicIn).ContinueWith(_ =>
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

        foreach (var p in ProfileService.Profiles)
        {
            ProfilesLayout.Children.Add(BuildProfileTile(p));
        }

        ProfilesLayout.Children.Add(BuildAddTile());
    }

    private View BuildProfileTile(PlayerProfile p)
    {
        var tile = new VerticalStackLayout
        {
            Spacing = 8,
            Margin = new Thickness(14, 6),
            HorizontalOptions = LayoutOptions.Center,
        };

        var avatar = new Border
        {
            WidthRequest = 72,
            HeightRequest = 72,
            BackgroundColor = Color.FromArgb(p.Color),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 36 },
        };
        avatar.Content = new Label
        {
            Text = p.Initial,
            FontFamily = "Orbitron",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0B0E14"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        var name = new Label
        {
            Text = p.Name,
            FontFamily = "Inter",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#E7ECF5"),
            HorizontalOptions = LayoutOptions.Center,
            MaxLines = 1,
            LineBreakMode = LineBreakMode.TailTruncation,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OnProfileTapped(p);
        tile.GestureRecognizers.Add(tap);

        tile.Children.Add(avatar);
        tile.Children.Add(name);
        return tile;
    }

    private View BuildAddTile()
    {
        var tile = new VerticalStackLayout
        {
            Spacing = 8,
            Margin = new Thickness(14, 6),
            HorizontalOptions = LayoutOptions.Center,
        };

        var avatar = new Border
        {
            WidthRequest = 72,
            HeightRequest = 72,
            BackgroundColor = Color.FromArgb("#0F1420"),
            StrokeThickness = 2,
            StrokeDashArray = new double[] { 6, 4 },
            Stroke = new SolidColorBrush(Color.FromArgb("#3A4A63")),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 36 },
        };
        avatar.Content = new Label
        {
            Text = "+",
            FontFamily = "Inter",
            FontSize = 36,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#9AA7BD"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        var name = new Label
        {
            Text = "Add profile",
            FontFamily = "Inter",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#9AA7BD"),
            HorizontalOptions = LayoutOptions.Center,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnAddTapped;
        tile.GestureRecognizers.Add(tap);

        tile.Children.Add(avatar);
        tile.Children.Add(name);
        return tile;
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
