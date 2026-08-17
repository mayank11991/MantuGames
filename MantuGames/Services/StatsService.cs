namespace MantuGames.Services;

public record GameStats(int Played, int Won);

/// <summary>
/// Per-game lifetime statistics: games played, games won.
/// </summary>
public static class StatsService
{
    private static string PlayedKey(string gameId) => ProfileService.Key($"{gameId}_stats_played");
    private static string WonKey(string gameId) => ProfileService.Key($"{gameId}_stats_won");

    public static void RecordGame(string gameId, bool isWin, int coins)
    {
        if (string.IsNullOrEmpty(gameId)) return;

        Preferences.Set(PlayedKey(gameId), Preferences.Get(PlayedKey(gameId), 0) + 1);
        if (isWin)
            Preferences.Set(WonKey(gameId), Preferences.Get(WonKey(gameId), 0) + 1);
    }

    public static GameStats GetStats(string gameId) => new(
        Preferences.Get(PlayedKey(gameId), 0),
        Preferences.Get(WonKey(gameId), 0));

    public static (int Played, int Won) GetTotals()
    {
        int played = 0, won = 0;
        foreach (var g in ViewModels.DashboardViewModel.Games)
        {
            var s = GetStats(g.Id);
            played += s.Played;
            won    += s.Won;
        }
        return (played, won);
    }

    public static double WinRate(int played, int won) =>
        played == 0 ? 0 : Math.Round(won * 100.0 / played);

    public static void ResetAll()
    {
        foreach (var g in ViewModels.DashboardViewModel.Games)
        {
            Preferences.Remove(PlayedKey(g.Id));
            Preferences.Remove(WonKey(g.Id));
        }
    }
}