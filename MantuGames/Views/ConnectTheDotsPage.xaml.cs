using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ConnectTheDotsPage : ContentPage
{
    private CtdViewModel _vm;
    private int _startLevel = 1;
    private bool _isDrawing;
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

        var drawable = new CtdDrawable(_vm);
        GameCanvas.Drawable = drawable;
        LevelLabel.Text = _vm.LevelDisplay;
        DifficultyLabel.Text = _vm.Difficulty;
        AudioService.Instance.StartMusic();

        // Setup touch handling on the overlay
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        TouchOverlay.GestureRecognizers.Add(pointerGesture);

        // Also handle tap for dot selection
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnOverlayTapped;
        TouchOverlay.GestureRecognizers.Add(tapGesture);
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

    private (int r, int c) HitTest(float x, float y)
    {
        var drawable = GameCanvas.Drawable as CtdDrawable;
        if (drawable == null) return (-1, -1);
        return drawable.HitTest(x, y);
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var position = e.GetPosition(TouchOverlay);
        if (position == null) return;

        float canvasX = (float)position.Value.X;
        float canvasY = (float)position.Value.Y;

        Console.WriteLine($"[CTD-UI] Tap at overlay ({canvasX:F1},{canvasY:F1})");

        var cell = HitTest(canvasX, canvasY);
        Console.WriteLine($"[CTD-UI] Hit test result: ({cell.r},{cell.c})");

        if (cell.r >= 0)
        {
            _vm.OnCellTapped(cell);
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var position = e.GetPosition(TouchOverlay);
        if (position == null) return;

        float canvasX = (float)position.Value.X;
        float canvasY = (float)position.Value.Y;

        var cell = HitTest(canvasX, canvasY);
        if (cell.r >= 0 && cell != _lastCell)
        {
            Console.WriteLine($"[CTD-UI] Drag to ({cell.r},{cell.c})");
            _lastCell = cell;
            _vm.OnCellTapped(cell);
        }
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        _lastCell = (-1, -1);
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
