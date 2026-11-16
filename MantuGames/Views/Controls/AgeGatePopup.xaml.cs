using MantuGames.Services;

namespace MantuGames.Views.Controls;

/// <summary>
/// One-time neutral age question shown before ads start. Every year is listed
/// and nothing is preselected, so no answer is nudged.
/// </summary>
public partial class AgeGatePopup : ContentView
{
    private TaskCompletionSource? _tcs;

    public AgeGatePopup()
    {
        InitializeComponent();
        int thisYear = DateTime.Now.Year;
        YearPicker.ItemsSource = Enumerable.Range(0, 101).Select(i => (thisYear - i).ToString()).ToList();
    }

    /// <summary>Shows the question and completes once a year is saved.</summary>
    public Task ShowAsync()
    {
        _tcs = new TaskCompletionSource();
        YearPicker.SelectedIndex = -1;
        ContinueBorder.Opacity = 0.45;
        IsVisible = true;
        Opacity = 0;
        _ = this.FadeToAsync(1, 200, Easing.CubicOut);
        return _tcs.Task;
    }

    private void OnYearChanged(object? sender, EventArgs e) =>
        ContinueBorder.Opacity = YearPicker.SelectedIndex >= 0 ? 1 : 0.45;

    private async void OnContinue(object? sender, TappedEventArgs e)
    {
        try
        {
            if (YearPicker.SelectedItem is not string text || !int.TryParse(text, out int year)) return;
            AudioService.Instance.Play("tap");
            AgeService.SetBirthYear(year);
            await this.FadeToAsync(0, 150, Easing.CubicIn);
            IsVisible = false;
            _tcs?.TrySetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving age: {ex.Message}");
        }
    }
}
