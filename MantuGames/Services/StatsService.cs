namespace MantuGames.Services;

public record GameStats(int Played, int Won, int BestPoints);

/// <summary>
/// Per-game lifetime statistics: games played, games won, best score.
/// </summary>
public static class StatsService
{
    private static string PlayedKey(string gameId) => ProfileService.Key($"{gameId}_stats_played");
    private static string WonKey(string gameId) => ProfileService.Key($"{gameId}_stats_won");
    private static string BestKey(string gameId) => ProfileService.Key($"{gameId}_stats_best");

    public static void RecordGame(string gameId, bool isWin, int points)
    {
        if (string.IsNullOrEmpty(gameId)) return;

        Preferences.Set(PlayedKey(gameId), Preferences.Get(PlayedKey(gameId), 0) + 1);
        if (isWin)
            Preferences.Set(WonKey(gameId), Preferences.Get(WonKey(gameId), 0) + 1);
        if (points > 0 && points > Preferences.Get(BestKey(gameId), 0))
            Preferences.Set(BestKey(gameId), points);
    }

    public static GameStats GetStats(string gameId) => new(
        Preferences.Get(PlayedKey(gameId), 0),
        Preferences.Get(WonKey(gameId), 0),
        Preferences.Get(BestKey(gameId), 0));

    public static (int Played, int Won, int TotalPoints) GetTotals()
    {
        int played = 0, won = 0, points = 0;
        foreach (var g in ViewModels.DashboardViewModel.Games)
        {
            var s = GetStats(g.Id);
            played += s.Played;
            won += s.Won;
            points += ProgressService.Instance.GetTotalPoints(g.Id);
        }
        return (played, won, points);
    }

    public static double WinRate(int played, int won) =>
        played == 0 ? 0 : Math.Round(won * 100.0 / played);

    public static void ResetAll()
    {
        foreach (var g in ViewModels.DashboardViewModel.Games)
        {
            Preferences.Remove(PlayedKey(g.Id));
            Preferences.Remove(WonKey(g.Id));
            Preferences.Remove(BestKey(g.Id));
        }
    }
}