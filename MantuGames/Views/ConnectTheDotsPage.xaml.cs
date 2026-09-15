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
        _vm = new CtdViewModel(level);
        TimerView.TotalSeconds = ProgressService.GetTimerSeconds(level);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;

        GameCanvas.Drawable = new CtdDrawable(_vm);

        _isDragging = false;
        _lastCell = (-1, -1);

        AttachNativeTouch();
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
                if (_vm.IsGameOver || cell.r < 0) return;
                _isDragging = true;
                _lastCell = cell;
                _vm.OnPointerDown(cell);
            }
            else if (isMove)
            {
                if (!_isDragging || _vm.IsGameOver) return;
                if (cell.r < 0 || cell == _lastCell) return;
                _lastCell = cell;
                _vm.OnPointerDrag(cell);
            }
            else if (isUp)
            {
                if (_isDragging && cell.r >= 0)
                    _vm.OnPointerUp(cell);
                _isDragging = false;
                _lastCell = (-1, -1);
            }
        });
    }

    // ── Pause / Resume ──────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    // ── BACK ────────────────────────────────────────────────────
    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            _vm?.PauseTimer();
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave)
            {
                try { _vm?.ResumeTimer(); } catch { }
                return;
            }
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
            int total = ProgressService.GetTimerSeconds(_startLevel);
            int elapsed = total - _vm.TimeRemainingSec;
            int stars = isWin ? ProgressService.CalcStars(elapsed, total) : 0;
            int coins = isWin ? stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 } : 0;

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, total, stars, coins,
                isWin ? null : "Time's Up!", "connectthedots");
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
        _vm = new CtdViewModel(_startLevel);
        TimerView.TotalSeconds = ProgressService.GetTimerSeconds(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        GameCanvas.Drawable = new CtdDrawable(_vm);
        _isDragging = false;
        _lastCell = (-1, -1);
        AttachNativeTouch();
    }

    private void OnRetry(object sender, EventArgs e)
    {
        _vm.GameEnded -= OnGameEnded;
        _vm = new CtdViewModel(_startLevel);
        TimerView.TotalSeconds = ProgressService.GetTimerSeconds(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        GameCanvas.Drawable = new CtdDrawable(_vm);
        _isDragging = false;
        _lastCell = (-1, -1);
        AttachNativeTouch();
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
