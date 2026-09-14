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

        AttachNativeTouch();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm?.Cleanup();
        _vm.BoardChanged -= OnBoardChanged;
        _vm.GameEnded -= OnGameEnded;
    }

    private void AttachNativeTouch()
    {
#if ANDROID
        if (GameCanvas.Handler?.PlatformView is Android.Views.View nativeView)
        {
            Console.WriteLine($"[CTD] Attaching native touch to {nativeView.GetType().Name}");
            nativeView.SetOnTouchListener(new Platforms.Android.CtdNativeTouchListener(nativeView, OnNativeTouch));
        }
        else
        {
            Console.WriteLine($"[CTD] Handler={GameCanvas.Handler} PlatformView={GameCanvas.Handler?.PlatformView?.GetType().Name}");
            GameCanvas.HandlerChanged += (s, e) =>
            {
                if (GameCanvas.Handler?.PlatformView is Android.Views.View nv)
                {
                    Console.WriteLine($"[CTD] Late attach native touch to {nv.GetType().Name}");
                    nv.SetOnTouchListener(new Platforms.Android.CtdNativeTouchListener(nv, OnNativeTouch));
                }
            };
        }
#endif
    }

    private void OnNativeTouch(float x, float y, bool isDown, bool isMove, bool isUp)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_vm == null) return;

            var drawable = GameCanvas.Drawable as CtdDrawable;
            if (drawable == null) return;

            var cell = drawable.HitTest(x, y);

            if (isDown)
            {
                Console.WriteLine($"[CTD] DOWN native=({x:F1},{y:F1}) cell=({cell.r},{cell.c})");
                if (_vm.IsGameOver || cell.r < 0) return;
                _isDragging = true;
                _lastCell = cell;
                _vm.OnPointerDown(cell);
            }
            else if (isMove)
            {
                if (!_isDragging || _vm.IsGameOver) return;
                if (cell.r < 0 || cell == _lastCell) return;
                Console.WriteLine($"[CTD] MOVE native=({x:F1},{y:F1}) cell=({cell.r},{cell.c})");
                _lastCell = cell;
                _vm.OnPointerDrag(cell);
            }
            else if (isUp)
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
