using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class EditProfilePopup : ContentView
{
    private PlayerProfile? _profile;
    private string _selectedAvatar = "a1.png";

    private static readonly string[] AvatarImages =
    {
        "a1.png", "a2.png", "a3.png", "a4.png", "a5.png", "a6.png", "a7.png"
    };

    public event EventHandler? ProfileUpdated;

    public EditProfilePopup()
    {
        InitializeComponent();
        BuildAvatarPalette();
    }

    public void Show(PlayerProfile profile)
    {
        _profile = profile;
        _selectedAvatar = string.IsNullOrEmpty(profile.AvatarImage) ? "a1.png" : profile.AvatarImage;
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

    private void BuildAvatarPalette()
    {
        AvatarPalette.Children.Clear();
        foreach (var img in AvatarImages)
        {
            var border = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
                BackgroundColor = Colors.White,
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            var image = new Image
            {
                Source = img,
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            border.Content = image;

            var tap = new TapGestureRecognizer();
            var avatar = img;
            tap.Tapped += (s, e) =>
            {
                _selectedAvatar = avatar;
                UpdatePaletteSelection();
                UpdateAvatar();
            };
            border.GestureRecognizers.Add(tap);
            AvatarPalette.Children.Add(border);
        }
    }

    private void UpdatePaletteSelection()
    {
        foreach (var child in AvatarPalette.Children)
        {
            if (child is Border b)
            {
                // Check if this border contains the selected avatar image
                if (b.Content is Image img && img.Source is FileImageSource fis)
                {
                    var isSelected = fis.File == _selectedAvatar;
                    b.StrokeThickness = isSelected ? 3 : 0;
                    b.Stroke = isSelected
                        ? new SolidColorBrush(Color.FromArgb("#3D2C4E"))
                        : new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }

    private void UpdateAvatar()
    {
        AvatarImage.Source = _selectedAvatar;
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e) { }

    private void OnSave(object sender, TappedEventArgs e)
    {
        if (_profile == null) return;

        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player";

        ProfileService.RenameProfile(_profile, name);
        ProfileService.ChangeAvatar(_profile, _selectedAvatar);
        ProfileService.Activate(_profile);

        AudioService.Instance.Play("pop");
        Hide();
        ProfileUpdated?.Invoke(this, EventArgs.Empty);
    }
}
