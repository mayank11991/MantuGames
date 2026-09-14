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

        _isDragging = false;
        _lastCell = (-1, -1);
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

    private (int r, int c) CellFromTouchOverlay(PointF? overlayPos)
    {
        if (overlayPos == null || _vm == null) return (-1, -1);

        var drawable = GameCanvas.Drawable as CtdDrawable;
        if (drawable == null) return (-1, -1);

        var overlayBounds = TouchOverlay.Bounds;
        var canvasBounds = GameCanvas.Bounds;

        float scaleX = (float)(canvasBounds.Width / overlayBounds.Width);
        float scaleY = (float)(canvasBounds.Height / overlayBounds.Height);

        float canvasX = overlayPos.Value.X * scaleX;
        float canvasY = overlayPos.Value.Y * scaleY;

        return drawable.HitTest(canvasX, canvasY);
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;
        var pos = e.GetPosition(TouchOverlay);
        var cell = CellFromTouchOverlay(pos);
        Console.WriteLine($"[CTD] DOWN ({pos?.X:F1},{pos?.Y:F1}) cell=({cell.r},{cell.c})");
        if (cell.r >= 0)
        {
            _isDragging = true;
            _lastCell = cell;
            _vm.OnPointerDown(cell);
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isDragging || _vm == null || _vm.IsGameOver) return;
        var pos = e.GetPosition(TouchOverlay);
        var cell = CellFromTouchOverlay(pos);
        if (cell.r < 0 || cell == _lastCell) return;
        Console.WriteLine($"[CTD] MOVE cell=({cell.r},{cell.c})");
        _lastCell = cell;
        _vm.OnPointerDrag(cell);
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        if (_vm == null) return;
        var pos = e.GetPosition(TouchOverlay);
        var cell = CellFromTouchOverlay(pos);
        Console.WriteLine($"[CTD] UP cell=({cell.r},{cell.c}) dragging={_isDragging}");
        if (_isDragging && cell.r >= 0)
            _vm.OnPointerUp(cell);
        _isDragging = false;
        _lastCell = (-1, -1);
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        _lastCell = (-1, -1);
        _isDragging = false;
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
        if (win)
        {
            _startLevel++;
            _vm.StartLevel(_startLevel);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
