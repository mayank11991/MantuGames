namespace MantuGames.Views.Controls;

public partial class PauseOverlay : ContentView
{
    public event EventHandler? Resumed;

    public PauseOverlay()
    {
        InitializeComponent();
    }

    public void Show()
    {
        IsVisible = true;
        Opacity = 0;
        Overlay.IsVisible = true;
        this.FadeToAsync(1, 200);
    }

    public void Hide()
    {
        this.FadeToAsync(0, 150).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e) { }

    private void OnResume(object? sender, TappedEventArgs e)
    {
        Hide();
        Resumed?.Invoke(this, EventArgs.Empty);
    }
}
