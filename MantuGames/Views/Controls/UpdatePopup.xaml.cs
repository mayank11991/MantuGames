using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class UpdatePopup : ContentView
{
    private bool _force;
    private string _url = AppConfig.PlayStoreUrl;

    public UpdatePopup()
    {
        InitializeComponent();
        ThemeStyler.RegisterDialog(DialogCard);
    }

    public void Show(UpdateInfo info)
    {
        _force = info.MinVersionCode > UpdateService.CurrentVersionCode;
        _url = info.Url;

        TitleLabel.Text = _force ? "Update Required" : "Update Available";
        VersionLabel.Text = $"New version {info.VersionName} is here!";
        NotesLabel.Text = string.IsNullOrEmpty(info.Notes) ? "Bug fixes and improvements." : info.Notes;

        // Force updates can't be dismissed — the app won't work otherwise.
        LaterLabel.IsVisible = !_force;

        IsVisible = true;
        Overlay.IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        this.FadeToAsync(1, 200, Easing.CubicOut);
        this.ScaleToAsync(1, 200, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeToAsync(0, 150, Easing.CubicIn);
        this.ScaleToAsync(0.8, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private async void OnUpdate(object? sender, TappedEventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            await Launcher.OpenAsync(_url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening update link: {ex.Message}");
        }
    }

    private void OnLater(object? sender, TappedEventArgs e) => Hide();
}