using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class ProfilePopup : ContentView
{
    private bool _submitted;
    private int _selectedAvatarIndex = 0;

    public event EventHandler<PlayerProfile>? ProfileCreated;

    private static readonly string[] AvatarImages =
    {
        "a1.png", "a2.png", "a3.png", "a4.png", "a5.png", "a6.png", "a7.png"
    };

    public ProfilePopup()
    {
        InitializeComponent();
    }

    public void Show()
    {
        _submitted = false;
        _selectedAvatarIndex = 0;
        NameEntry.Text = "";
        NameEntry.PlaceholderColor = Color.FromArgb("#DCC8A8");
        BuildAvatarPicker();
        UpdateAvatarSelection();
        IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        Overlay.IsVisible = true;
        this.FadeToAsync(1, 300, Easing.CubicOut);
        this.ScaleToAsync(1, 300, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeToAsync(0, 200, Easing.CubicIn);
        this.ScaleToAsync(0.8, 200, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void BuildAvatarPicker()
    {
        AvatarPickerGrid.Children.Clear();
        for (int i = 0; i < AvatarImages.Length; i++)
        {
            int idx = i;
            var border = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                WidthRequest = 60,
                HeightRequest = 60,
                Padding = new Thickness(4),
            };

            var img = new Image
            {
                Source = AvatarImages[i],
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            border.Content = img;

            border.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => SelectAvatar(idx))
            });
            Grid.SetRow(border, i / 4);
            Grid.SetColumn(border, i % 4);
            AvatarPickerGrid.Children.Add(border);
        }
    }

    private void SelectAvatar(int index)
    {
        _selectedAvatarIndex = index;
        UpdateAvatarSelection();
        AudioService.Instance.Play("tap");
    }

    private void UpdateAvatarSelection()
    {
        foreach (var child in AvatarPickerGrid.Children)
        {
            int idx = AvatarPickerGrid.Children.IndexOf(child);
            if (child is Border b)
            {
                b.StrokeThickness = idx == _selectedAvatarIndex ? 3 : 0;
                b.Stroke = idx == _selectedAvatarIndex
                    ? new SolidColorBrush(Color.FromArgb("#3D2C4E"))
                    : null;
            }
        }
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Consume tap to prevent passing through
    }

    private void OnCancel(object? sender, TappedEventArgs e)
    {
        Hide();
    }

    private void OnSubmit(object? sender, TappedEventArgs e)
    {
        if (_submitted) return;
        string? name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NameEntry.PlaceholderColor = Color.FromArgb("#C0392B");
            return;
        }

        _submitted = true;

        var profile = ProfileService.AddProfile(name);
        profile.AvatarImage = AvatarImages[_selectedAvatarIndex];
        ProfileService.Save();

        AudioService.Instance.Play("win");
        Hide();
        ProfileCreated?.Invoke(this, profile);
    }
}
