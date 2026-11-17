namespace MantuGames.Models;

/// <summary>Pure daily-challenge rules (no device state), shared by DailyService and the tests.</summary>
public static class DailyRules
{
    public const int BaseReward = 20;
    public const int StreakStep = 5;
    public const int MaxStreakBonus = 30;
    public const int WeekBonus = 50;

    /// <summary>
    /// Index of the featured game for a day. Stepping by 4 walks every game in a
    /// shuffled order when 4 is coprime with the count, and never repeats on consecutive days.
    /// </summary>
    public static int GameIndexFor(DateOnly day, int gameCount)
    {
        if (gameCount <= 0) throw new ArgumentOutOfRangeException(nameof(gameCount));
        int step = Gcd(4, gameCount) == 1 ? 4 : 1;
        return (int)((day.DayNumber * (long)step) % gameCount);
    }

    /// <summary>
    /// Today's challenge level: a fixed pick among the levels the player has already
    /// unlocked (1..<paramref name="highestUnlocked"/>), so it is always playable.
    /// </summary>
    public static int LevelFor(DateOnly day, int highestUnlocked)
    {
        int max = Math.Max(1, highestUnlocked);
        return 1 + (int)((day.DayNumber * 7L + 3) % max);
    }

    /// <summary>Coins for finishing the challenge on day <paramref name="streak"/> of a streak.</summary>
    public static int RewardFor(int streak)
    {
        int reward = BaseReward + Math.Min(Math.Max(streak - 1, 0) * StreakStep, MaxStreakBonus);
        if (streak > 0 && streak % 7 == 0) reward += WeekBonus;
        return reward;
    }

    /// <summary>Streak after finishing today, given the last day it was finished.</summary>
    public static int NextStreak(DateOnly? lastDone, int storedStreak, DateOnly today) =>
        lastDone == today.AddDays(-1) ? storedStreak + 1 : 1;

    private static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
}
