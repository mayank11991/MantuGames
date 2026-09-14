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

    private (int r, int c) HitTest(PointF? position)
    {
        if (position == null) return (-1, -1);
        var drawable = GameCanvas.Drawable as CtdDrawable;
        return drawable?.HitTest(position.Value.X, position.Value.Y) ?? (-1, -1);
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(GameCanvas);
        var cell = HitTest(pos);
        if (cell.r < 0) return;

        _isDragging = true;
        _lastCell = cell;
        _vm?.OnCellTapped(cell);
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isDragging || _vm == null || _vm.IsGameOver) return;

        var pos = e.GetPosition(GameCanvas);
        var cell = HitTest(pos);
        if (cell.r < 0) return;

        if (cell != _lastCell)
        {
            _lastCell = cell;
            _vm.OnCellTapped(cell);
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        _isDragging = false;
        _lastCell = (-1, -1);
    }

    private void OnCanvasTapped(object sender, TappedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;
        var pos = e.GetPosition(GameCanvas);
        var cell = HitTest(pos);
        if (cell.r >= 0)
            _vm.OnCellTapped(cell);
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
