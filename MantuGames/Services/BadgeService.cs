using MantuGames.Models;
using MantuGames.ViewModels;

namespace MantuGames.Services;

/// <summary>Tracks which badges the current player has earned and pays their coin reward once.</summary>
public static class BadgeService
{
    private static string EarnedKey => ProfileService.Key("badges_earned");

    public static ISet<string> Earned =>
        Preferences.Default.Get(EarnedKey, "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

    public static PlayerSnapshot Snapshot()
    {
        var wins = new Dictionary<string, int>();
        int levels = 0;
        foreach (var g in DashboardViewModel.Games)
        {
            wins[g.Id] = StatsService.GetStats(g.Id).Won;
            levels += ProgressService.Instance.GetLevels(g.Id, 50).Count(l => l.IsCompleted);
        }
        return new PlayerSnapshot(wins.Values.Sum(), levels, DailyService.BestStreak, wins);
    }

    /// <summary>
    /// Call after a game result is saved. Records any newly earned badges, pays
    /// <see cref="BadgeCatalog.Reward"/> coins for each and returns them (empty if none).
    /// </summary>
    public static IReadOnlyList<Badge> CheckNew()
    {
        if (ProfileService.Active == null) return Array.Empty<Badge>();
        var earned = Earned;
        var fresh = BadgeCatalog.NewlyEarned(Snapshot(), earned);
        if (fresh.Count == 0) return fresh;

        foreach (var b in fresh) earned.Add(b.Id);
        Preferences.Default.Set(EarnedKey, string.Join(',', earned));
        CoinService.AddCoins(BadgeCatalog.Reward * fresh.Count);
        return fresh;
    }
}
