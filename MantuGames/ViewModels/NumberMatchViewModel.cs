using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class NmCell : INotifyPropertyChanged
{
    private int _index;
    public int Index { get => _index; set { _index = value; OnPropertyChanged(); } }

    private int _row;
    public int Row { get => _row; set { _row = value; OnPropertyChanged(); } }

    private int _col;
    public int Col { get => _col; set { _col = value; OnPropertyChanged(); } }

    private int _value;
    public int Value { get => _value; set { _value = value; OnPropertyChanged(); } }

    private bool _isEliminated;
    public bool IsEliminated { get => _isEliminated; set { _isEliminated = value; OnPropertyChanged(); OnPropertyChanged(nameof(CellBg)); OnPropertyChanged(nameof(TextVisibility)); OnPropertyChanged(nameof(ValueTextColor)); } }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); OnPropertyChanged(nameof(CellBg)); } }

    public string CellBg => IsEliminated ? "#1A2940" : IsSelected ? "#F97316" : "#1E293B";
    public bool TextVisibility => true;
    public string ValueTextColor => IsEliminated ? "#3A5070" : "#E2E8F0";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class NumberMatchViewModel : INotifyPropertyChanged
{
    private const int Columns = 9;
    private readonly List<int> _numbers = new();
    private int _selectedIdx = -1;

    public ObservableCollection<NmCell> Cells { get; } = new();
    public ICommand CellTappedCommand { get; }

    private string _levelDisplay = "Level 1";
    public string LevelDisplay { get => _levelDisplay; set { _levelDisplay = value; OnPropertyChanged(); } }

    private string _difficulty = "EASY";
    public string Difficulty { get => _difficulty; set { _difficulty = value; OnPropertyChanged(); } }

    private bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; set { _isGameOver = value; OnPropertyChanged(); } }

    private int _score;
    public int Score { get => _score; set { _score = value; OnPropertyChanged(); } }

    private int _moves;
    public int Moves { get => _moves; set { _moves = value; OnPropertyChanged(); } }

    private int _remainingAdds = 3;
    public int RemainingAdds { get => _remainingAdds; set { _remainingAdds = value; OnPropertyChanged(); OnPropertyChanged(nameof(AddButtonText)); OnPropertyChanged(nameof(CanAddNumbers)); OnPropertyChanged(nameof(AddButtonColor)); } }

    public string AddButtonText => RemainingAdds > 0 ? $"➕ ADD ROWS ({RemainingAdds})" : "NO ADDS LEFT";
    public bool CanAddNumbers => RemainingAdds > 0 && !IsGameOver;
    public string AddButtonColor => CanAddNumbers ? "#F97316" : "#475569";

    public int CurrentLevel { get; private set; }

    public event Action BoardChanged;
    public event Action<bool> GameEnded;

    public NumberMatchViewModel(int level = 1)
    {
        CellTappedCommand = new Command<int>(OnCellTapped);
        CurrentLevel = level;
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        CurrentLevel = level;
        _selectedIdx = -1;
        _numbers.Clear();
        Cells.Clear();
        Score = 0;
        Moves = 0;
        IsGameOver = false;
        RemainingAdds = 3;

        int initialCount = level switch
        {
            <= 5 => 36,
            <= 15 => 45,
            _ => 54
        };

        GenerateSolvableBoard(initialCount);

        LevelDisplay = $"Level {level}";
        Difficulty = level switch
        {
            <= 5 => "EASY",
            <= 15 => "MEDIUM",
            _ => "HARD"
        };

        BuildCells();
        BoardChanged?.Invoke();
    }

    private void GenerateSolvableBoard(int count)
    {
        var rng = Random.Shared;
        _numbers.Clear();

        // Must be even for all cells to be pairable
        if (count % 2 != 0) count--;

        int rows = count / Columns;

        // Build list of all possible adjacent pairs (right, down, down-right, down-left)
        var allPairs = new List<(int a, int b)>();
        for (int i = 0; i < count; i++)
        {
            int r = i / Columns, c = i % Columns;
            if (c < Columns - 1) allPairs.Add((i, i + 1));           // right
            if (r < rows - 1) allPairs.Add((i, i + Columns));        // down
            if (r < rows - 1 && c < Columns - 1) allPairs.Add((i, i + Columns + 1)); // down-right
            if (r < rows - 1 && c > 0) allPairs.Add((i, i + Columns - 1));           // down-left
        }

        // Shuffle all pairs
        for (int i = allPairs.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (allPairs[i], allPairs[j]) = (allPairs[j], allPairs[i]);
        }

        // Greedily place pairs — each pair is guaranteed adjacent
        var used = new bool[count];
        var result = new int[count];
        int pairsPlaced = 0;

        foreach (var (a, b) in allPairs)
        {
            if (used[a] || used[b]) continue;
            used[a] = true;
            used[b] = true;
            pairsPlaced++;

            // 50% identical pair, 50% sum-to-10 pair
            if (rng.Next(2) == 0)
            {
                int val = rng.Next(1, 10);
                result[a] = val;
                result[b] = val;
            }
            else
            {
                // Sum-to-10: pick one half (1-4), partner gets the complement
                int val = rng.Next(1, 5);  // 1,2,3,4
                int comp = 10 - val;       // 9,8,7,6
                if (rng.Next(2) == 0)
                {
                    result[a] = val;
                    result[b] = comp;
                }
                else
                {
                    result[a] = comp;
                    result[b] = val;
                }
            }

            if (pairsPlaced >= count / 2) break;
        }

        // Fill any unpaired cells with random (safety fallback)
        for (int i = 0; i < count; i++)
        {
            if (!used[i])
                result[i] = rng.Next(1, 10);
        }

        _numbers.AddRange(result);
    }

    private void BuildCells()
    {
        Cells.Clear();
        for (int i = 0; i < _numbers.Count; i++)
        {
            int r = i / Columns;
            int c = i % Columns;
            Cells.Add(new NmCell { Index = i, Row = r, Col = c, Value = _numbers[i] });
        }
    }

    public void OnCellTapped(int index)
    {
        if (IsGameOver || index < 0 || index >= Cells.Count) return;
        if (Cells[index].IsEliminated) return;

        AudioService.Instance.Play("tap");

        if (_selectedIdx == -1)
        {
            _selectedIdx = index;
            Cells[index].IsSelected = true;
            BoardChanged?.Invoke();
            return;
        }

        if (_selectedIdx == index)
        {
            Cells[index].IsSelected = false;
            _selectedIdx = -1;
            BoardChanged?.Invoke();
            return;
        }

        int a = _selectedIdx;
        int b = index;
        int valA = _numbers[a];
        int valB = _numbers[b];

        Cells[a].IsSelected = false;

        bool sameValue = valA == valB;
        bool sumsToTen = valA + valB == 10;

        if ((sameValue || sumsToTen) && AreAdjacent(a, b))
        {
            Cells[a].IsEliminated = true;
            Cells[b].IsEliminated = true;
            Score += sameValue ? valA * 2 : 10;
            Moves++;
            AudioService.Instance.Play("pop");
            CompactGrid();
            CheckWin();
        }

        _selectedIdx = -1;
        BoardChanged?.Invoke();
    }

    private bool IsCellEliminated(int idx)
    {
        return idx >= 0 && idx < Cells.Count && Cells[idx].IsEliminated;
    }

    private bool AreAdjacent(int idxA, int idxB)
    {
        int rA = idxA / Columns, cA = idxA % Columns;
        int rB = idxB / Columns, cB = idxB % Columns;
        int dr = rB - rA;
        int dc = cB - cA;
        int adr = Math.Abs(dr);
        int adc = Math.Abs(dc);

        if (adr <= 1 && adc <= 1) return true;

        if (rA == rB)
        {
            int minC = Math.Min(cA, cB);
            int maxC = Math.Max(cA, cB);
            for (int c = minC + 1; c < maxC; c++)
            {
                if (!IsCellEliminated(rA * Columns + c))
                    return false;
            }
            return true;
        }

        if (cA == cB)
        {
            int minR = Math.Min(rA, rB);
            int maxR = Math.Max(rA, rB);
            for (int r = minR + 1; r < maxR; r++)
            {
                if (!IsCellEliminated(r * Columns + cA))
                    return false;
            }
            return true;
        }

        if (adr == adc)
        {
            int rStep = dr > 0 ? 1 : -1;
            int cStep = dc > 0 ? 1 : -1;
            for (int k = 1; k < adr; k++)
            {
                int r = rA + k * rStep;
                int c = cA + k * cStep;
                if (r < 0 || r >= Cells.Count / Columns || c < 0 || c >= Columns)
                    return false;
                if (!IsCellEliminated(r * Columns + c))
                    return false;
            }
            return true;
        }

        if (rB == rA + 1 && IsLastVisibleInRow(rA, cA) && IsFirstVisibleInRow(rB, cB)) return true;
        if (rA == rB + 1 && IsLastVisibleInRow(rB, cB) && IsFirstVisibleInRow(rA, cA)) return true;

        if (rB > rA + 1 && AllRowsClearedBetween(rA + 1, rB))
        {
            if (IsLastVisibleInRow(rA, cA) && IsFirstVisibleInRow(rB, cB)) return true;
        }
        if (rA > rB + 1 && AllRowsClearedBetween(rB + 1, rA))
        {
            if (IsLastVisibleInRow(rB, cB) && IsFirstVisibleInRow(rA, cA)) return true;
        }

        return false;
    }

    private bool IsLastVisibleInRow(int row, int col)
    {
        for (int c = Columns - 1; c >= 0; c--)
        {
            int idx = row * Columns + c;
            if (idx < Cells.Count && !Cells[idx].IsEliminated)
                return c == col;
        }
        return false;
    }

    private bool IsFirstVisibleInRow(int row, int col)
    {
        for (int c = 0; c < Columns; c++)
        {
            int idx = row * Columns + c;
            if (idx < Cells.Count && !Cells[idx].IsEliminated)
                return c == col;
        }
        return false;
    }

    private bool AllRowsClearedBetween(int fromRow, int toRow)
    {
        for (int r = fromRow; r < toRow; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                int idx = r * Columns + c;
                if (idx < Cells.Count && !Cells[idx].IsEliminated)
                    return false;
            }
        }
        return true;
    }

    private void CompactGrid()
    {
        int totalRows = Cells.Count / Columns;
        bool removedRow = false;

        for (int r = totalRows - 1; r >= 0; r--)
        {
            bool allEliminated = true;
            for (int c = 0; c < Columns; c++)
            {
                int idx = r * Columns + c;
                if (idx < Cells.Count && !Cells[idx].IsEliminated)
                {
                    allEliminated = false;
                    break;
                }
            }

            if (allEliminated)
            {
                for (int c = Columns - 1; c >= 0; c--)
                {
                    int idx = r * Columns + c;
                    if (idx < _numbers.Count && idx < Cells.Count)
                    {
                        _numbers.RemoveAt(idx);
                        Cells.RemoveAt(idx);
                        removedRow = true;
                    }
                }
                Score += 20;
            }
        }

        if (removedRow)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Index = i;
                Cells[i].Row = i / Columns;
                Cells[i].Col = i % Columns;
            }
        }
    }

    public void AddNumbers()
    {
        if (IsGameOver || RemainingAdds <= 0) return;

        var activeValues = new List<int>();
        for (int i = 0; i < Cells.Count; i++)
        {
            if (!Cells[i].IsEliminated)
                activeValues.Add(_numbers[i]);
        }

        if (activeValues.Count == 0) return;

        // Append new rows at the bottom with duplicates of active values
        int startIdx = _numbers.Count;
        foreach (int v in activeValues)
            _numbers.Add(v);

        // Only create new cells for the appended values
        for (int i = startIdx; i < _numbers.Count; i++)
        {
            int r = i / Columns;
            int c = i % Columns;
            Cells.Add(new NmCell { Index = i, Row = r, Col = c, Value = _numbers[i] });
        }

        RemainingAdds--;
        AudioService.Instance.Play("pop");
        BoardChanged?.Invoke();
    }

    private void CheckWin()
    {
        bool allEliminated = Cells.All(c => c.IsEliminated);
        if (allEliminated)
        {
            IsGameOver = true;
            GameEnded?.Invoke(true);
            return;
        }

        if (RemainingAdds <= 0)
            CheckNoMovesLeft();
    }

    private void CheckNoMovesLeft()
    {
        if (IsGameOver) return;

        var active = new List<int>();
        for (int i = 0; i < Cells.Count; i++)
        {
            if (!Cells[i].IsEliminated)
                active.Add(i);
        }

        for (int i = 0; i < active.Count; i++)
        {
            for (int j = i + 1; j < active.Count; j++)
            {
                int a = active[i], b = active[j];
                int valA = _numbers[a], valB = _numbers[b];
                if ((valA == valB || valA + valB == 10) && AreAdjacent(a, b))
                    return;
            }
        }

        IsGameOver = true;
        GameEnded?.Invoke(false);
    }

    public void Restart() => StartLevel(CurrentLevel);

    public void Cleanup()
    {
        BoardChanged = null;
        GameEnded = null;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
