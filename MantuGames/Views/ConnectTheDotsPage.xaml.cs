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
        AudioService.Instance.StartMusic();
        InitGame(_startLevel);
        PauseOverlay.Resumed += OnResumeGame;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        Cleanup();
    }

    private void InitGame(int level)
    {
        _vm = new CtdViewModel(level);
        _vm.BoardChanged += OnBoardChanged;
        _vm.GameEnded += OnGameEnded;

        GameCanvas.Drawable = new CtdDrawable(_vm);
        LevelBadge.Text = _vm.LevelDisplay;
        DifficultyLabel.Text = _vm.Difficulty;
        CtdTimer.TotalSeconds = ProgressService.GetTimerSeconds(level);

        _isDragging = false;
        _lastCell = (-1, -1);

        AttachNativeTouch();
    }

    private void Cleanup()
    {
        GameCanvas.HandlerChanged -= OnCanvasHandlerChanged;
        _vm?.Cleanup();
        if (_vm != null)
        {
            _vm.BoardChanged -= OnBoardChanged;
            _vm.GameEnded -= OnGameEnded;
        }
    }

    private void AttachNativeTouch()
    {
#if ANDROID
        if (GameCanvas.Handler?.PlatformView is Android.Views.View nativeView)
        {
            nativeView.SetOnTouchListener(new Platforms.Android.CtdNativeTouchListener(nativeView, OnNativeTouch));
        }
        else
        {
            GameCanvas.HandlerChanged += OnCanvasHandlerChanged;
        }
#endif
    }

    private void OnCanvasHandlerChanged(object sender, EventArgs e)
    {
#if ANDROID
        if (GameCanvas.Handler?.PlatformView is Android.Views.View nv)
        {
            nv.SetOnTouchListener(new Platforms.Android.CtdNativeTouchListener(nv, OnNativeTouch));
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

    private async void OnGameEnded(bool isWin)
    {
        _vm?.StopTimer();
        int total = ProgressService.GetTimerSeconds(_startLevel);
        int elapsed = total - _vm.TimeRemainingSec;

        int stars = isWin ? ProgressService.CalcStars(elapsed, total) : 0;
        int coins = isWin ? stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 } : 0;

        await ResultPopup.Show(isWin, _startLevel, elapsed, total, stars, coins,
            isWin ? null : "Time's Up!", "connectthedots");
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel++;
        Cleanup();
        InitGame(_startLevel);
    }

    private void OnRetry(object sender, EventArgs e)
    {
        Cleanup();
        InitGame(_startLevel);
    }

    private void OnPause(object sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        _vm?.PauseTimer();
        bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
        if (leave)
            await Shell.Current.GoToAsync("..");
        else
            try { _vm?.ResumeTimer(); } catch { }
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
