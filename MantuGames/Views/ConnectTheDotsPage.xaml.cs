using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ConnectTheDotsPage : ContentPage
{
    private CtdViewModel _vm;
    private int _startLevel = 1;

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

        var drawable = new CtdDrawable(_vm);
        GameCanvas.Drawable = drawable;
        LevelLabel.Text = _vm.LevelDisplay;
        DifficultyLabel.Text = _vm.Difficulty;
        AudioService.Instance.StartMusic();

        // Use pointer gesture for drag drawing
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerExited += OnPointerExited;
        GameCanvas.GestureRecognizers.Add(pointerGesture);
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

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        // Touch started
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var position = e.GetPosition(GameCanvas);
        if (position == null) return;

        var drawable = GameCanvas.Drawable as CtdDrawable;
        if (drawable == null) return;

        var cell = drawable.HitTest((float)position.Value.X, (float)position.Value.Y);
        if (cell.r >= 0)
        {
            _vm.OnCellTapped(cell);
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        // Touch ended - complete path if on matching dot
    }

    private void OnCanvasTapped(object sender, TappedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var position = e.GetPosition(GameCanvas);
        if (position == null) return;

        var drawable = GameCanvas.Drawable as CtdDrawable;
        if (drawable == null) return;

        var cell = drawable.HitTest((float)position.Value.X, (float)position.Value.Y);
        if (cell.r >= 0)
        {
            _vm.OnCellTapped(cell);
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
