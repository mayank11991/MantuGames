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

        var pointer = new PointerGestureRecognizer();
        pointer.PointerPressed += OnPointerPressed;
        pointer.PointerMoved += OnPointerMoved;
        pointer.PointerReleased += OnPointerReleased;
        pointer.PointerExited += OnPointerReleased;
        TouchSurface.GestureRecognizers.Add(pointer);
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

    private (int r, int c) CellFromPosition(PointF? pos)
    {
        if (pos == null || _vm == null) return (-1, -1);
        var drawable = GameCanvas.Drawable as CtdDrawable;
        return drawable?.HitTest(pos.Value.X, pos.Value.Y) ?? (-1, -1);
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var pos = e.GetPosition(TouchSurface);
        var cell = CellFromPosition(pos);
        Console.WriteLine($"[CTD] PointerPressed cell=({cell.r},{cell.c})");

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

        var pos = e.GetPosition(TouchSurface);
        var cell = CellFromPosition(pos);
        if (cell.r < 0 || cell == _lastCell) return;

        Console.WriteLine($"[CTD] PointerMoved cell=({cell.r},{cell.c})");
        _lastCell = cell;
        _vm.OnPointerDrag(cell);
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        if (_vm == null) return;

        var pos = e.GetPosition(TouchSurface);
        var cell = CellFromPosition(pos);

        Console.WriteLine($"[CTD] PointerReleased cell=({cell.r},{cell.c}) isDragging={_isDragging}");

        if (_isDragging && cell.r >= 0)
        {
            _vm.OnPointerUp(cell);
        }

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
