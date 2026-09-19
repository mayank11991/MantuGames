using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class NumberMatchPage : ContentPage
{
    private NumberMatchViewModel _vm;
    private int _startLevel = 1;

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public NumberMatchPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        InitGame(_startLevel);
        PauseOverlay.Resumed += OnResumeGame;

        this.Opacity = 0;
        this.FadeTo(1, 400);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        if (_vm != null)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm.Cleanup();
        }
    }

    private void InitGame(int level)
    {
        _vm = new NumberMatchViewModel(level);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
    }

    // ── Pause / Resume ──────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
    }

    // ── BACK ────────────────────────────────────────────────────
    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            _vm?.Cleanup();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }

    // ── GAME EVENTS ─────────────────────────────────────────────
    private async void OnGameEnded(bool isWin)
    {
        try
        {
            int stars = isWin ? 3 : 0;
            int coins = isWin ? 5 : 0;

            await ResultPopup.Show(isWin, _vm.CurrentLevel, 0, 1, stars, coins,
                isWin ? null : "No moves left!", "numbermatch");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel = _vm.CurrentLevel + 1;
        _vm.GameEnded -= OnGameEnded;
        InitGame(_startLevel);
    }

    private void OnRetry(object sender, EventArgs e)
    {
        _vm.GameEnded -= OnGameEnded;
        InitGame(_startLevel);
    }

    // ── ADD NUMBERS ─────────────────────────────────────────────
    private void OnAddNumbers(object sender, EventArgs e)
    {
        _vm?.AddNumbers();
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
