using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class CtdViewModel : INotifyPropertyChanged
{
    private CtdPuzzle _puzzle;
    private int _level;
    private int? _activePair;
    private readonly List<(int r, int c)> _currentPath = new();
    private readonly Dictionary<int, List<(int r, int c)>> _completedPaths = new();

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

    public ICommand CellTappedCommand { get; }

    public CtdViewModel(int level = 1)
    {
        _level = level;
        CellTappedCommand = new Command<(int r, int c)>(OnCellTapped);
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        _level = level;
        _puzzle = CtdPuzzle.Generate(level);
        _activePair = null;
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

    public void OnCellTapped((int r, int c) cell)
    {
        if (IsGameOver) return;

        int pairIdx = FindPairAt(cell.r, cell.c);
        Console.WriteLine($"[CTD] Tap at ({cell.r},{cell.c}) pairIdx={pairIdx} activePair={_activePair} pathLen={_currentPath.Count}");

        if (pairIdx >= 0)
        {
            Console.WriteLine($"[CTD] Tapped on dot pair {pairIdx} active={_activePair} pathLen={_currentPath.Count}");

            if (_activePair == pairIdx)
            {
                var p = _puzzle.Pairs[pairIdx];
                bool isOnStartCell = (cell.r == _currentPath[0].r && cell.c == _currentPath[0].c);

                // Cancel if tapping the start cell again
                if (isOnStartCell && _currentPath.Count <= 1)
                {
                    Console.WriteLine($"[CTD] Cancelling");
                    _activePair = null;
                    _currentPath.Clear();
                    BoardChanged?.Invoke();
                    return;
                }

                // Check if path already has both endpoints
                bool hasEnd1 = _currentPath.Any(c => c.r == p.R1 && c.c == p.C1);
                bool hasEnd2 = _currentPath.Any(c => c.r == p.R2 && c.c == p.C2);

                if (hasEnd1 && hasEnd2 && _currentPath.Count >= 2)
                {
                    Console.WriteLine($"[CTD] COMPLETING - path has both endpoints!");
                    CompletePath(pairIdx);
                    return;
                }

                // Add dot to path if adjacent and not already in path
                var last = _currentPath[^1];
                bool isAdj = (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) == 1);
                if (isAdj && !_currentPath.Contains(cell))
                {
                    _currentPath.Add(cell);
                    Console.WriteLine($"[CTD] Added dot to path, now [{string.Join(" ", _currentPath.Select(p => $"({p.r},{p.c})"))}]");

                    // Check again after adding
                    hasEnd1 = _currentPath.Any(c => c.r == p.R1 && c.c == p.C1);
                    hasEnd2 = _currentPath.Any(c => c.r == p.R2 && c.c == p.C2);
                    if (hasEnd1 && hasEnd2)
                    {
                        Console.WriteLine($"[CTD] COMPLETING after add!");
                        CompletePath(pairIdx);
                        return;
                    }

                    BoardChanged?.Invoke();
                    return;
                }

                // If not adjacent, just extend path to the dot anyway (skip obstacles)
                if (!_currentPath.Contains(cell))
                {
                    _currentPath.Add(cell);
                    Console.WriteLine($"[CTD] Force-added dot, path now [{string.Join(" ", _currentPath.Select(p => $"({p.r},{p.c})"))}]");

                    hasEnd1 = _currentPath.Any(c => c.r == p.R1 && c.c == p.C1);
                    hasEnd2 = _currentPath.Any(c => c.r == p.R2 && c.c == p.C2);
                    if (hasEnd1 && hasEnd2)
                    {
                        Console.WriteLine($"[CTD] COMPLETING after force-add!");
                        CompletePath(pairIdx);
                        return;
                    }

                    BoardChanged?.Invoke();
                }
            }
            else
            {
                Console.WriteLine($"[CTD] Starting new path for pair {pairIdx}");
                if (_completedPaths.ContainsKey(pairIdx) && _completedPaths[pairIdx].Count > 0)
                    _completedPaths[pairIdx].Clear();

                _activePair = pairIdx;
                _currentPath.Clear();
                _currentPath.Add(cell);
                BoardChanged?.Invoke();
            }
        }
        else if (_activePair.HasValue)
        {
            Console.WriteLine($"[CTD] Empty cell ({cell.r},{cell.c}), extending path");

            // Cancel if tapping start cell
            if (_currentPath.Count == 1 && _currentPath[0] == cell)
            {
                _activePair = null;
                _currentPath.Clear();
                BoardChanged?.Invoke();
                return;
            }

            // Undo last cell if tapping previous
            if (_currentPath.Count >= 2 && _currentPath[^2] == cell)
            {
                _currentPath.RemoveAt(_currentPath.Count - 1);
                BoardChanged?.Invoke();
                return;
            }

            // Check adjacency
            var last = _currentPath[^1];
            if (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) != 1)
            {
                Console.WriteLine($"[CTD] Not adjacent, skipping");
                return;
            }

            if (IsCellOwnedByOther(cell, _activePair.Value))
            {
                Console.WriteLine($"[CTD] Cell owned by other path");
                return;
            }
            if (_currentPath.Contains(cell))
            {
                Console.WriteLine($"[CTD] Already in path");
                return;
            }

            _currentPath.Add(cell);
            Console.WriteLine($"[CTD] Path: [{string.Join(" ", _currentPath.Select(p => $"({p.r},{p.c})"))}]");
            BoardChanged?.Invoke();
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
        if (filled.Count < total * 0.8f) return;

        IsGameOver = true;
        GameEnded?.Invoke(true);
    }

    public int GetCellColor(int r, int c)
    {
        int val = _puzzle.Grid[r, c];
        if (val > 0) return val;

        if (_activePair.HasValue)
        {
            if (_currentPath.Contains((r, c)))
                return _activePair.Value + 1;
        }

        foreach (var kvp in _completedPaths)
        {
            if (kvp.Value.Contains((r, c)))
                return kvp.Key + 1;
        }

        return 0;
    }

    public bool IsDot(int r, int c)
    {
        return _puzzle.Grid[r, c] > 0;
    }

    public Color GetPairColor(int pairIdx)
    {
        if (pairIdx >= 0 && pairIdx < _puzzle.Pairs.Count)
            return _puzzle.Pairs[pairIdx].Color;
        return Colors.Transparent;
    }

    public void Restart()
    {
        StartLevel(_level);
    }

    public void Cleanup()
    {
        BoardChanged = null;
        GameEnded = null;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
