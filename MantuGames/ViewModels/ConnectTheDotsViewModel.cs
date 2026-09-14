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

        if (pairIdx >= 0)
        {
            if (_activePair == pairIdx)
            {
                if (_currentPath.Count >= 2)
                {
                    var p = _puzzle.Pairs[pairIdx];
                    var last = _currentPath[^1];
                    var first = _currentPath[0];
                    bool atEnd1 = (last.r == p.R1 && last.c == p.C1) && (first.r == p.R2 && first.c == p.C2);
                    bool atEnd2 = (last.r == p.R2 && last.c == p.C2) && (first.r == p.R1 && first.c == p.C1);

                    if (atEnd1 || atEnd2)
                    {
                        CompletePath(pairIdx);
                        return;
                    }
                }
                if (_currentPath.Count == 1 && _currentPath[0] == cell)
                {
                    _activePair = null;
                    _currentPath.Clear();
                    BoardChanged?.Invoke();
                    return;
                }
            }
            else
            {
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
            if (Math.Abs(last.r - cell.r) + Math.Abs(last.c - cell.c) != 1) return;

            if (IsCellOwnedByOther(cell, _activePair.Value)) return;
            if (_currentPath.Contains(cell)) return;

            _currentPath.Add(cell);
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
