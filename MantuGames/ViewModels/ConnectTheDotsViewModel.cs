using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.Views;

namespace MantuGames.ViewModels;

public class ConnectTheDotsViewModel : INotifyPropertyChanged
{
    private ConnectTheDotsPuzzle _puzzle;
    private System.Threading.Timer _timer;
    private bool _isGameOver, _isWin;
    private int _timeRemainingSec;
    private int _currentLevel;
    private int _score;
    private int? _activePairId = null;
    private readonly List<int> _currentPath = new();
    private readonly Dictionary<int, List<int>> _completedPaths = new();
    private bool _solutionWasShown = false;
    private IDrawable _drawable;

    public IDrawable Drawable => _drawable ??= new ConnectTheDotsDrawable(this);

    public bool SolutionWasShown => _solutionWasShown;

    private string _timerDisplay;
    public string TimerDisplay
    {
        get => _timerDisplay;
        private set { _timerDisplay = value; OnPropertyChanged(); }
    }

    private double _timerProgress;
    public double TimerProgress
    {
        get => _timerProgress;
        private set { _timerProgress = value; OnPropertyChanged(); }
    }

    public int GridSize => _puzzle?.GridSize ?? 5;

    public int TotalTimerSeconds => _puzzle?.TimerSeconds ?? 120;

    public int PuzzleTimerSeconds => _puzzle?.TimerSeconds ?? 120;

    public IReadOnlyList<DotPair> Pairs => _puzzle?.Pairs ?? new List<DotPair>();

    public IReadOnlySet<int> Obstacles => _puzzle?.Obstacles ?? new HashSet<int>();

    public IReadOnlyDictionary<int, List<int>> SolutionPaths => _puzzle?.SolutionPaths ?? new Dictionary<int, List<int>>();

    public IReadOnlyDictionary<int, List<int>> CompletedPaths => _completedPaths;

    public int? ActivePairId => _activePairId;

    public IReadOnlyList<int> CurrentPath => _currentPath;

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        private set
        {
            _timeRemainingSec = value;
            OnPropertyChanged();
            UpdateTimerDisplay();
        }
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    public bool IsWin
    {
        get => _isWin;
        private set { _isWin = value; OnPropertyChanged(); }
    }

