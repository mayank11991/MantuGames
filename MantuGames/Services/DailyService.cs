using MantuGames.Models;
using MantuGames.ViewModels;

namespace MantuGames.Services;

/// <summary>Coins and streak earned by finishing today's daily challenge.</summary>
public sealed record DailyReward(int Coins, int Streak);

/// <summary>
/// One daily challenge per profile: win any level of today's featured game.
/// Finishing it on consecutive days builds a streak, and longer streaks pay more.
/// Everything is local to the device; the date is the device's local date.
/// </summary>
public static class DailyService
{
    /// <summary>Raised after today's challenge is completed.</summary>
    public static event EventHandler? Changed;

    private static string LastDoneKey => ProfileService.Key("daily_last_done");
    private static string StreakKey => ProfileService.Key("daily_streak");
    private static string BestKey => ProfileService.Key("daily_best_streak");
    private static string LevelDayKey => ProfileService.Key("daily_level_day");
    private static string LevelKey => ProfileService.Key("daily_level");

    /// <summary>
    /// Today's challenge level in <see cref="TodayGame"/>. Picked once per day from the
    /// levels already unlocked and remembered, so it doesn't move when more unlock.
    /// </summary>
    public static int TodayLevel
    {
        get
        {
            string today = Today.ToString("yyyy-MM-dd");
            if (Preferences.Default.Get(LevelDayKey, "") == today)
                return Math.Max(1, Preferences.Default.Get(LevelKey, 1));
            int level = DailyRules.LevelFor(Today, ProgressService.Instance.GetHighestUnlocked(TodayGame.Id));
            Preferences.Default.Set(LevelDayKey, today);
            Preferences.Default.Set(LevelKey, level);
            return level;
        }
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>Today's featured game. Steps through every game before repeating, never the same two days running.</summary>
    public static GameInfo TodayGame => GameFor(Today);

    public static GameInfo GameFor(DateOnly day)
    {
        var games = DashboardViewModel.Games;
        return games[DailyRules.GameIndexFor(day, games.Count)];
    }

    public static bool IsDoneToday => LastDone == Today;

    /// <summary>Streak still alive today: completed today or yesterday.</summary>
    public static int CurrentStreak
    {
        get
        {
            var last = LastDone;
            if (last == null) return 0;
            if (last == Today || last == Today.AddDays(-1))
                return Preferences.Default.Get(StreakKey, 0);
            return 0;
        }
    }

    public static int BestStreak => Preferences.Default.Get(BestKey, 0);

    /// <summary>Coins today's challenge pays if finished now.</summary>
    public static int RewardForToday => RewardFor(IsDoneToday ? CurrentStreak : CurrentStreak + 1);

    public static int RewardFor(int streak) => DailyRules.RewardFor(streak);

    /// <summary>
    /// Call after a solved (non-assisted) level. Completes today's challenge only for
    /// today's game and today's level; returns the reward, or null if nothing changed.
    /// </summary>
    public static DailyReward? OnLevelWon(string? gameId, int level)
    {
        if (string.IsNullOrEmpty(gameId) || ProfileService.Active == null) return null;
        if (IsDoneToday || gameId != TodayGame.Id || level != TodayLevel) return null;

        int streak = DailyRules.NextStreak(LastDone, Preferences.Default.Get(StreakKey, 0), Today);
        int coins = RewardFor(streak);

        Preferences.Default.Set(LastDoneKey, Today.ToString("yyyy-MM-dd"));
        Preferences.Default.Set(StreakKey, streak);
        if (streak > BestStreak) Preferences.Default.Set(BestKey, streak);

        CoinService.AddCoins(coins);
        Changed?.Invoke(null, EventArgs.Empty);
        return new DailyReward(coins, streak);
    }

    private static DateOnly? LastDone =>
        DateOnly.TryParseExact(Preferences.Default.Get(LastDoneKey, ""), "yyyy-MM-dd", out var d) ? d : null;
}
