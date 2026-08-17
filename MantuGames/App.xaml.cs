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
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new Views.SplashPage());
    }
}