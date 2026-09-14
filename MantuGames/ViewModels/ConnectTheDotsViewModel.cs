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
            Console.WriteLine($"[CTD] Tapped on dot pair {pairIdx}");

            if (_activePair == pairIdx)
            {
                var p = _puzzle.Pairs[pairIdx];

                // If path exists and last cell is adjacent to this dot, complete it
                if (_currentPath.Count >= 1)
                {
                    var last = _currentPath[^1];
                    bool isEnd1 = (cell.r == p.R1 && cell.c == p.C1);
                    bool isEnd2 = (cell.r == p.R2 && cell.c == p.C2);
                    bool isAdjToLast = (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) == 1);

                    Console.WriteLine($"[CTD] Check complete: isEnd1={isEnd1} isEnd2={isEnd2} isAdj={isAdjToLast} pathLen={_currentPath.Count}");

                    // Don't complete if we're still on the start cell
                    bool isOnStartCell = (cell.r == _currentPath[0].r && cell.c == _currentPath[0].c);

                    if (!isOnStartCell && isAdjToLast)
                    {
                        if (_currentPath.Count >= 2)
                        {
                            // Path has intermediate cells - check we're reaching the other end
                            bool startedAtEnd1 = (_currentPath[0].r == p.R1 && _currentPath[0].c == p.C1);
                            bool startedAtEnd2 = (_currentPath[0].r == p.R2 && _currentPath[0].c == p.C2);

                            if ((startedAtEnd1 && isEnd2) || (startedAtEnd2 && isEnd1))
                            {
                                Console.WriteLine($"[CTD] COMPLETING PATH!");
                                _currentPath.Add(cell);
                                CompletePath(pairIdx);
                                return;
                            }
                        }

                        // Just extend path to this dot
                        _currentPath.Add(cell);
                        BoardChanged?.Invoke();
                        return;
                    }
                }

                // Cancel if tapping same start cell
                if (_currentPath.Count == 1 && _currentPath[0] == cell)
                {
                    Console.WriteLine($"[CTD] Cancelling - same cell");
                    _activePair = null;
                    _currentPath.Clear();
                    BoardChanged?.Invoke();
                    return;
                }
            }
            else
            {
                Console.WriteLine($"[CTD] Different pair, starting new path");
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
            Console.WriteLine($"[CTD] Empty cell, extending path");
            if (_currentPath.Count == 1 && _currentPath[0] == cell)
            {
                _activePair = null;
                _currentPath.Clear();
                BoardChanged?.Invoke();
                return;
            }

            if (_currentPath.Count >= 2 && _currentPath[^2] == cell)
            {
                _currentPath.RemoveAt(_currentPath.Count - 1);
                BoardChanged?.Invoke();
                return;
            }

            var last = _currentPath[^1];
            if (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) != 1)
            {
                Console.WriteLine($"[CTD] Not adjacent: last=({last.r},{last.c}) cell=({cell.r},{cell.c})");
                return;
            }

            if (IsCellOwnedByOther(cell, _activePair.Value))
            {
                Console.WriteLine($"[CTD] Cell owned by other path");
                return;
            }
            if (_currentPath.Contains(cell))
            {
                Console.WriteLine($"[CTD] Cell already in path");
                return;
            }

            _currentPath.Add(cell);
            Console.WriteLine($"[CTD] Path now: [{string.Join(", ", _currentPath.Select(p => $"({p.r},{p.c})"))}]");
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
