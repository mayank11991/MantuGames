namespace MantuGames.Helpers;

/// <summary>Matches Android's status / navigation bars to the current screen's backdrop.</summary>
public static class SystemBars
{
    public static void Apply(Color statusBar, Color navigationBar, bool darkIcons)
    {
#if ANDROID
        try
        {
            var window = Platform.CurrentActivity?.Window;
            if (window == null) return;

            // Android 15+ is edge-to-edge and ignores bar colours; icons still follow the flag.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                window.SetStatusBarColor(Android.Graphics.Color.ParseColor(statusBar.ToArgbHex()));
                window.SetNavigationBarColor(Android.Graphics.Color.ParseColor(navigationBar.ToArgbHex()));
            }

            var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
            if (controller != null)
            {
                controller.AppearanceLightStatusBars = darkIcons;
                controller.AppearanceLightNavigationBars = darkIcons;
            }
        }
        catch (Exception ex)
        {
            Services.CrashGuardService.Log(ex);
        }
#endif
    }
}
