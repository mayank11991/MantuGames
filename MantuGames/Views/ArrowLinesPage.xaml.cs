using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ArrowLinesPage : ContentPage
{
    private ArrowLinesViewModel _vm = null!;
    private int _startLevel = 1;
    private readonly ArrowLinesDrawable _drawable = new();
    private bool _isAnimating;

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public ArrowLinesPage()
    {
        InitializeComponent();
        this.AddBannerAd();

        ArrowGraphics.Drawable = _drawable;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnGraphicsTapped;
        ArrowGraphics.GestureRecognizers.Add(tap);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        PauseOverlay.Resumed += OnResumeGame;
        InitGame(_startLevel);
        RefreshSolutionText();

        Opacity = 0;
        this.FadeToAsync(1, 400);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        _vm?.Cleanup();
    }

    private void InitGame(int level)
    {
        _vm?.Cleanup();
        _vm = new ArrowLinesViewModel(level);
        BindingContext = _vm;
        _vm.LineEscaped += OnLineEscaped;
        _vm.LineBlocked += OnLineBlocked;
        _vm.GameEnded += OnGameEnded;
        HeartsLabel.SetBinding(Label.TextProperty, new Binding(nameof(ArrowLinesViewModel.LivesDisplay)));

        _isAnimating = false;
        _drawable.Puzzle = _vm.Puzzle;
        _drawable.Moving = null;
        _drawable.Hinted = null;
        ArrowGraphics.Invalidate();
    }

    // ── Input ────────────────────────────────────────────────────────────────
    private void OnGraphicsTapped(object? sender, TappedEventArgs e)
    {
        if (_isAnimating || _vm.IsGameOver) return;
        var pt = e.GetPosition(ArrowGraphics);
        if (pt == null || _drawable.CellSize <= 0) return;

        float fx = ((float)pt.Value.X - _drawable.Origin.X) / _drawable.CellSize;
        float fy = ((float)pt.Value.Y - _drawable.Origin.Y) / _drawable.CellSize;
        int col = (int)Math.Floor(fx), row = (int)Math.Floor(fy);

        // Exact cell first, then the nearest neighbour (fingers are bigger than lines).
        var line = _vm.Puzzle.LineAt(row, col);
        if (line == null)
        {
            double best = 0.75;
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    var l = _vm.Puzzle.LineAt(row + dr, col + dc);
                    if (l == null) continue;
                    double d = Math.Sqrt(Math.Pow(fx - (col + dc + 0.5), 2) + Math.Pow(fy - (row + dr + 0.5), 2));
                    if (d < best) { best = d; line = l; }
                }
        }
        if (line == null) return;

        AudioService.Instance.Play("tap");
        _drawable.Hinted = null;
        _vm.Tap(line);
    }

    // ── Animations ───────────────────────────────────────────────────────────
    private async Task AnimateAsync(float from, float to, uint ms, Easing easing)
    {
        var tcs = new TaskCompletionSource();
        new Animation(v =>
        {
            _drawable.Offset = (float)v;
            ArrowGraphics.Invalidate();
        }, from, to, easing).Commit(this, "arrowline", 16, ms, finished: (_, _) => tcs.TrySetResult());
        await tcs.Task;
    }

    private async void OnLineEscaped(ArrowLine line)
    {
        _isAnimating = true;
        try
        {
            AudioService.Instance.Play("slide");
            _drawable.Moving = line;
            _drawable.MovingIsBump = false;
            int distance = _vm.Puzzle.Rows + line.Cells.Count + 1;
            await AnimateAsync(0, distance, (uint)Math.Min(900, 260 + distance * 45), Easing.CubicIn);
        }
        finally
        {
            _drawable.Moving = null;
            _drawable.Offset = 0;
            _isAnimating = false;
            ArrowGraphics.Invalidate();
        }
    }

    private async void OnLineBlocked(ArrowLine line, int run)
    {
        _isAnimating = true;
        try
        {
            _drawable.Moving = line;
            _drawable.MovingIsBump = false;
            float hit = run + 0.3f;
            await AnimateAsync(0, hit, (uint)(140 + run * 50), Easing.CubicIn);
            _drawable.MovingIsBump = true;
            AudioService.Instance.Play("wrong");
            VibrationHelper.Click();
            _ = ArrowGraphics.TranslateToAsync(-6, 0, 40).ContinueWith(_ =>
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ArrowGraphics.TranslateToAsync(6, 0, 40);
                    await ArrowGraphics.TranslateToAsync(0, 0, 40);
                }));
            await AnimateAsync(hit, 0, (uint)(180 + run * 50), Easing.CubicOut);
        }
        finally
        {
            _drawable.Moving = null;
            _drawable.Offset = 0;
            _drawable.MovingIsBump = false;
            _isAnimating = false;
            ArrowGraphics.Invalidate();
        }
    }

    // ── Game end ─────────────────────────────────────────────────────────────
    private async void OnGameEnded(bool isWin)
    {
        // Let the last slide finish before the popup covers the board.
        while (_isAnimating) await Task.Delay(50);

        if (isWin)
        {
            int stars = _vm.Stars;
            AudioService.Instance.Play("win");
            VibrationHelper.Click();
            await ResultPopup.Show(true, _vm.CurrentLevel, 0, 210, stars, ProgressService.CoinReward(stars), gameId: "arrowlines");
        }
        else
        {
            AudioService.Instance.Play("lose");
            await ResultPopup.Show(false, _vm.CurrentLevel, 0, 210, reason: "Out of lives!", gameId: "arrowlines");
        }
    }

    private void OnNextLevel(object? sender, EventArgs e) => InitGame(_vm.CurrentLevel + 1);

    private void OnRetry(object? sender, EventArgs e) => InitGame(_vm.CurrentLevel);

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        bool leave = ResultPopup.IsVisible || await ConfirmPopup.Show("Leave Game?", "Your progress will be lost.", "Leave", "Stay");
        if (!leave) return;
        _vm?.Cleanup();
        await Shell.Current.GoToAsync("..");
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }

    private void OnPause(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        PauseOverlay.Show();
    }

    private void OnResumeGame(object? sender, EventArgs e) => AudioService.Instance.Play("tap");

    // ── Show Solution: clears the board in a valid order ────────────────────
    private async void OnShowSolution(object? sender, EventArgs e)
    {
        if (_vm == null || _vm.IsGameOver || _isAnimating) return;

        if (CoinService.Balance < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("arrowlines");
            return;
        }
        CoinService.SpendForSolution("arrowlines");
        RefreshSolutionText();

        _vm.IsShowSolutionActive = true;
        SolutionCoinLabel.IsEnabled = false;
        try
        {
            while (!_vm.IsGameOver)
            {
                var next = _vm.NextFreeLine();
                if (next == null) break;
                _drawable.Hinted = next;
                ArrowGraphics.Invalidate();
                await Task.Delay(220);
                _drawable.Hinted = null;
                _vm.Tap(next);
                while (_isAnimating) await Task.Delay(40);
            }
        }
        finally
        {
            _vm.IsShowSolutionActive = false;
            SolutionCoinLabel.IsEnabled = true;
            RefreshSolutionText();
        }
    }

    private void RefreshSolutionText()
    {
        SolutionCoinLabel.Text = $" Show Solution \n* Costs {CoinService.SolutionCost} coins — you have {CoinService.Balance}";
    }
}
