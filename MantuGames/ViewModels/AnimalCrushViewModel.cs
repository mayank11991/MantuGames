using System.ComponentModel;
using System.Runtime.CompilerServices;
using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class AnimalCrushViewModel : INotifyPropertyChanged
{
    private System.Threading.Timer _timer;
    private bool _isGameOver;
    private bool _isBusy;
    private int  _timeRemainingSec;
    private int  _totalSecondsForLevel;
    private int  _currentLevel;
    private int  _score;
    private int  _gained;
    private int  _hammerCount;
    private int  _rocketCount;
    private int  _bombCount;

    public AnimalCrushPuzzle Puzzle { get; private set; }

    public event Action<bool> GameEnded;

    public int CurrentLevel
    {
        get => _currentLevel;
        private set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
    }

    public string LevelDisplay => $"Level {CurrentLevel}";

    public int TimeRemainingSec
    {
        get => _timeRemainingSec;
        set { _timeRemainingSec = value; OnPropertyChanged(); }
    }

    public int TotalSecondsForLevel
    {
        get => _totalSecondsForLevel;
        private set { _totalSecondsForLevel = value; OnPropertyChanged(); }
    }

    public int Score
    {
        get => _score;
        private set { _score = value; OnPropertyChanged(); OnPropertyChanged(nameof(ScoreDisplay)); }
    }

    public int TargetScore => Puzzle?.TargetScore ?? 0;
    public string ScoreDisplay => $"{Score}/{TargetScore}";
    public string TargetText => $"Target: {TargetScore} pts";

    public int HammerCount
    {
        get => _hammerCount;
        private set { _hammerCount = value; OnPropertyChanged(); }
    }

    public int RocketCount
    {
        get => _rocketCount;
        private set { _rocketCount = value; OnPropertyChanged(); }
    }

    public int BombCount
    {
        get => _bombCount;
        private set { _bombCount = value; OnPropertyChanged(); }
    }

    public static int HammerCost = 10;
    public static int RocketCost = 20;
    public static int BombCost  = 30;

    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); }
    }

    public AnimalCrushViewModel(int level = 1)
    {
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        StopTimer();
        IsGameOver = false;
        IsBusy     = false;
        Score      = 0;
        CurrentLevel = level;
        HammerCount  = 1;
        RocketCount  = 1;
        BombCount    = 1;

        Puzzle = AnimalCrushPuzzle.Generate(level, TimerSecForLevel(level), TargetForLevel(level));
        TotalSecondsForLevel = Puzzle.TimerSec;
        TimeRemainingSec     = Puzzle.TimerSec;

        StartTimer();
    }

    private static int TimerSecForLevel(int level) =>
        level <= 3 ? 180 : level <= 7 ? 240 : 300;

    private static int TargetForLevel(int level) =>
        Math.Min(6000, 1500 + 250 * (level - 1));

    /// <summary>
    /// Swipes a tile in direction (dr, dc) and swaps it with its neighbour.
    /// </summary>
    public async Task<bool> TrySwipeAsync(int r, int c, int dr, int dc)
    {
        int r2 = r + dr, c2 = c + dc;
        if (r2 < 0 || c2 < 0 || r2 >= AnimalCrushPuzzle.Rows || c2 >= AnimalCrushPuzzle.Cols)
            return false;
        return await TrySwapAsync(r, c, r2, c2);
    }

    /// <summary>
    /// Attempts a swap. Returns true when the swap was valid and scored.
    /// </summary>
    public async Task<bool> TrySwapAsync(int r1, int c1, int r2, int c2)
    {
        if (IsGameOver || IsBusy) return false;
        if (Puzzle == null) return false;

        IsBusy = true;
        try
        {
            bool ok = await Task.Run(() =>
            {
                bool valid = Puzzle.TrySwap(r1, c1, r2, c2, out int g);
                _gained = g;
                return valid;
            });
            if (!ok) return false;

            Score += _gained;
            EnsureMovesExist();

            if (Score >= TargetScore)
            {
                EndGame(true);
                return true;
            }

            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Uses a power-up on a tile. Returns -1 when the power-up is not owned,
    /// otherwise the score gained (0 is possible).
    /// </summary>
    public async Task<int> UsePowerUpAsync(AnimalCrushPuzzle.PowerUpKind kind, int r, int c)
    {
        if (IsGameOver || IsBusy || Puzzle == null) return 0;
        if (CountOf(kind) <= 0) return -1;

        IsBusy = true;
        try
        {
            int gained = await Task.Run(() => Puzzle.UsePowerUp(kind, r, c));

            switch (kind)
            {
                case AnimalCrushPuzzle.PowerUpKind.Hammer: HammerCount--; break;
                case AnimalCrushPuzzle.PowerUpKind.Rocket: RocketCount--; break;
                case AnimalCrushPuzzle.PowerUpKind.Bomb:   BombCount--;   break;
            }

            Score += gained;
            EnsureMovesExist();

            if (Score >= TargetScore)
            {
                EndGame(true);
                return gained;
            }

            return gained;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Spends coins to buy one more power-up. Returns false when unaffordable.</summary>
    public bool BuyPowerUp(AnimalCrushPuzzle.PowerUpKind kind)
    {
        int cost = kind switch
        {
            AnimalCrushPuzzle.PowerUpKind.Hammer => HammerCost,
            AnimalCrushPuzzle.PowerUpKind.Rocket => RocketCost,
            _ => BombCost,
        };

        if (CoinService.GetCoins("animalcrush") < cost) return false;
        CoinService.SpendCoins("animalcrush", cost);

        switch (kind)
        {
            case AnimalCrushPuzzle.PowerUpKind.Hammer: HammerCount++; break;
            case AnimalCrushPuzzle.PowerUpKind.Rocket: RocketCount++; break;
            case AnimalCrushPuzzle.PowerUpKind.Bomb:   BombCount++;   break;
        }
        return true;
    }

    public int CountOf(AnimalCrushPuzzle.PowerUpKind kind) =>
        kind switch
        {
            AnimalCrushPuzzle.PowerUpKind.Hammer => HammerCount,
            AnimalCrushPuzzle.PowerUpKind.Rocket => RocketCount,
            _ => BombCount,
        };

    private void EnsureMovesExist()
    {
        if (AnimalCrushPuzzle.FindAllMatches(Puzzle.Board).Count == 0 &&
            !Puzzle.HasPossibleMove())
        {
            Puzzle.Reshuffle();
        }
    }

    /// <summary>Finds a swap that creates a match, or null.</summary>
    public (int r1, int c1, int r2, int c2)? FindHint() => Puzzle?.FindHint();

    private void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsGameOver) return;
                if (TimeRemainingSec > 0)
                {
                    TimeRemainingSec--;
                }
                else
                {
                    EndGame(false);
                }
            });
        }, null, 1000, 1000);
    }

    private void StopTimer()
    {
        try { _timer?.Dispose(); } catch { }
        _timer = null;
    }

    public void PauseTimer() => StopTimer();
    public void ResumeTimer() => StartTimer();
    public void Cleanup() => StopTimer();

    private void EndGame(bool win)
    {
        if (IsGameOver) return;
        StopTimer();
        IsGameOver = true;
        GameEnded?.Invoke(win);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}