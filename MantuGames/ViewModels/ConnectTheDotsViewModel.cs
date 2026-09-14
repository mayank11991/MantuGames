using System.ComponentModel;
using System.Runtime.CompilerServices;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class CtdViewModel : INotifyPropertyChanged
{
    private CtdPuzzle _puzzle;
    private int _level;
    private int? _activePair;
    private readonly List<(int r, int c)> _currentPath = new();
    private readonly Dictionary<int, List<(int r, int c)>> _completedPaths = new();
    private (int r, int c) _startDot;

    public int Rows => _puzzle?.Rows ?? 5;
    public int Cols => _puzzle?.Cols ?? 5;
    public List<CtdPair> Pairs => _puzzle?.Pairs ?? new();
    public Dictionary<int, List<(int r, int c)>> CompletedPaths => _completedPaths;
    public int? ActivePair => _activePair;
    public List<(int r, int c)> CurrentPath => _currentPath;

    private string _levelDisplay = "Level 1";
    public string LevelDisplay { get => _levelDisplay; set { _levelDisplay = value; OnPropertyChanged(); } }

    private string _difficulty = "EASY";
    public string Difficulty { get => _difficulty; set { _difficulty = value; OnPropertyChanged(); } }

    private bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; set { _isGameOver = value; OnPropertyChanged(); } }

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
        _startDot = (-1, -1);
        _currentPath.Clear();
        _completedPaths.Clear();

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
        BoardChanged?.Invoke();
    }

    public void Restart() => StartLevel(_level);

    public void Cleanup()
    {
        BoardChanged = null;
        GameEnded = null;
    }

    // Called when finger touches the screen
    public void OnPointerDown((int r, int c) cell)
    {
        if (IsGameOver) return;

        int pairIdx = FindPairAt(cell.r, cell.c);
        Console.WriteLine($"[CTD] PointerDown ({cell.r},{cell.c}) pairIdx={pairIdx}");

        if (pairIdx >= 0)
        {
            // Touching a dot - start a new path
            if (_completedPaths.ContainsKey(pairIdx) && _completedPaths[pairIdx].Count > 0)
                _completedPaths[pairIdx].Clear();

            _activePair = pairIdx;
            _startDot = cell;
            _currentPath.Clear();
            _currentPath.Add(cell);
            BoardChanged?.Invoke();
        }
    }

    // Called while finger moves across cells
    public void OnPointerDrag((int r, int c) cell)
    {
        if (IsGameOver || !_activePair.HasValue) return;

        int pairIdx = _activePair.Value;

        // Dragging onto a dot
        int tappedPair = FindPairAt(cell.r, cell.c);
        if (tappedPair >= 0)
        {
            // Same pair dot - try to complete
            if (tappedPair == pairIdx)
            {
                var p = _puzzle.Pairs[pairIdx];
                bool isEnd1 = (cell.r == p.R1 && cell.c == p.C1);
                bool isEnd2 = (cell.r == p.R2 && cell.c == p.C2);
                bool startedAtEnd1 = (_startDot.r == p.R1 && _startDot.c == p.C1);

                // Must be the OTHER dot (not the start)
                if ((startedAtEnd1 && isEnd2) || (!startedAtEnd1 && isEnd1))
                {
                    if (!_currentPath.Contains(cell))
                        _currentPath.Add(cell);

                    Console.WriteLine($"[CTD] COMPLETING via drag! Path has {(_currentPath.Count)} cells");
                    CompletePath(pairIdx);
                    return;
                }
            }
            // Different pair - skip
            return;
        }

        // Empty cell - add to path if adjacent and not already in path
        if (_currentPath.Count > 0)
        {
            var last = _currentPath[^1];
            if (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) == 1)
            {
                if (!_currentPath.Contains(cell))
                {
                    _currentPath.Add(cell);
                    BoardChanged?.Invoke();
                }
            }
        }
    }

    // Called when finger lifts off
    public void OnPointerUp((int r, int c) cell)
    {
        if (IsGameOver || !_activePair.HasValue) return;

        int pairIdx = _activePair.Value;
        var p = _puzzle.Pairs[pairIdx];

        Console.WriteLine($"[CTD] PointerUp ({cell.r},{cell.c}) pathLen={_currentPath.Count}");

        // Check if we ended on the matching dot
        bool isEnd1 = (cell.r == p.R1 && cell.c == p.C1);
        bool isEnd2 = (cell.r == p.R2 && cell.c == p.C2);
        bool startedAtEnd1 = (_startDot.r == p.R1 && _startDot.c == p.C1);

        bool reachedOtherEnd = (startedAtEnd1 && isEnd2) || (!startedAtEnd1 && isEnd1);

        if (reachedOtherEnd && _currentPath.Count >= 2)
        {
            if (!_currentPath.Contains(cell))
                _currentPath.Add(cell);

            Console.WriteLine($"[CTD] COMPLETING via release!");
            CompletePath(pairIdx);
        }
        else
        {
            Console.WriteLine($"[CTD] Path not complete - startedAtEnd1={startedAtEnd1} isEnd1={isEnd1} isEnd2={isEnd2} pathLen={_currentPath.Count}");
        }
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

    private void CompletePath(int pairIdx)
    {
        _completedPaths[pairIdx] = new List<(int, int)>(_currentPath);
        _activePair = null;
        _startDot = (-1, -1);
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
