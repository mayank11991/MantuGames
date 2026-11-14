using System.ComponentModel;
using System.Runtime.CompilerServices;
using MantuGames.Models;

namespace MantuGames.ViewModels;

/// <summary>
/// Arrow Lines: tap a line and it slides out the way its arrow points. If another
/// line is in the way it bumps, bounces back and costs a life. Clear every line to win.
/// </summary>
public class ArrowLinesViewModel : INotifyPropertyChanged
{
    private ArrowLinesPuzzle _puzzle = null!;
    private int _currentLevel;
    private int _moves;
    private int _cleared;
    private int _lives = 3;
    private bool _isGameOver;

    public bool IsShowSolutionActive { get; set; }

    public int CurrentLevel
    {
        get => _currentLevel;
        private set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public int Moves
    {
        get => _moves;
        private set { _moves = value; OnPropertyChanged(); }
    }

    /// <summary>Lines cleared so far (bound as "CLEARED" in the header).</summary>
    public int ArrowsCleared
    {
        get => _cleared;
        private set { _cleared = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingText)); }
    }

    public int Lives
    {
        get => _lives;
        private set { _lives = value; OnPropertyChanged(); OnPropertyChanged(nameof(LivesDisplay)); }
    }

    public string LivesDisplay => new string('❤', Math.Max(0, _lives));

    public string RemainingText => _puzzle == null ? "" :
        _puzzle.RemainingCount == 1 ? "1 line left" : $"{_puzzle.RemainingCount} lines left";

    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    public ArrowLinesPuzzle Puzzle => _puzzle;
    public int TotalLines => _puzzle.Lines.Count;

    /// <summary>Line escaped: animate it out along its path.</summary>
    public event Action<ArrowLine>? LineEscaped;
    /// <summary>Line bumped into another after travelling <c>run</c> free cells.</summary>
    public event Action<ArrowLine, int>? LineBlocked;
    public event Action<bool>? GameEnded;

    public ArrowLinesViewModel(int level = 1) => StartLevel(level);

    public void StartLevel(int level)
    {
        CurrentLevel = level;
        _puzzle = ArrowLinesPuzzle.Generate(level);
        Moves = 0;
        ArrowsCleared = 0;
        Lives = 3;
        IsGameOver = false;
        OnPropertyChanged(nameof(Puzzle));
    }

    public void Tap(ArrowLine line)
    {
        if (IsGameOver || line.IsCleared) return;
        Moves++;

        int run = _puzzle.FreeRunBeforeBlock(line);
        if (run < 0)
        {
            _puzzle.Remove(line);
            ArrowsCleared++;
            LineEscaped?.Invoke(line);
            if (_puzzle.AllCleared)
            {
                IsGameOver = true;
                GameEnded?.Invoke(true);
            }
            return;
        }

        LineBlocked?.Invoke(line, run);
        if (IsShowSolutionActive) return;
        Lives--;
        if (Lives <= 0)
        {
            IsGameOver = true;
            GameEnded?.Invoke(false);
        }
    }

    /// <summary>Any line whose way out is clear (used by Show Solution).</summary>
    public ArrowLine? NextFreeLine() => _puzzle.Lines.FirstOrDefault(_puzzle.CanEscape);

    /// <summary>3 lives left → 3★, 2 → 2★, 1 → 1★.</summary>
    public int Stars => Math.Clamp(Lives, 1, 3);

    public void Cleanup()
    {
        LineEscaped = null;
        LineBlocked = null;
        GameEnded = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
