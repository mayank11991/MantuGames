using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Services;

namespace MantuGames.Models;

public class HanoiViewModel : INotifyPropertyChanged
{
    private System.Threading.Timer _timer;
    private bool _isGameOver, _isWin;
    private int _timeRemainingSec = 210;
    private int _totalTimerSec = 210;
    private int _currentLevel;
    private int _moveCount;

    public HanoiPuzzle Puzzle { get; private set; }

    public bool SolutionWasShown { get; set; }

    public int MoveCount
    {
        get => _moveCount;
        set
        {
            _moveCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MovesDisplay));
        }
    }

    public string MovesDisplay => $"Moves: {MoveCount}  (min {Puzzle?.MinMoves})";

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        set
        {
            _timeRemainingSec = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(TimerProgress));
        }
    }

    public int TotalTimerSec
    {
        get => _totalTimerSec;
        set { _totalTimerSec = value; OnPropertyChanged(); }
    }

    public string TimerDisplay => $"{TimeRemainingSec / 60}:{TimeRemainingSec % 60:D2}";
    public double TimerProgress => TimeRemainingSec / (TotalTimerSec > 0 ? (double)TotalTimerSec : 1.0);

    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            OnPropertyChanged();
        }
    }

    public bool IsWin
    {
        get => _isWin;
        set
        {
            _isWin = value;
            OnPropertyChanged();
        }
    }

    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LevelDisplay));
        }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public ICommand ShowSolutionCommand { get; }

    public event Action<bool> GameEnded;
    public event Action BoardChanged;

    public HanoiViewModel(int level = 1)
    {
        ShowSolutionCommand = new Command(ShowSolution);
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        CurrentLevel = level;
        TotalTimerSec = ProgressService.GetTimerSeconds(level);
        TimeRemainingSec = TotalTimerSec;
        IsGameOver = false;
        IsWin = false;
        SolutionWasShown = false;
        MoveCount = 0;
        Puzzle = HanoiPuzzle.Generate(level);
        StopTimer();
        StartTimer();
    }

    // Called by page after a successful TryMove
    public void SyncMoves() => MoveCount = Puzzle.MoveCount;

    // Called by page when it detects IsSolved
    public void TriggerWin() => EndGame(true);

    // ── SHOW SOLUTION ─────────────────────────────────────────────
    private void ShowSolution()
    {
        if (IsGameOver) return;
        StopTimer();
        IsGameOver = true;
        SolutionWasShown = true;
        // Discs start scattered across all poles, so solve the actual state:
        // HanoiSolver computes the exact move sequence.
        var moves = HanoiSolver.Solve(Puzzle);
        _ = AnimateSolutionAsync(moves);
    }

    private async System.Threading.Tasks.Task AnimateSolutionAsync(List<(int From, int To)> moves)
    {
        foreach (var (from, to) in moves)
        {
            Puzzle.TryMove(from, to);
            MoveCount = Puzzle.MoveCount;
            MainThread.BeginInvokeOnMainThread(() => BoardChanged?.Invoke());
            await System.Threading.Tasks.Task.Delay(500);
        }

        MainThread.BeginInvokeOnMainThread(() => GameEnded?.Invoke(false));
    }

    private void EndGame(bool win)
    {
        StopTimer();
        IsWin = win;
        IsGameOver = true;
        GameEnded?.Invoke(win);
    }

    private void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (TimeRemainingSec > 0) TimeRemainingSec--;
                else EndGame(false);
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}