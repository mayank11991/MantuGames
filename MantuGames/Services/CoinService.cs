using MantuGames.ViewModels;

namespace MantuGames.Services;

/// <summary>
/// One coin wallet per player profile. Coins are earned by beating levels (new best
/// stars only), bought in the shop, and spent on "Show Solution" in any game.
/// </summary>
public static class CoinService
{
    public const int SolutionCost = 10;

    /// <summary>Raised (any thread) whenever the balance changes.</summary>
    public static event EventHandler? CoinsChanged;

    private static readonly object _lock = new();

    private static string WalletKey => ProfileService.Key("wallet_coins");
    private static string MigratedKey => ProfileService.Key("wallet_migrated");

    public static int Balance
    {
        get
        {
            EnsureMigrated();
            return Preferences.Default.Get(WalletKey, 0);
        }
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        lock (_lock) Preferences.Default.Set(WalletKey, Balance + amount);
        CoinsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static bool SpendCoins(int amount)
    {
        lock (_lock)
        {
            int balance = Balance;
            if (balance < amount) return false;
            Preferences.Default.Set(WalletKey, balance - amount);
        }
        CoinsChanged?.Invoke(null, EventArgs.Empty);
        return true;
    }

    /// <summary>Pays for "Show Solution" and marks the current attempt as assisted.</summary>
    public static bool SpendForSolution(string gameId)
    {
        if (!SpendCoins(SolutionCost)) return false;
        ProgressService.MarkAssisted(gameId);
        return true;
    }

    public static void ResetAll()
    {
        Preferences.Default.Set(WalletKey, 0);
        CoinsChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Older builds kept a separate wallet per game plus a device-wide bank of bought
    /// coins. Fold all of it into this profile's wallet once, so nobody loses coins.
    /// </summary>
    private static void EnsureMigrated()
    {
        if (Preferences.Default.Get(MigratedKey, false)) return;
        lock (_lock)
        {
            if (Preferences.Default.Get(MigratedKey, false)) return;
            int total = Preferences.Default.Get(WalletKey, 0);
            foreach (var g in DashboardViewModel.Games)
            {
                string legacy = ProfileService.Key($"{g.Id}_coins");
                total += Preferences.Default.Get(legacy, 0);
                Preferences.Default.Remove(legacy);
            }
            total += IapService.TakeLegacyUnassignedCoins();
            // Coins credited before any profile was active landed in the un-prefixed key.
            if (ProfileService.Active != null)
            {
                total += Preferences.Default.Get("wallet_coins", 0);
                Preferences.Default.Remove("wallet_coins");
            }
            Preferences.Default.Set(WalletKey, total);
            Preferences.Default.Set(MigratedKey, true);
        }
    }
}
