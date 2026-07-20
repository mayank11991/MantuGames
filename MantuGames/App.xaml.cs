using MantuGames.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace MantuGames;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeHelper.Init();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new Views.SplashPage());
    }
}