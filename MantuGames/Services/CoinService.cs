using MantuGames.ViewModels;

namespace MantuGames.Services;

/// <summary>
/// Per-game coin wallets. Each game has its own independent coin balance,
/// used to pay for "Show Solution". Coins are bought via in-app purchase
/// and transferred to a game of the player's choice.
/// </summary>
public static class CoinService
{
    public const int SolutionCost = 10;

    private static string Key(string gameId) => ProfileService.Key($"{gameId}_coins");

    public static int GetCoins(string gameId) => Preferences.Default.Get(Key(gameId), 0);

    public static void AddCoins(string gameId, int amount)
    {
        if (amount <= 0) return;
        Preferences.Default.Set(Key(gameId), GetCoins(gameId) + amount);
    }

    public static bool SpendCoins(string gameId, int amount)
    {
        int balance = GetCoins(gameId);
        if (balance < amount) return false;
        Preferences.Default.Set(Key(gameId), balance - amount);
        return true;
    }

    public static int GetTotalCoins()
    {
        int total = 0;
        foreach (var g in DashboardViewModel.Games)
            total += GetCoins(g.Id);
        return total;
    }

    public static void ResetAll()
    {
        foreach (var g in DashboardViewModel.Games)
            Preferences.Default.Remove(Key(g.Id));
    }
}