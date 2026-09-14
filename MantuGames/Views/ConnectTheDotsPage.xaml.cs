using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ConnectTheDotsPage : ContentPage
{
    private CtdViewModel _vm;
    private int _startLevel = 1;
    private bool _isDragging;
    private (int r, int c) _lastCell = (-1, -1);

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public ConnectTheDotsPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm = new CtdViewModel(_startLevel);
        _vm.BoardChanged += OnBoardChanged;
        _vm.GameEnded += OnGameEnded;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CtdViewModel.LevelDisplay))
                LevelLabel.Text = _vm.LevelDisplay;
            if (e.PropertyName == nameof(CtdViewModel.Difficulty))
                DifficultyLabel.Text = _vm.Difficulty;
        };

        GameCanvas.Drawable = new CtdDrawable(_vm);
        LevelLabel.Text = _vm.LevelDisplay;
        DifficultyLabel.Text = _vm.Difficulty;
        AudioService.Instance.StartMusic();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm?.Cleanup();
        _vm.BoardChanged -= OnBoardChanged;
        _vm.GameEnded -= OnGameEnded;
    }

    private void OnBoardChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => GameCanvas.Invalidate());
    }

    private void OnCanvasTapped(object sender, TappedEventArgs e)
    {
        var pos = e.GetPosition(GameCanvas);
        if (pos == null || _vm == null) return;

        var drawable = GameCanvas.Drawable as CtdDrawable;
        var cell = drawable.HitTest((float)pos.Value.X, (float)pos.Value.Y);
        if (cell.r >= 0)
            _vm.CellTappedCommand.Execute(cell);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;
        if (sender is not GraphicsView) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDragging = true;
                break;

            case GestureStatus.Running:
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isDragging = false;
                _lastCell = (-1, -1);
                break;
        }
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        _vm?.Restart();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnGameEnded(bool win)
    {
        AudioService.Instance.Play(win ? "win" : "lose");
        if (win)
        {
            int coins = _vm.Difficulty switch
            {
                "EASY" => 5,
                "MEDIUM" => 10,
                "HARD" => 15,
                _ => 20
            };
            CoinService.AddCoins("connectthedots", coins);
            ProgressService.Instance.CompleteLevel("connectthedots", _startLevel, 3);
        }
        await DisplayAlert(win ? "You Win!" : "Game Over",
            win ? $"Level {_startLevel} completed!" : "Try again!",
            "OK");
        if (win) _vm.StartLevel(_startLevel + 1);
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
