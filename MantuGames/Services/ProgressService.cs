using MantuGames.Models;

namespace MantuGames.Services;

public class ProgressService
{
    private static ProgressService _instance;
    public static ProgressService Instance => _instance ??= new ProgressService();
    private ProgressService() { }

    // ── TIMER DURATION ─────────────────────────────────────────────
    // Easy (levels 1-3): 80s   Medium (4-7): 120s   Hard (8+): 150s
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

    public void CompleteLevel(string gameId, int levelNumber, int stars = 0)
    {
        SetState(gameId, levelNumber,     LevelState.Completed);
        SetState(gameId, levelNumber + 1, LevelState.Unlocked);

        // Keep best stars
        int existing = GetStars(gameId, levelNumber);
        if (stars > existing)
            Preferences.Set(StarsKey(gameId, levelNumber), stars);

        // Award coins based on stars earned
        if (stars > 0)
        {
            int coinReward = stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 };
            CoinService.AddCoins(gameId, coinReward);
        }
    }

    public int GetStars(string gameId, int level) =>
        Preferences.Get(StarsKey(gameId, level), 0);

    public int GetHighestUnlocked(string gameId)
    {
        int highest = 1;
        for (int i = 1; i <= 200; i++)
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
            Preferences.Remove(Key(gameId, i));
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
