using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class ArrowLinesViewModel : INotifyPropertyChanged
{
    private ArrowLinesPuzzle _puzzle;
    private int _currentLevel;
    private int _moves;
    private int _arrowsCleared;
    private bool _isGameOver;
    private bool _isPaused;
    private string _statusText = "Tap arrows to clear the board!";
    private int _lives = 3;

    public int CurrentLevel
    {
        get => _currentLevel;
        set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public int Moves
    {
        get => _moves;
        set { _moves = value; OnPropertyChanged(); }
    }

    public int ArrowsCleared
    {
        get => _arrowsCleared;
        set { _arrowsCleared = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingText)); }
    }

    public int TotalArrows => _puzzle?.Arrows.Count ?? 0;

    public string RemainingText => _puzzle == null ? "" : $"{_puzzle.RemainingCount} arrows left";

    public bool IsGameOver
    {
        get => _isGameOver;
        set { _isGameOver = value; OnPropertyChanged(); }
    }

    public bool IsPaused
    {
        get => _isPaused;
        set { _isPaused = value; OnPropertyChanged(); }
    }

    public int Lives
    {
        get => _lives;
        set { _lives = value; OnPropertyChanged(); OnPropertyChanged(nameof(LivesDisplay)); }
    }

    public string LivesDisplay => new string('\u2764', _lives);

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public List<ArrowCell> Arrows => _puzzle?.Arrows ?? new();

    public int GridRows => _puzzle?.Rows ?? 5;
    public int GridCols => _puzzle?.Cols ?? 5;

    public ICommand ArrowTappedCommand { get; }

    public event Action<bool> GameEnded;
    public event Action<ArrowCell, List<(int Row, int Col)>> MoveAnimated;
    public event Action BoardChanged;
    public event Action<ArrowCell> ArrowBlocked;

    public ArrowLinesViewModel(int level = 1)
    {
        ArrowTappedCommand = new Command<ArrowCell>(OnArrowTapped);
        CurrentLevel = level;
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        CurrentLevel = level;
        _puzzle = ArrowLinesPuzzle.Generate(level);
        Moves = 0;
        ArrowsCleared = 0;
        Lives = 3;
        IsGameOver = false;
        IsPaused = false;
        StatusText = "Tap arrows to clear the board!";

        OnPropertyChanged(nameof(Arrows));
        OnPropertyChanged(nameof(TotalArrows));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(GridRows));
        OnPropertyChanged(nameof(GridCols));
        BoardChanged?.Invoke();
    }

    private void OnArrowTapped(ArrowCell arrow)
    {
        if (IsGameOver || arrow == null || arrow.IsCleared) return;

        var path = _puzzle.SimulateMove(arrow);

        if (arrow.IsCleared)
        {
            Moves++;
            ArrowsCleared++;
            StatusText = $"Cleared! {_puzzle.RemainingCount} remaining";

            // Fire animation BEFORE board refresh removes the view
            MoveAnimated?.Invoke(arrow, path);
            BoardChanged?.Invoke();

            if (_puzzle.AllCleared)
            {
                IsGameOver = true;
                StatusText = "All arrows cleared!";
                GameEnded?.Invoke(true);
            }
        }
        else
        {
            Moves++;
            Lives--;
            StatusText = Lives > 0 ? $"Blocked! {Lives} lives left" : "No lives left!";
            ArrowBlocked?.Invoke(arrow);
            BoardChanged?.Invoke();

            if (Lives <= 0)
            {
                IsGameOver = true;
                StatusText = "Game Over!";
                GameEnded?.Invoke(false);
            }
        }
    }

    public void Cleanup()
    {
        GameEnded = null;
        MoveAnimated = null;
        BoardChanged = null;
        ArrowBlocked = null;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
