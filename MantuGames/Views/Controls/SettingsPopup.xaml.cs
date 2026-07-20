using MantuGames.Helpers;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class SettingsPopup : ContentView
{
    public event EventHandler EditProfileRequested;
    public event EventHandler ProgressReset;

    public SettingsPopup()
    {
        InitializeComponent();
        MusicSwitch.IsToggled = AudioService.Instance.MusicEnabled;
        //VibrationSwitch.IsToggled = Preferences.Get("vibration_enabled", true);
        DarkModeSwitch.IsToggled = ThemeHelper.IsDarkMode;
        VersionLabel.Text = $"Version {AppInfo.VersionString} ({AppInfo.BuildString})";
    }

    public void Show()
    {
        IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        Overlay.IsVisible = true;
        this.FadeTo(1, 200, Easing.CubicOut);
        this.ScaleTo(1, 200, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeTo(0, 150, Easing.CubicIn);
        this.ScaleTo(0.8, 150, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnMusicToggled(object sender, ToggledEventArgs e)
    {
        AudioService.Instance.MusicEnabled = e.Value;
    }

    private void OnVibrationToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("vibration_enabled", e.Value);
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        ThemeHelper.IsDarkMode = e.Value;
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Consume tap to prevent passing through to views underneath
    }

    private void OnEditProfile(object sender, TappedEventArgs e)
    {
        Hide();
        EditProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnResetProgress(object sender, TappedEventArgs e)
    {
        try
        {
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Reset Progress", "This will erase all your progress for all games. Are you sure?", "Reset", "Cancel");
            if (!confirm) return;
          
            var games = ViewModels.DashboardViewModel.Games;
            foreach (var g in games)
            {
                ProgressService.Instance.ResetGame(g.Id);
            }

            ProgressReset?.Invoke(this, EventArgs.Empty);
            Hide();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnResetProgress: {ex.Message}");
        }
    }

    private void OnClose(object sender, TappedEventArgs e)
    {
        Hide();
    }
}