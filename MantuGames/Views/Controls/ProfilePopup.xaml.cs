using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class ProfilePopup : ContentView
{
    private bool _submitted;
    private int _selectedColorIndex = 0;

    public event EventHandler<PlayerProfile>? ProfileCreated;

    private static readonly string[] AvatarPalette =
    {
        "#22D3EE", "#A855F7", "#FF9F1C", "#34D399",
        "#F43F5E", "#3B82F6", "#FACC15", "#EC4899",
    };

    public ProfilePopup()
    {
        InitializeComponent();
    }

    public void Show()
    {
        _submitted = false;
        _selectedColorIndex = 0;
        NameEntry.Text = "";
        NameEntry.PlaceholderColor = Color.FromArgb("#DCC8A8");
        BuildColorPicker();
        UpdateColorSelection();
        IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        Overlay.IsVisible = true;
        this.FadeTo(1, 300, Easing.CubicOut);
        this.ScaleTo(1, 300, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeTo(0, 200, Easing.CubicIn);
        this.ScaleTo(0.8, 200, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void BuildColorPicker()
    {
        ColorPickerGrid.Children.Clear();
        for (int i = 0; i < AvatarPalette.Length; i++)
        {
            int idx = i;
            var border = new Border
            {
                BackgroundColor = Color.FromArgb(AvatarPalette[i]),
                StrokeThickness = 0,
                WidthRequest = 60,
                HeightRequest = 50,
            };
            border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
            border.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => SelectColor(idx))
            });
            Grid.SetRow(border, i / 4);
            Grid.SetColumn(border, i % 4);
            ColorPickerGrid.Children.Add(border);
        }
    }

    private void SelectColor(int index)
    {
        _selectedColorIndex = index;
        UpdateColorSelection();
        AudioService.Instance.Play("tap");
    }

    private void UpdateColorSelection()
    {
        foreach (var child in ColorPickerGrid.Children)
        {
            int idx = ColorPickerGrid.Children.IndexOf(child);
            if (child is Border b)
            {
                b.StrokeThickness = idx == _selectedColorIndex ? 4 : 0;
                b.Stroke = idx == _selectedColorIndex ? new SolidColorBrush(Color.FromArgb("#FF9F1C")) : null;
            }
        }
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Consume tap to prevent passing through
    }

    private void OnSubmit(object sender, TappedEventArgs e)
    {
        if (_submitted) return;
        string name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NameEntry.PlaceholderColor = Color.FromArgb("#C0392B");
            return;
        }

        _submitted = true;

        var profile = ProfileService.AddProfile(name);
        // Override the auto-assigned color with user's selection
        profile.Color = AvatarPalette[_selectedColorIndex];
        ProfileService.Save();

        AudioService.Instance.Play("win");
        Hide();
        ProfileCreated?.Invoke(this, profile);
    }
}