    public int CurrentLevel
    {
        get => _currentLevel;
        private set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public int Score
    {
        get => _score;
        set { _score = value; OnPropertyChanged(); }
    }

    public ICommand CellTappedCommand { get; }
    public ICommand ShowSolutionCommand { get; }

    public event Action<bool> GameEnded;
    public event Action BoardChanged;
    public event Action<int, int> CellTouched;

    public ConnectTheDotsViewModel(int level = 1)
    {
        CellTappedCommand = new Command<int>(OnCellTapped);
        ShowSolutionCommand = new Command(ShowSolution);
        StartLevel(level);
    }

    private void StartLevel(int level)
    {
        CurrentLevel = level;
        _puzzle = ConnectTheDotsPuzzle.Generate(level);
        TimeRemainingSec = _puzzle.TimerSeconds;
        IsGameOver = false;
        IsWin = false;
        _solutionWasShown = false;
        Score = 0;
        _activePairId = null;
        _currentPath.Clear();
        _completedPaths.Clear();

        foreach (var pair in _puzzle.Pairs)
            _completedPaths[pair.Id] = new List<int>();

        StopTimer();
        StartTimer();
        BoardChanged?.Invoke();
    }

    private void UpdateTimerDisplay()
    {
        TimerDisplay = $"{TimeRemainingSec / 60}:{TimeRemainingSec % 60:D2}";
        TimerProgress = _puzzle != null ? (double)TimeRemainingSec / _puzzle.TimerSeconds : 1.0;
    }

    private void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (TimeRemainingSec > 0)
                    TimeRemainingSec--;
                else
                    EndGame(false);
            });
        }, null, 1000, 1000);
    }

    public void Cleanup() => StopTimer();
    public void PauseTimer() => StopTimer();
    public void ResumeTimer() => StartTimer();

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void OnCellTapped(int cellIndex)
    {
        if (IsGameOver || _puzzle == null) return;

        int size = _puzzle.GridSize;
        if (_puzzle.Obstacles.Contains(cellIndex)) return;

        int row = cellIndex / size;
        int col = cellIndex % size;
        CellTouched?.Invoke(row, col);

        // Check if this cell is a dot (start/end of a pair)
        var pairAtCell = FindPairAtCell(row, col);

        if (pairAtCell != null)
        {
            // Tapping on a dot
            if (_activePairId == null)
            {
                // No active pair — start drawing this one
                // If this pair was already completed, clear it first
                if (_completedPaths[pairAtCell.Id].Count > 0)
                    ClearCompletedPath(pairAtCell.Id);

                _activePairId = pairAtCell.Id;
                _currentPath.Clear();
                _currentPath.Add(cellIndex);
                BoardChanged?.Invoke();
            }
            else if (_activePairId == pairAtCell.Id)
            {
                // Tapping on same pair — if we have a path, complete it
                if (_currentPath.Count > 1 && IsLastInCurrentPath(cellIndex))
                {
                    // Only complete if we're at the OTHER end of the pair
                    int startIdx = pairAtCell.StartIndex(size);
                    int endIdx = pairAtCell.EndIndex(size);
                    if (cellIndex == startIdx || cellIndex == endIdx)
                    {
                        if (_currentPath[0] == startIdx || _currentPath[0] == endIdx)
                        {
                            CompletePath(pairAtCell.Id);
                            return;
                        }
                    }
                }
                // Tapping on the first cell — cancel
                if (_currentPath.Count == 1 && _currentPath[0] == cellIndex)
                {
                    _activePairId = null;
                    _currentPath.Clear();
                    BoardChanged?.Invoke();
                }
            }
            else
            {
                // Tapping on a different pair — switch to it
                if (_completedPaths[pairAtCell.Id].Count > 0)
                    ClearCompletedPath(pairAtCell.Id);

                _activePairId = pairAtCell.Id;
                _currentPath.Clear();
                _currentPath.Add(cellIndex);
                BoardChanged?.Invoke();
            }
        }
        else if (_activePairId.HasValue)
        {
            // Tapping on an empty cell while drawing
            if (_currentPath.Count > 0)
            {
                int lastCell = _currentPath[^1];

                // Check if tapping the previous cell — erase last step
                if (_currentPath.Count >= 2 && cellIndex == _currentPath[^2])
                {
                    _currentPath.RemoveAt(_currentPath.Count - 1);
                    BoardChanged?.Invoke();
                    return;
                }

                // Must be adjacent
                if (!IsAdjacent(lastCell, cellIndex)) return;

                // Must not be in any completed path (except our own)
                if (IsCellOwnedByOtherCompletedPath(cellIndex, _activePairId.Value)) return;

                // Must not be in current path (no loops)
                if (_currentPath.Contains(cellIndex)) return;

                _currentPath.Add(cellIndex);
                BoardChanged?.Invoke();
            }
        }
    }

    private bool IsLastInCurrentPath(int cellIndex)
    {
        return _currentPath.Count > 0 && _currentPath[^1] == cellIndex;
    }

    private bool IsCellOwnedByOtherCompletedPath(int cellIndex, int excludePairId)
    {
        foreach (var kvp in _completedPaths)
        {
            if (kvp.Key == excludePairId) continue;
            if (kvp.Value.Contains(cellIndex)) return true;
        }
        return false;
    }

    private void ClearCompletedPath(int pairId)
    {
        _completedPaths[pairId].Clear();
        BoardChanged?.Invoke();
    }

    private DotPair FindPairAtCell(int row, int col)
    {
        foreach (var pair in _puzzle.Pairs)
        {
            if ((pair.StartRow == row && pair.StartCol == col) ||
                (pair.EndRow == row && pair.EndCol == col))
                return pair;
        }
        return null;
    }

    private bool IsAdjacent(int cell1, int cell2)
    {
        int size = _puzzle.GridSize;
        int r1 = cell1 / size, c1 = cell1 % size;
        int r2 = cell2 / size, c2 = cell2 % size;
        return Math.Abs(r1 - r2) + Math.Abs(c1 - c2) == 1;
    }

    private void CompletePath(int pairId)
    {
        // Commit the current path to completed paths
        _completedPaths[pairId] = new List<int>(_currentPath);
        _activePairId = null;
        _currentPath.Clear();
        Score += 100 * CurrentLevel;

        AudioService.Instance.Play("correct");
        BoardChanged?.Invoke();

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        int size = _puzzle.GridSize;
        int totalCells = size * size;

        // Check all pairs are completed
        foreach (var pair in _puzzle.Pairs)
        {
            if (_completedPaths[pair.Id].Count == 0)
                return;
        }

        // Check the entire board is filled (all non-obstacle cells are in some path)
        var allFilled = new HashSet<int>();
        foreach (var kvp in _completedPaths)
        {
            foreach (int cell in kvp.Value)
                allFilled.Add(cell);
        }

        for (int i = 0; i < totalCells; i++)
        {
            if (_puzzle.Obstacles.Contains(i)) continue;
            if (!allFilled.Contains(i)) return;
        }

        EndGame(true);
    }

    private void ShowSolution()
    {
        if (IsGameOver) return;
        StopTimer();
        IsGameOver = true;
        _solutionWasShown = true;

        _completedPaths.Clear();
        foreach (var pair in _puzzle.Pairs)
            _completedPaths[pair.Id] = new List<int>(_puzzle.SolutionPaths[pair.Id]);

        IsWin = false;
        BoardChanged?.Invoke();
        GameEnded?.Invoke(false);
    }

    private void EndGame(bool win)
    {
        StopTimer();
        IsWin = win;
        IsGameOver = true;
        GameEnded?.Invoke(win);
    }

    public int GetCellOwner(int cellIndex)
    {
        if (_puzzle == null) return -1;
        if (_puzzle.Obstacles.Contains(cellIndex)) return -2;

        // Check if cell is in the active (drawing) path
        if (_activePairId.HasValue && _currentPath.Contains(cellIndex))
            return _activePairId.Value;

        // Check completed paths
        foreach (var kvp in _completedPaths)
        {
            if (kvp.Value.Contains(cellIndex))
                return kvp.Key;
        }

        return -1;
    }

    public bool IsCellInActivePath(int cellIndex) => _activePairId.HasValue && _currentPath.Contains(cellIndex);

    public bool IsCellCompleted(int cellIndex)
    {
        foreach (var kvp in _completedPaths)
        {
            if (kvp.Value.Contains(cellIndex))
                return true;
        }
        return false;
    }

    public Color GetPairColor(int pairId)
    {
        var pair = _puzzle?.Pairs.FirstOrDefault(p => p.Id == pairId);
        return pair?.Color ?? Colors.Transparent;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
