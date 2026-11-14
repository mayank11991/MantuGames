using MantuGames.Models;

namespace MantuGames.Services;

public class ProgressService
{
    private static ProgressService? _instance;
    public static ProgressService Instance => _instance ??= new ProgressService();
    private ProgressService() { }

    // ── TIMER DURATION ─────────────────────────────────────────────
    // Easy (levels 1-3): 80s   Medium (4-7): 120s   Hard (8+): 150s
    // Sudoku, Word Search and Math Challenge run a fixed 3:30 clock.
    public const int ClassicTimerSeconds = 210;

    public static int GetTimerSeconds(int level) =>
        level <= 3 ? 80 : level <= 7 ? 120 : 150;

    // ── STAR CALCULATION ───────────────────────────────────────────
    // Based on fraction of total time used: <40% → 3★, <70% → 2★, else 1★
    public static int CalcStars(int elapsedSec, int totalSec)
    {
        if (totalSec <= 0) return 1;
        double pct = (double)elapsedSec / totalSec;
        return pct < 0.40 ? 3 : pct < 0.70 ? 2 : 1;
    }

    // ── LEVEL PROGRESS ─────────────────────────────────────────────
    public List<LevelProgress> GetLevels(string gameId, int visibleCount)
    {
        var list = new List<LevelProgress>();
        for (int i = 1; i <= visibleCount; i++)
        {
            var state = (LevelState)Preferences.Get(Key(gameId, i), (int)LevelState.Locked);
            if (i == 1 && state == LevelState.Locked) state = LevelState.Unlocked;
            int stars = GetStars(gameId, i);
            list.Add(new LevelProgress { LevelNumber = i, State = state, Stars = stars });
        }
        return list;
    }

    /// <summary>Coins for a star rating: 1★ → 1, 2★ → 3, 3★ → 5.</summary>
    public static int CoinReward(int stars) => stars switch { >= 3 => 5, 2 => 3, 1 => 1, _ => 0 };

    /// <summary>
    /// Marks a level completed and unlocks the next one. Coins are paid only for
    /// stars above the player's previous best on this level, so replaying a solved
    /// level cannot farm coins. Returns the coins actually awarded.
    /// </summary>
    public int CompleteLevel(string gameId, int levelNumber, int stars = 0)
    {
        SetState(gameId, levelNumber,     LevelState.Completed);
        SetState(gameId, levelNumber + 1, LevelState.Unlocked);

        int existing = GetStars(gameId, levelNumber);
        if (stars <= existing) return 0;

        Preferences.Set(StarsKey(gameId, levelNumber), stars);
        int awarded = CoinReward(stars) - CoinReward(existing);
        CoinService.AddCoins(gameId, awarded);
        return awarded;
    }

    // ── ASSISTED ATTEMPTS ──────────────────────────────────────────
    // A paid "Show Solution" makes the current attempt assisted: the level still
    // unlocks the next one, but earns no stars or coins.
    private static readonly HashSet<string> _assisted = new();

    public static void MarkAssisted(string gameId)
    {
        lock (_assisted) _assisted.Add(gameId);
    }

    /// <summary>Returns whether the current attempt was assisted and resets the flag.</summary>
    public static bool TakeAssisted(string gameId)
    {
        lock (_assisted) return _assisted.Remove(gameId);
    }

    public static void ClearAssisted(string gameId)
    {
        lock (_assisted) _assisted.Remove(gameId);
    }

    public int GetStars(string gameId, int level) =>
        Preferences.Get(StarsKey(gameId, level), 0);

    public int GetHighestUnlocked(string gameId)
    {
        int highest = 1;
        for (int i = 1; i <= 50; i++)
        {
            var state = (LevelState)Preferences.Get(Key(gameId, i), (int)LevelState.Locked);
            if (i == 1 && state == LevelState.Locked) state = LevelState.Unlocked;
            if (state == LevelState.Locked) break;
            highest = i;
        }
        return highest;
    }

    public void ResetGame(string gameId)
    {
        for (int i = 1; i <= 200; i++)
        {
            Preferences.Remove(Key(gameId, i));
            Preferences.Remove(StarsKey(gameId, i));
        }
    }

    private void SetState(string gameId, int level, LevelState state)
    {
        var current = (LevelState)Preferences.Get(Key(gameId, level), (int)LevelState.Locked);
        if (current == LevelState.Completed && state == LevelState.Unlocked) return;
        Preferences.Set(Key(gameId, level), (int)state);
    }

    private static string Key(string gameId, int level)      => ProfileService.Key($"{gameId}_level_{level}_state");
    private static string StarsKey(string gameId, int level) => ProfileService.Key($"{gameId}_level_{level}_stars");
}
