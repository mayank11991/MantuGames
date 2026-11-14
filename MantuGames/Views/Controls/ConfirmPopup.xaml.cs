namespace MantuGames.Views.Controls;

public partial class ConfirmPopup : ContentView
{
    private TaskCompletionSource<bool>? _tcs;

    public ConfirmPopup()
    {
        InitializeComponent();
    }

    public async Task<bool> Show(string title, string message, string confirm = "Leave", string cancel = "Stay")
    {
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ConfirmLabel.Text = confirm;
        CancelLabel.Text = cancel;

        _tcs = new TaskCompletionSource<bool>();

        IsVisible = true;
        Overlay.IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        _ = this.FadeToAsync(1, 200, Easing.CubicOut);
        _ = this.ScaleToAsync(1, 200, Easing.SpringOut);

        bool result = await _tcs.Task;

        _ = this.FadeToAsync(0, 150, Easing.CubicIn);
        _ = this.ScaleToAsync(0.8, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });

        return result;
    }

    public void Hide()
    {
        _tcs?.TrySetResult(false);
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e) { }

    private void OnConfirm(object? sender, TappedEventArgs e)
    {
        _tcs?.TrySetResult(true);
    }

    private void OnCancel(object? sender, TappedEventArgs e)
    {
        _tcs?.TrySetResult(false);
    }
}
