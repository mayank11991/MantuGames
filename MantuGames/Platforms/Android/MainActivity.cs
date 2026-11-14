using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace MantuGames;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Workaround for MAUI Shell "No view found for id italic" crash
        if (savedInstanceState != null)
        {
            savedInstanceState.Remove("android:support:fragments");
            savedInstanceState.Remove("android:fragments");
        }

        // Opt in to edge-to-edge before the base theme is applied
        // (fixes Play Console edge-to-edge warnings; see styles.xml).
        SetTheme(Resource.Style.MainThemeEdgeToEdge);

        base.OnCreate(savedInstanceState);

        ApplyClaySystemBars();

        // Gather UMP consent first; ads SDK is initialised only once consent allows it.
        MantuGames.Services.ConsentService.Gather(this);
    }

    private void ApplyClaySystemBars()
    {
        try
        {
            var clay = Android.Graphics.Color.ParseColor("#F7EFFF");
            // Android 15+ draws edge-to-edge and ignores bar colours; the page background shows through.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                Window?.SetStatusBarColor(clay);
                Window?.SetNavigationBarColor(clay);
            }

            if (Window is not null)
            {
                var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
                if (controller != null)
                {
                    controller.AppearanceLightStatusBars = true;
                    controller.AppearanceLightNavigationBars = true;
                }
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("MantuGames", $"System bar theming failed: {ex.Message}");
        }
    }

    protected override void OnStart()
    {
        try
        {
            base.OnStart();
        }
        catch (Java.Lang.IllegalArgumentException ex) when (ex.Message?.Contains("italic") == true)
        {
            // MAUI Shell NavigationRootManager fragment bug - ignore, app still works
            Android.Util.Log.Warn("MantuGames", $"Shell italic crash suppressed: {ex.Message}");
        }
    }
}
