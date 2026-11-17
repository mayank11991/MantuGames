using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class BadgeTests
{
    private static PlayerSnapshot Snap(int wins = 0, int levels = 0, int streak = 0, Dictionary<string, int>? byGame = null) =>
        new(wins, levels, streak, byGame ?? new Dictionary<string, int>());

    [Fact]
    public void BadgeIds_AreUnique() =>
        Assert.Equal(BadgeCatalog.All.Count, BadgeCatalog.All.Select(b => b.Id).Distinct().Count());

    [Fact]
    public void NewPlayer_HasNoBadges() =>
        Assert.Empty(BadgeCatalog.NewlyEarned(Snap(), new HashSet<string>()));

    [Fact]
    public void FirstWin_EarnsFirstWinOnly()
    {
        var earned = BadgeCatalog.NewlyEarned(Snap(wins: 1, levels: 1, byGame: new() { ["sudoku"] = 1 }), new HashSet<string>());
        Assert.Equal(new[] { "first_win" }, earned.Select(b => b.Id));
    }

    [Fact]
    public void AlreadyEarnedBadges_AreNotReturnedAgain()
    {
        var snap = Snap(wins: 12, levels: 12);
        var first = BadgeCatalog.NewlyEarned(snap, new HashSet<string>());
        Assert.Contains(first, b => b.Id == "wins_10");
        Assert.Empty(BadgeCatalog.NewlyEarned(snap, first.Select(b => b.Id).ToHashSet()));
    }

    [Fact]
    public void Explorer_NeedsWinsInFiveDifferentGames()
    {
        var four = new Dictionary<string, int> { ["a"] = 3, ["b"] = 1, ["c"] = 1, ["d"] = 9, ["e"] = 0 };
        Assert.DoesNotContain(BadgeCatalog.NewlyEarned(Snap(wins: 14, byGame: four), new HashSet<string>()), b => b.Id == "explorer");
        four["e"] = 1;
        Assert.Contains(BadgeCatalog.NewlyEarned(Snap(wins: 15, byGame: four), new HashSet<string>()), b => b.Id == "explorer");
    }

    [Fact]
    public void StreakBadges_FollowBestStreak()
    {
        var ids = BadgeCatalog.NewlyEarned(Snap(streak: 7), new HashSet<string>()).Select(b => b.Id).ToList();
        Assert.Contains("streak_3", ids);
        Assert.Contains("streak_7", ids);
        Assert.DoesNotContain("streak_30", ids);
    }
}
