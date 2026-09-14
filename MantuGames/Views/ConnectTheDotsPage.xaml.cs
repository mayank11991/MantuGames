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

        GameCanvas.HandlerChanged += OnCanvasHandlerChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        GameCanvas.HandlerChanged -= OnCanvasHandlerChanged;
        _vm?.Cleanup();
        _vm.BoardChanged -= OnBoardChanged;
        _vm.GameEnded -= OnGameEnded;
    }

    private void OnCanvasHandlerChanged(object sender, EventArgs e)
    {
        if (GameCanvas.Handler?.PlatformView == null) return;

#if ANDROID
        var platformView = GameCanvas.Handler.PlatformView as Android.Views.View;
        if (platformView != null)
        {
            Console.WriteLine($"[CTD] Handler attached: {platformView.GetType().Name}");
            platformView.SetOnTouchListener(new MantuGames.Platforms.Android.GraphicsTouchListener(platformView, OnNativeTouch));
        }
#endif
    }

    private void OnNativeTouch(float x, float y, bool down, bool move, bool up)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_vm == null) return;

            var drawable = GameCanvas.Drawable as CtdDrawable;
            if (drawable == null) return;

            var canvasBounds = GameCanvas.Bounds;
            float canvasX = (float)canvasBounds.X + x;
            float canvasY = (float)canvasBounds.Y + y;
            var cell = drawable.HitTest(canvasX, canvasY);

            if (down)
            {
                Console.WriteLine($"[CTD] DOWN native=({x:F1},{y:F1}) canvas=({canvasX:F1},{canvasY:F1}) cell=({cell.r},{cell.c})");
                if (_vm.IsGameOver || cell.r < 0) return;
                _isDragging = true;
                _lastCell = cell;
                _vm.OnPointerDown(cell);
            }
            else if (move && _isDragging)
            {
                if (_vm.IsGameOver || cell.r < 0 || cell == _lastCell) return;
                Console.WriteLine($"[CTD] MOVE native=({x:F1},{y:F1}) cell=({cell.r},{cell.c})");
                _lastCell = cell;
                _vm.OnPointerDrag(cell);
            }
            else if (up)
            {
                Console.WriteLine($"[CTD] UP native=({x:F1},{y:F1}) cell=({cell.r},{cell.c}) dragging={_isDragging}");
                if (_isDragging && cell.r >= 0)
                    _vm.OnPointerUp(cell);
                _isDragging = false;
                _lastCell = (-1, -1);
            }
        });
    }

    private void OnBoardChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => GameCanvas.Invalidate());
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
