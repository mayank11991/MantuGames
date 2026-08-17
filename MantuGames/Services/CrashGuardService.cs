namespace MantuGames.Services;

/// <summary>
/// Catches unhandled exceptions (managed + native) and persists them to a
/// local crash log so the app can show a friendly recovery notice next launch.
/// </summary>
public static class CrashGuardService
{
    private static readonly object _lock = new();
    private static bool _registered;

    public static string LogPath => Path.Combine(FileSystem.AppDataDirectory, "crash_log.txt");

    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        AppDomain.CurrentDomain.UnhandledException += (s, e) => Log(e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log(e.Exception);
            e.SetObserved();
        };
    }

    public static void Log(Exception? ex)
    {
        if (ex == null) return;
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n");
            }
        }
        catch { }
    }

    /// <summary>Returns any pending crash report and clears the log.</summary>
    public static string? TakePendingReport()
    {
        try
        {
            lock (_lock)
            {
                if (!File.Exists(LogPath)) return null;
                string text = File.ReadAllText(LogPath).Trim();
                File.Delete(LogPath);
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }
        catch { return null; }
    }
}