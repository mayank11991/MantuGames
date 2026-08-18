using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.MauiMtAdmob;

namespace MantuGames;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        // Opt in to edge-to-edge before the base theme is applied
        // (fixes Play Console edge-to-edge warnings; see styles.xml).
        SetTheme(Resource.Style.MainThemeEdgeToEdge);

        base.OnCreate(savedInstanceState);

        CrossMauiMTAdmob.Current.Init(
            activity: this,
            appId: AppConfig.AdMobAppId,
#if DEBUG
            forceTesting: true,
            debugMode: true
#else
            forceTesting: false,
            debugMode: false
#endif
        );
    }
}