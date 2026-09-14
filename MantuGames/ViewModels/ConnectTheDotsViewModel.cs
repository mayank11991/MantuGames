using System.ComponentModel;
using System.Runtime.CompilerServices;
using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class CtdViewModel : INotifyPropertyChanged
{
    private CtdPuzzle _puzzle;
    private int _level;
    private int? _activePair;
    private readonly List<(int r, int c)> _currentPath = new();
    private readonly Dictionary<int, List<(int r, int c)>> _completedPaths = new();
    private System.Threading.Timer _timer;

    public int Rows => _puzzle?.Rows ?? 5;
    public int Cols => _puzzle?.Cols ?? 5;
    public List<CtdPair> Pairs => _puzzle?.Pairs ?? new();
    public Dictionary<int, List<(int r, int c)>> CompletedPaths => _completedPaths;
    public int? ActivePair => _activePair;
    public List<(int r, int c)> CurrentPath => _currentPath;
    public int CurrentLevel => _level;

    private string _levelDisplay = "Level 1";
    public string LevelDisplay { get => _levelDisplay; set { _levelDisplay = value; OnPropertyChanged(); } }

    private string _difficulty = "EASY";
    public string Difficulty { get => _difficulty; set { _difficulty = value; OnPropertyChanged(); } }

    private bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; set { _isGameOver = value; OnPropertyChanged(); } }

    private int _timeRemainingSec;
    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        set { _timeRemainingSec = value; OnPropertyChanged(); }
    }

    public event Action BoardChanged;
    public event Action<bool> GameEnded;

    public CtdViewModel(int level = 1)
    {
        _level = level;
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        _level = level;
        _puzzle = CtdPuzzle.Generate(level);
        _activePair = null;
        _currentPath.Clear();
        _completedPaths.Clear();
        StopTimer();

        for (int i = 0; i < _puzzle.Pairs.Count; i++)
            _completedPaths[i] = new List<(int, int)>();

        LevelDisplay = $"Level {level}";
        Difficulty = level switch
        {
            <= 5 => "EASY",
            <= 15 => "MEDIUM",
            <= 25 => "HARD",
            _ => "EXPERT"
        };

        IsGameOver = false;
        TimeRemainingSec = ProgressService.GetTimerSeconds(level);
        BoardChanged?.Invoke();
        StartTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsGameOver) return;
                if (TimeRemainingSec > 0) TimeRemainingSec--;
                else
                {
                    IsGameOver = true;
                    StopTimer();
                    GameEnded?.Invoke(false);
                }
            });
        }, null, 1000, 1000);
    }

    public void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void PauseTimer() => StopTimer();

    public void ResumeTimer()
    {
        if (!IsGameOver) StartTimer();
    }

    public void Restart() => StartLevel(_level);

    public void Cleanup()
    {
        StopTimer();
        BoardChanged = null;
        GameEnded = null;
    }

    public void OnPointerDown((int r, int c) cell)
    {
        if (IsGameOver) return;

        int pair = FindPairAt(cell.r, cell.c);

        if (pair >= 0)
        {
            if (_completedPaths.ContainsKey(pair) && _completedPaths[pair].Count > 0)
                _completedPaths[pair].Clear();

            _activePair = pair;
            _currentPath.Clear();
            _currentPath.Add(cell);
            BoardChanged?.Invoke();
            return;
        }

        _activePair = null;
        _currentPath.Clear();
    }

    public void OnPointerDrag((int r, int c) cell)
    {
        if (IsGameOver || !_activePair.HasValue) return;
        if (_currentPath.Contains(cell)) return;
        if (IsCellOwnedByOther(cell, _activePair.Value)) return;

        var last = _currentPath[^1];
        if (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) != 1) return;

        int tappedPair = FindPairAt(cell.r, cell.c);

        if (tappedPair >= 0 && tappedPair != _activePair.Value)
            return;

        if (tappedPair == _activePair.Value)
        {
            var p = _puzzle.Pairs[_activePair.Value];
            bool isStart = (_currentPath[0].r == p.R1 && _currentPath[0].c == p.C1);
            bool isEnd = (_currentPath[0].r == p.R2 && _currentPath[0].c == p.C2);

            bool reachedEnd = (isStart && cell.r == p.R2 && cell.c == p.C2);
            bool reachedStart = (isEnd && cell.r == p.R1 && cell.c == p.C1);

            if ((reachedEnd || reachedStart) && _currentPath.Count >= 2)
            {
                _currentPath.Add(cell);
                CompletePath(_activePair.Value);
                return;
            }
            return;
        }

        _currentPath.Add(cell);
        BoardChanged?.Invoke();
    }

    public void OnPointerUp((int r, int c) cell)
    {
        if (IsGameOver || !_activePair.HasValue) return;
        BoardChanged?.Invoke();
    }

    private int FindPairAt(int r, int c)
    {
        for (int i = 0; i < _puzzle.Pairs.Count; i++)
        {
            var p = _puzzle.Pairs[i];
            if ((p.R1 == r && p.C1 == c) || (p.R2 == r && p.C2 == c))
                return i;
        }
        return -1;
    }

    private bool IsCellOwnedByOther((int r, int c) cell, int excludePair)
    {
        foreach (var kvp in _completedPaths)
        {
            if (kvp.Key == excludePair) continue;
            if (kvp.Value.Contains(cell)) return true;
        }
        return false;
    }

    private void CompletePath(int pairIdx)
    {
        _completedPaths[pairIdx] = new List<(int, int)>(_currentPath);
        _activePair = null;
        _currentPath.Clear();
        BoardChanged?.Invoke();
        CheckWin();
    }

    private void CheckWin()
    {
        for (int i = 0; i < _puzzle.Pairs.Count; i++)
        {
            if (_completedPaths[i].Count == 0) return;
        }

        var filled = new HashSet<(int, int)>();
        foreach (var kvp in _completedPaths)
            foreach (var cell in kvp.Value)
                filled.Add(cell);

        int total = _puzzle.Rows * _puzzle.Cols;
        if (filled.Count < total * 0.7f) return;

        IsGameOver = true;
        StopTimer();
        GameEnded?.Invoke(true);
    }

    public Color GetPairColor(int pairIdx)
    {
        if (pairIdx >= 0 && pairIdx < _puzzle.Pairs.Count)
            return _puzzle.Pairs[pairIdx].Color;
        return Colors.Transparent;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
