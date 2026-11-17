using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

/// <summary>
/// Edit the player's name. Their picture is the current skin's character,
/// so "Change skin" hands over to the skin shop.
/// </summary>
public partial class EditProfilePopup : ContentView
{
    private PlayerProfile? _profile;

    public event EventHandler? ProfileUpdated;
    public event EventHandler? SkinShopRequested;

    public EditProfilePopup()
    {
        InitializeComponent();
        ThemeStyler.RegisterDialog(Sheet);
    }

    public void Show(PlayerProfile profile)
    {
        _profile = profile;
        NameEntry.Text = profile.Name;
        var skin = SkinService.Current;
        AvatarImage.Source = skin.Mascot;
        AvatarPreview.BackgroundColor = Color.FromArgb(skin.ShellMid);

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
            Sheet.TranslateToAsync(0, 0, 320, Easing.CubicOut);
        });
    }

    public void Hide()
    {
        Sheet.TranslateToAsync(0, 500, 240, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                Sheet.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e) { }

    private void OnChangeSkin(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        Hide();
        SkinShopRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSave(object? sender, TappedEventArgs e)
    {
        if (_profile == null) return;

        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player";
        ProfileService.RenameProfile(_profile, name);

        AudioService.Instance.Play("pop");
        Hide();
        ProfileUpdated?.Invoke(this, EventArgs.Empty);
    }
}
