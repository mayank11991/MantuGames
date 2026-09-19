using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class EditProfilePopup : ContentView
{
    private PlayerProfile? _profile;
    private string _selectedColor = "#22D3EE";

    private static readonly string[] Palette =
    {
        "#22D3EE", "#A855F7", "#FF9F1C", "#34D399",
        "#F43F5E", "#3B82F6", "#FACC15", "#EC4899",
    };

    public event EventHandler? ProfileUpdated;

    public EditProfilePopup()
    {
        InitializeComponent();
        BuildPalette();
    }

    public void Show(PlayerProfile profile)
    {
        _profile = profile;
        _selectedColor = profile.Color;
        NameEntry.Text = profile.Name;
        UpdateAvatar();
        UpdatePaletteSelection();

        IsVisible = true;
        Overlay.IsVisible = true;
        Sheet.IsVisible = true;
        Opacity = 1;
        Sheet.TranslationY = 0;
        Sheet.Opacity = 0;

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
                Sheet.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void BuildPalette()
    {
        ColorPalette.Children.Clear();
        foreach (var c in Palette)
        {
            var border = new Border
            {
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeThickness = 3,
                Stroke = new SolidColorBrush(Colors.Transparent),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                BackgroundColor = Color.FromArgb(c),
                Margin = new Thickness(2),
            };

            var tap = new TapGestureRecognizer();
            var color = c;
            tap.Tapped += (s, e) =>
            {
                _selectedColor = color;
                UpdatePaletteSelection();
                UpdateAvatar();
            };
            border.GestureRecognizers.Add(tap);
            ColorPalette.Children.Add(border);
        }
    }

    private void UpdatePaletteSelection()
    {
        foreach (var child in ColorPalette.Children)
        {
            if (child is Border b)
            {
                var bg = b.BackgroundColor?.ToHex() ?? "";
                b.Stroke = bg.Equals(_selectedColor, StringComparison.OrdinalIgnoreCase)
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Colors.Transparent);
            }
        }
    }

    private void UpdateAvatar()
    {
        AvatarPreview.BackgroundColor = Color.FromArgb(_selectedColor);
        var name = NameEntry.Text?.Trim();
        InitialLabel.Text = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e) { }

    private void OnSave(object sender, TappedEventArgs e)
    {
        if (_profile == null) return;

        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player";

        ProfileService.RenameProfile(_profile, name);
        ProfileService.ChangeColor(_profile, _selectedColor);
        ProfileService.Activate(_profile);

        AudioService.Instance.Play("pop");
        Hide();
        ProfileUpdated?.Invoke(this, EventArgs.Empty);
    }
}
