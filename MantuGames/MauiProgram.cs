using Microsoft.Extensions.Logging;
using MantuGames.Services;
using Plugin.Maui.Audio;
using Plugin.MauiMtAdmob;

namespace MantuGames;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMTAdmob()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Qilka-Bold.otf", "QilkaBold");
                // fonts.AddFont("BrickSans-Regular.ttf", "BrickSans");
                fonts.AddFont("rimouski-sb.otf", "BrickSans");
            });

        builder.AddAudio();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<AdService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}