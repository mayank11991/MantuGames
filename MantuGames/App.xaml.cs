using MantuGames.Helpers;
using MantuGames.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MantuGames;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeHelper.Init();
        CrashGuardService.Register();
        ProfileService.EnsureLoaded();
        SkinService.ApplyTheme();

        // Block Stack pieces take the premium skin's palette (I O T S Z L J -> colours 0..6).
        Models.BlockPiece.ThemeOverride = name =>
        {
            if (!ThemeColors.Active) return null;
            int i = "IOTSZLJ".IndexOf(name, StringComparison.Ordinal);
            if (i < 0) return null;
            return i < 5 ? ThemeColors.Get(i, "#7C5CFF") : Color.FromArgb(ThemeColors.Tint(i - 5, 0.35, "#7C5CFF"));
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new Views.SplashPage());
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        try { AudioService.Instance.PauseForBackground(); } catch { }
    }

    protected override void OnResume()
    {
        base.OnResume();
        try { AudioService.Instance.ResumeFromBackground(); } catch { }
    }
}