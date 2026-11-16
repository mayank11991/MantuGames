using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class DailyRulesTests
{
    [Fact]
    public void GameIndex_VisitsEveryGameOnceInElevenDays()
    {
        var start = new DateOnly(2026, 11, 16);
        var seen = Enumerable.Range(0, 11).Select(i => DailyRules.GameIndexFor(start.AddDays(i), 11)).ToList();
        Assert.Equal(11, seen.Distinct().Count());
    }

    [Theory]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(5)]
    public void GameIndex_NeverRepeatsOnConsecutiveDays(int count)
    {
        var day = new DateOnly(2026, 1, 1);
        for (int i = 0; i < 400; i++, day = day.AddDays(1))
        {
            int a = DailyRules.GameIndexFor(day, count);
            Assert.InRange(a, 0, count - 1);
            Assert.NotEqual(a, DailyRules.GameIndexFor(day.AddDays(1), count));
        }
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(2, 25)]
    [InlineData(6, 45)]
    [InlineData(7, 100)]   // 50 capped streak bonus + week bonus
    [InlineData(8, 50)]
    [InlineData(14, 100)]
    public void RewardFor_GrowsWithStreakAndPaysWeekBonus(int streak, int coins) =>
        Assert.Equal(coins, DailyRules.RewardFor(streak));

    [Fact]
    public void NextStreak_ContinuesFromYesterdayOtherwiseRestarts()
    {
        var today = new DateOnly(2026, 11, 16);
        Assert.Equal(4, DailyRules.NextStreak(today.AddDays(-1), 3, today));
        Assert.Equal(1, DailyRules.NextStreak(today.AddDays(-2), 3, today));
        Assert.Equal(1, DailyRules.NextStreak(null, 0, today));
    }
}
