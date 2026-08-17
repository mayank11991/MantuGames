using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class BlockPuzzlePage : ContentPage
{
    private BlockPuzzleViewModel _vm;
    private int _startLevel = 1;
    private int _prevLinesCleared;
    private BoxView[,] _cells;

    // Gesture tracking
    private double _panX, _panY;
    private bool   _panFired;
    private const double SwipeMin = 30; // dp to register a swipe

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    private double _lastBoardW = -1, _lastBoardH = -1;

    public BlockPuzzlePage()
    {
        InitializeComponent();
        this.AddBannerAd();
        BoardContainer.SizeChanged += OnBoardContainerSizeChanged;
    }

    private void OnBoardContainerSizeChanged(object sender, EventArgs e)
    {
        if (_vm == null) return;
        if (BoardContainer.Width <= 0 || BoardContainer.Height <= 0) return;
        if (Math.Abs(BoardContainer.Width - _lastBoardW) < 1 && Math.Abs(BoardContainer.Height - _lastBoardH) < 1) return;

        _lastBoardW = BoardContainer.Width;
        _lastBoardH = BoardContainer.Height;
        BuildBoard();
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
        Cleanup();
        _vm = new BlockPuzzleViewModel(level);
        BindingContext = _vm;
        _vm.BoardChanged += OnBoardChanged;
        _vm.PieceLocked  += OnPieceLocked;
        _vm.GameEnded    += OnGameEnded;
        _vm.LinesClearing += OnLinesClearing;
        BlockTimer.TotalSeconds = BlockPuzzleViewModel.LevelTimerSeconds;
        BuildBoard();
        BuildNextPreview();
    }

    private void Cleanup()
    {
        if (_vm == null) return;
        _vm.BoardChanged -= OnBoardChanged;
        _vm.PieceLocked  -= OnPieceLocked;
        _vm.GameEnded    -= OnGameEnded;
        _vm.LinesClearing -= OnLinesClearing;
        _vm.Cleanup();
        _vm = null;
    }

    // ── Build board ───────────────────────────────────────────────────────────
    private void BuildBoard()
    {
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();

        _cells = new BoxView[BlockPuzzleViewModel.Rows, BlockPuzzleViewModel.Cols];

        // Size from BoardContainer's own measured bounds (it fills whatever
        // space is left in the "*" row after the top bar/score row/next-piece
        // panel), not a guessed device-height constant. This is what keeps the
        // board from ever overlapping the panel below it.
        double availW = BoardContainer.Width  - 12; // minus stroke + a little breathing room
        double availH = BoardContainer.Height - 12;
        double cell;
        if (availW > 0 && availH > 0)
        {
            cell = Math.Floor(Math.Min(availW / BlockPuzzleViewModel.Cols,
                                        availH / BlockPuzzleViewModel.Rows));
        }
        else
        {
            // First layout pass hasn't measured the container yet; fall back to
            // a width-only estimate. OnBoardContainerSizeChanged rebuilds with
            // the real size as soon as it's known.
            double sw = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            cell = Math.Floor((sw - 24) / BlockPuzzleViewModel.Cols);
        }
        cell = Math.Max(cell, 12);

        for (int r = 0; r < BlockPuzzleViewModel.Rows; r++)
            BoardGrid.RowDefinitions.Add(new RowDefinition { Height = cell });
        for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cell });

        for (int r = 0; r < BlockPuzzleViewModel.Rows; r++)
            for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
            {
                var bv = new BoxView
                {
                    Color        = Color.FromArgb("#0D1B2A"),
                    CornerRadius = 2,
                    Margin       = new Thickness(0.5)
                };
                Grid.SetRow(bv, r);
                Grid.SetColumn(bv, c);
                BoardGrid.Children.Add(bv);
                _cells[r, c] = bv;
            }

        Render();
    }

    // ── Render ────────────────────────────────────────────────────────────────
    private void Render()
    {
        if (_cells == null || _vm == null) return;

        // Locked cells
        for (int r = 0; r < BlockPuzzleViewModel.Rows; r++)
            for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
                _cells[r, c].Color = _vm.Board[r, c]
                    ? _vm.BoardColors[r, c]
                    : Color.FromArgb("#0D1B2A");

        var piece = _vm.CurrentPiece;
        if (piece == null || _vm.IsGameOver) return;

        // Ghost
        int gy = _vm.CurrentY;
        while (_vm.CanPlace(piece, _vm.CurrentX, gy + 1)) gy++;
        if (gy != _vm.CurrentY) Paint(piece, _vm.CurrentX, gy, ghost: true);

        // Active piece
        Paint(piece, _vm.CurrentX, _vm.CurrentY, ghost: false);

        UpdateNextPreview();
    }

    private void Paint(BlockPiece piece, int x, int y, bool ghost)
    {
        int pr = piece.Shape.GetLength(0), pc = piece.Shape.GetLength(1);
        for (int r = 0; r < pr; r++)
            for (int c = 0; c < pc; c++)
            {
                if (piece.Shape[r, c] == 0) continue;
                int br = y + r, bc = x + c;
                if (br < 0 || br >= BlockPuzzleViewModel.Rows) continue;
                if (bc < 0 || bc >= BlockPuzzleViewModel.Cols) continue;
                _cells[br, bc].Color = ghost
                    ? piece.PieceColor.WithAlpha(0.25f)
                    : piece.PieceColor;
            }
    }

    // ── Next-piece preview ────────────────────────────────────────────────────
    private void BuildNextPreview()
    {
        NextPieceGrid.Children.Clear();
        NextPieceGrid.RowDefinitions.Clear();
        NextPieceGrid.ColumnDefinitions.Clear();

        const int N = 4; const double PS = 13;
        for (int i = 0; i < N; i++)
        {
            NextPieceGrid.RowDefinitions.Add(new RowDefinition { Height = PS });
            NextPieceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = PS });
        }
        for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
            {
                var bv = new BoxView { Color = Colors.Transparent, Margin = new Thickness(0.5) };
                Grid.SetRow(bv, r); Grid.SetColumn(bv, c);
                NextPieceGrid.Children.Add(bv);
            }
        UpdateNextPreview();
    }

    private void UpdateNextPreview()
    {
        var next = _vm?.NextPiece;
        if (next == null || NextPieceGrid.Children.Count == 0) return;

        const int N = 4;
        foreach (var ch in NextPieceGrid.Children)
            if (ch is BoxView bv) bv.Color = Colors.Transparent;

        int sr = next.Shape.GetLength(0), sc = next.Shape.GetLength(1);
        int or = (N - sr) / 2, oc = (N - sc) / 2;
        for (int r = 0; r < sr; r++)
            for (int c = 0; c < sc; c++)
            {
                if (next.Shape[r, c] == 0) continue;
                int idx = (or + r) * N + (oc + c);
                if (idx >= 0 && idx < NextPieceGrid.Children.Count
                    && NextPieceGrid.Children[idx] is BoxView bv)
                    bv.Color = next.PieceColor;
            }
    }

    // ── Gesture handlers ─────────────────────────────────────────────────────
    private void OnRotate(object sender, EventArgs e)
    {
        _vm?.RotateCommand.Execute(null);
        AudioService.Instance.Play("pop");
    }

    private void OnPanBoard(object s, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panX = _panY = 0;
                _panFired = false;
                break;

            case GestureStatus.Running:
                _panX = e.TotalX;
                _panY = e.TotalY;

                if (_panFired) break;

                double ax = Math.Abs(_panX), ay = Math.Abs(_panY);

                if (ax >= SwipeMin && ax > ay)
                {
                    _panFired = true;
                    AudioService.Instance.Play("slide");
                    if (_panX > 0) _vm?.MoveRightCommand.Execute(null);
                    else           _vm?.MoveLeftCommand.Execute(null);
                }
                else if (ay >= SwipeMin && ay > ax && _panY > 0)
                {
                    _panFired = true;
                    _vm?.DropCommand.Execute(null);
                    AudioService.Instance.Play("slide");
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _panFired = false;
                break;
        }
    }

    // ── ViewModel callbacks ───────────────────────────────────────────────────
    private void OnBoardChanged()
    {
        if (_vm != null && _vm.LinesCleared > _prevLinesCleared)
        {
            AudioService.Instance.Play("clear");
            _prevLinesCleared = _vm.LinesCleared;
        }
        MainThread.BeginInvokeOnMainThread(Render);
    }

    private async void OnLinesClearing(List<int> rows)
    {
        try
        {
            foreach (int r in rows)
                for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
                    if (_cells[r, c] != null)
                        _cells[r, c].Opacity = 1;

            var fades = new List<Task>();
            foreach (int r in rows)
                for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
                    if (_cells[r, c] != null)
                        fades.Add(_cells[r, c].FadeTo(0, 60, Easing.CubicIn));
            await Task.WhenAll(fades);

            foreach (int r in rows)
                for (int c = 0; c < BlockPuzzleViewModel.Cols; c++)
                    if (_cells[r, c] != null)
                        _cells[r, c].Opacity = 1;
        }
        catch { }
    }

    private async void OnPieceLocked()
    {
        if (SolutionButton != null)
            SolutionButton.IsEnabled = true;

        AudioService.Instance.Play("slide");
        try
        {
            await BoardContainer.TranslateTo(-6, 0, 30);
            await BoardContainer.TranslateTo(6, 0, 30);
            await BoardContainer.TranslateTo(-3, 0, 20);
            await BoardContainer.TranslateTo(0, 0, 20);
        }
        catch { }
    }

    private async void OnGameEnded(bool isWin)
    {
        try
        {
            if (SolutionButton != null)
                SolutionButton.IsEnabled = true;
            int stars  = isWin ? 3 : 1;
            int points = _vm?.Score ?? 0;
            int elapsed = _vm != null
                ? BlockPuzzleViewModel.LevelTimerSeconds - _vm.TimeRemainingSec
                : 0;
            await ResultPopup.Show(isWin, _vm?.Level ?? _startLevel, elapsed, BlockPuzzleViewModel.LevelTimerSeconds, stars, points,
                isWin ? null : "Game Over!", "blockpuzzle");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    // ── Pause / Resume ───────────────────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    private void OnShowSolutionClicked(object sender, EventArgs e)
    {
        if (_vm == null || _vm.IsGameOver || _vm.CurrentPiece == null) return;

        if (CoinService.GetCoins("blockpuzzle") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("blockpuzzle");
            return;
        }
        CoinService.SpendCoins("blockpuzzle", CoinService.SolutionCost);

        SolutionButton.IsEnabled = false;
        _ = AutoPlaceBestAsync();
    }

    // Finds the lowest valid placement for the current piece and plays it.
    private async Task AutoPlaceBestAsync()
    {
        try
        {
            var piece = _vm.CurrentPiece;
            if (piece == null) return;

            int bestX = _vm.CurrentX;
            int bestY = -1;
            for (int x = 0; x < BlockPuzzleViewModel.Cols; x++)
            {
                int y = _vm.CurrentY;
                while (_vm.CanPlace(piece, x, y + 1)) y++;
                if (y >= 0 && _vm.CanPlace(piece, x, y) && y > bestY)
                {
                    bestY = y;
                    bestX = x;
                }
            }

            if (bestY < 0) { SolutionButton.IsEnabled = true; return; }

            while (_vm.CurrentX > bestX)
            {
                _vm.MoveLeftCommand.Execute(null);
                await Task.Delay(90);
            }
            while (_vm.CurrentX < bestX)
            {
                _vm.MoveRightCommand.Execute(null);
                await Task.Delay(90);
            }

            _vm.DropCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AutoPlaceBestAsync: {ex.Message}");
            SolutionButton.IsEnabled = true;
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            Cleanup();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel = (_vm?.Level ?? _startLevel) + 1;
        InitGame(_startLevel);
    }

    private void OnRetry(object sender, EventArgs e) => InitGame(_startLevel);
}
