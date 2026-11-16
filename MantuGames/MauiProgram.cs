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
                // Whole app uses Lilita One (chunky rounded cartoon type, OFL — see licenses/LilitaOne-OFL.txt)
                fonts.AddFont("LilitaOne-Regular.ttf", "Display");

                // Body aliases point at the same face
                fonts.AddFont("LilitaOne-Regular.ttf", "Baloo2");
                fonts.AddFont("LilitaOne-Regular.ttf", "Baloo2Medium");
                fonts.AddFont("LilitaOne-Regular.ttf", "Baloo2SemiBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "Baloo2Bold");
                fonts.AddFont("LilitaOne-Regular.ttf", "Baloo2ExtraBold");

                // Legacy alias: every XAML page uses MadimiOne
                fonts.AddFont("LilitaOne-Regular.ttf", "MadimiOne");

                // Legacy aliases remapped to the clay type family
                fonts.AddFont("LilitaOne-Regular.ttf", "MomoTrustDisplay");
                fonts.AddFont("LilitaOne-Regular.ttf", "Milkyway");
                fonts.AddFont("LilitaOne-Regular.ttf", "Orbitron");
                fonts.AddFont("LilitaOne-Regular.ttf", "Inter");
                fonts.AddFont("LilitaOne-Regular.ttf", "InterMedium");
                fonts.AddFont("LilitaOne-Regular.ttf", "InterSemiBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "InterBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("LilitaOne-Regular.ttf", "OpenSansSemibold");
                fonts.AddFont("LilitaOne-Regular.ttf", "PressStart2P");
                fonts.AddFont("LilitaOne-Regular.ttf", "Fredoka");
                fonts.AddFont("LilitaOne-Regular.ttf", "FredokaSemiBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "Nunito");
                fonts.AddFont("LilitaOne-Regular.ttf", "NunitoBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "NunitoExtraBold");
                fonts.AddFont("LilitaOne-Regular.ttf", "BrickSans");
                fonts.AddFont("LilitaOne-Regular.ttf", "SagoMini");
            });

        builder.AddAudio();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<AdService>();

        // Remove the default Android underline from every Entry (search, profile name, ...)
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryNoUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}