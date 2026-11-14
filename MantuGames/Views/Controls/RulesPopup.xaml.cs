namespace MantuGames.Views.Controls;

public partial class RulesPopup : ContentView
{
    public RulesPopup()
    {
        InitializeComponent();
    }

    public void Show(string body)
    {
        BodyLabel.Text = body;
        IsVisible = true;
        Overlay.IsVisible = true;
        Opacity = 0;
        Scale = 0.85;
        this.FadeToAsync(1, 200, Easing.CubicOut);
        this.ScaleToAsync(1, 200, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeToAsync(0, 150, Easing.CubicIn);
        this.ScaleToAsync(0.85, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnDismiss(object? sender, TappedEventArgs e) => Hide();
}
