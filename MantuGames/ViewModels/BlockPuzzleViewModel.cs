using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class BlockPuzzleViewModel : INotifyPropertyChanged
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public const int Rows = 20;
    public const int Cols = 10;

    // ── Backing fields ────────────────────────────────────────────────────────
    private bool[,]  _board;
    private Color[,] _boardColors;
    private BlockPiece _currentPiece;
    private BlockPiece _nextPiece;
    private int  _currentX;
    private int  _currentY;
    private int  _score;
    private int  _level;
    private int  _linesCleared;
    private int  _timeRemainingSec;
    private bool _isGameOver;
    private System.Threading.Timer _dropTimer;
    private IDispatcherTimer _gameTimer;
    private static readonly Random Rng = new();

    // ── Public properties ─────────────────────────────────────────────────────
    public bool[,] Board
    {
        get => _board;
        private set { _board = value; OnPropertyChanged(); }
    }

    public Color[,] BoardColors
    {
        get => _boardColors;
        private set { _boardColors = value; OnPropertyChanged(); }
    }

    public BlockPiece CurrentPiece
    {
        get => _currentPiece;
        private set { _currentPiece = value; OnPropertyChanged(); }
    }

    public BlockPiece NextPiece
    {
        get => _nextPiece;
        private set { _nextPiece = value; OnPropertyChanged(); }
    }

    public int CurrentX
    {
        get => _currentX;
        private set { _currentX = value; OnPropertyChanged(); }
    }

    public int CurrentY
    {
        get => _currentY;
        private set { _currentY = value; OnPropertyChanged(); }
    }

    public int Score
    {
        get => _score;
        private set { _score = value; OnPropertyChanged(); OnPropertyChanged(nameof(ScoreDisplay)); }
    }

    public int Level
    {
        get => _level;
        private set { _level = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public int LinesCleared
    {
        get => _linesCleared;
        private set { _linesCleared = value; OnPropertyChanged(); OnPropertyChanged(nameof(LinesDisplay)); }
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    /// <summary>Level completes once this many lines are cleared.</summary>
    public int LevelTargetLines => Math.Min(30, 18 + 2 * (_level - 1));

    /// <summary>Level time budget in seconds (5 minutes — enough for a full level).</summary>
    public const int LevelTimerSeconds = 300;

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        private set { _timeRemainingSec = value; OnPropertyChanged(); }
    }

    public string LevelDisplay => $"Level {_level}";
    public string ScoreDisplay => $"Score: {_score}";
    public string LinesDisplay => $"{_linesCleared}/{LevelTargetLines}";

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand MoveLeftCommand   { get; }
    public ICommand MoveRightCommand  { get; }
    public ICommand RotateCommand     { get; }
    public ICommand DropCommand       { get; }

    // ── Events ────────────────────────────────────────────────────────────────
    public event Action<bool> GameEnded;
    public event Action BoardChanged;
    public event Action PieceLocked;
    public event Action<List<int>> LinesClearing;

    // ── Constructor ───────────────────────────────────────────────────────────
    public BlockPuzzleViewModel(int level = 1)
    {
        MoveLeftCommand  = new Command(() => TryMove(-1, 0));
        MoveRightCommand = new Command(() => TryMove(1, 0));
        RotateCommand    = new Command(TryRotate);
        DropCommand      = new Command(HardDrop);

        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        StopTimer();
        StopGameTimer();
        Level        = level;
        Score        = 0;
        LinesCleared = 0;
        TimeRemainingSec = LevelTimerSeconds;
        IsGameOver   = false;
        _board       = new bool[Rows, Cols];
        _boardColors = new Color[Rows, Cols];

        // Fill with transparent color
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                _boardColors[r, c] = Colors.Transparent;

        SpawnNextPiece();
        SpawnPiece();
        StartTimer();
        StartGameTimer();
        BoardChanged?.Invoke();
    }

    // ── Piece spawning ────────────────────────────────────────────────────────
    private void SpawnNextPiece()
    {
        NextPiece = BlockPiece.All[Rng.Next(BlockPiece.All.Count)];
    }

    private void SpawnPiece()
    {
        CurrentPiece = NextPiece ?? BlockPiece.All[Rng.Next(BlockPiece.All.Count)];
        SpawnNextPiece();

        // Center the piece horizontally, spawn at top
        int pieceW = CurrentPiece.Shape.GetLength(1);
        _currentX  = (Cols - pieceW) / 2;
        _currentY  = 0;

        // Check if spawn position is blocked → game over
        if (!CanPlace(CurrentPiece, _currentX, _currentY))
        {
            IsGameOver = true;
            StopTimer();
            StopGameTimer();
            GameEnded?.Invoke(false);
        }
    }

    // ── Placement check ───────────────────────────────────────────────────────
    public bool CanPlace(BlockPiece piece, int x, int y)
    {
        int rows = piece.Shape.GetLength(0);
        int cols = piece.Shape.GetLength(1);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (piece.Shape[r, c] == 0) continue;

                int boardR = y + r;
                int boardC = x + c;

                if (boardR < 0 || boardR >= Rows) return false;
                if (boardC < 0 || boardC >= Cols) return false;
                if (_board[boardR, boardC])        return false;
            }

        return true;
    }

    // ── Movement ──────────────────────────────────────────────────────────────
    private void TryMove(int dx, int dy)
    {
        if (_isGameOver || _currentPiece == null) return;
        if (CanPlace(_currentPiece, _currentX + dx, _currentY + dy))
        {
            _currentX += dx;
            _currentY += dy;
            OnPropertyChanged(nameof(CurrentX));
            OnPropertyChanged(nameof(CurrentY));
            BoardChanged?.Invoke();
        }
    }

    private void TryRotate()
    {
        if (_isGameOver || _currentPiece == null) return;

        var rotated = _currentPiece.RotateClockwise();

        // Wall kick — try original, then shift right, then left, then right 2
        int[] kicks = { 0, 1, -1, 2, -2 };
        foreach (int kick in kicks)
        {
            if (CanPlace(rotated, _currentX + kick, _currentY))
            {
                CurrentPiece = rotated;
                _currentX   += kick;
                OnPropertyChanged(nameof(CurrentX));
                BoardChanged?.Invoke();
                return;
            }
        }
    }

    // ── Auto-drop timer ───────────────────────────────────────────────────────
    private void StartTimer()
    {
        int intervalMs = DropIntervalMs(_level);
        _dropTimer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(AutoDrop);
        }, null, intervalMs, intervalMs);
    }

    private void StopTimer() { _dropTimer?.Dispose(); _dropTimer = null; }

    private static int DropIntervalMs(int level)
    {
        // Starts at 800ms, decreases by 50ms per level, minimum 100ms
        return Math.Max(100, 800 - (level - 1) * 50);
    }

    private void AutoDrop()
    {
        if (_isGameOver) return;

        if (CanPlace(_currentPiece, _currentX, _currentY + 1))
        {
            _currentY++;
            OnPropertyChanged(nameof(CurrentY));
            BoardChanged?.Invoke();
        }
        else
        {
            LockPiece();
        }
    }

    // ── Hard drop ─────────────────────────────────────────────────────────────
    private void HardDrop()
    {
        if (_isGameOver || _currentPiece == null) return;

        // Find lowest valid Y
        while (CanPlace(_currentPiece, _currentX, _currentY + 1))
            _currentY++;

        OnPropertyChanged(nameof(CurrentY));
        LockPiece();
    }

    // ── Lock piece into board ─────────────────────────────────────────────────
    private void PlacePiece()
    {
        if (_currentPiece == null) return;
        int rows = _currentPiece.Shape.GetLength(0);
        int cols = _currentPiece.Shape.GetLength(1);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (_currentPiece.Shape[r, c] == 0) continue;
                int br = _currentY + r;
                int bc = _currentX + c;
                if (br >= 0 && br < Rows && bc >= 0 && bc < Cols)
                {
                    _board[br, bc]       = true;
                    _boardColors[br, bc] = _currentPiece.PieceColor;
                }
            }
    }

    private void LockPiece()
    {
        PlacePiece();

        var fullRows = new List<int>();
        for (int r = Rows - 1; r >= 0; r--)
        {
            bool full = true;
            for (int c = 0; c < Cols; c++)
                if (!_board[r, c]) { full = false; break; }
            if (full) fullRows.Add(r);
        }

        if (fullRows.Count > 0)
            LinesClearing?.Invoke(fullRows);

        int cleared = ClearLines();
        UpdateScore(cleared);
        LinesCleared += cleared;

        PieceLocked?.Invoke();
        BoardChanged?.Invoke();

        // Level complete — enough lines cleared
        if (LinesCleared >= LevelTargetLines)
        {
            IsGameOver = true;
            StopTimer();
            StopGameTimer();
            GameEnded?.Invoke(true);
            return;
        }

        SpawnPiece();
        BoardChanged?.Invoke();
    }

    // ── Clear completed lines ─────────────────────────────────────────────────
    private int ClearLines()
    {
        int cleared = 0;

        for (int r = Rows - 1; r >= 0; r--)
        {
            bool full = true;
            for (int c = 0; c < Cols; c++)
                if (!_board[r, c]) { full = false; break; }

            if (!full) continue;

            cleared++;

            // Shift everything above down
            for (int rr = r; rr > 0; rr--)
                for (int c = 0; c < Cols; c++)
                {
                    _board[rr, c]       = _board[rr - 1, c];
                    _boardColors[rr, c] = _boardColors[rr - 1, c];
                }

            // Clear top row
            for (int c = 0; c < Cols; c++)
            {
                _board[0, c]       = false;
                _boardColors[0, c] = Colors.Transparent;
            }

            r++; // recheck same row index
        }

        return cleared;
    }

    // ── Score calculation ─────────────────────────────────────────────────────
    private void UpdateScore(int lines)
    {
        int[] lineScores = { 0, 100, 300, 500, 800 };
        int pts = lines < lineScores.Length ? lineScores[lines] : 1200;
        Score += pts * _level;
    }

    // ── Level countdown timer ──────────────────────────────────────────────
    private void StartGameTimer()
    {
        _gameTimer = Application.Current?.Dispatcher?.CreateTimer();
        if (_gameTimer == null) return;
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();
    }

    private void StopGameTimer()
    {
        if (_gameTimer == null) return;
        _gameTimer.Tick -= OnGameTick;
        _gameTimer.Stop();
        _gameTimer = null;
    }

    private void OnGameTick(object? s, EventArgs e)
    {
        if (_isGameOver) return;
        TimeRemainingSec = Math.Max(0, TimeRemainingSec - 1);
        if (TimeRemainingSec <= 0)
        {
            IsGameOver = true;
            StopTimer();
            StopGameTimer();
            GameEnded?.Invoke(false);
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────
    public void Cleanup() { StopTimer(); StopGameTimer(); }
    public void PauseTimer() { StopTimer(); StopGameTimer(); }
    public void ResumeTimer() { StartTimer(); StartGameTimer(); }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
