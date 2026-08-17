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
        base.OnCreate(savedInstanceState);

        // Dark system bars matching the app theme
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#0B0E14"));
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#0B0E14"));
        }

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