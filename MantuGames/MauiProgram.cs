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
                // Display type: Anton (tall condensed headings, OFL — see licenses/Anton-OFL.txt)
                fonts.AddFont("Anton-Regular.ttf", "Display");

                // Body type: Baloo 2 (rounded, friendly)
                fonts.AddFont("Baloo2-Regular.ttf", "Baloo2");
                fonts.AddFont("Baloo2-Medium.ttf", "Baloo2Medium");
                fonts.AddFont("Baloo2-SemiBold.ttf", "Baloo2SemiBold");
                fonts.AddFont("Baloo2-Bold.ttf", "Baloo2Bold");
                fonts.AddFont("Baloo2-ExtraBold.ttf", "Baloo2ExtraBold");

                // Legacy alias: every XAML page uses MadimiOne
                fonts.AddFont("Baloo2-Medium.ttf", "MadimiOne");

                // Legacy aliases remapped to the clay type family
                fonts.AddFont("Baloo2-Medium.ttf", "MomoTrustDisplay");
                fonts.AddFont("Baloo2-Medium.ttf", "Milkyway");
                fonts.AddFont("Baloo2-SemiBold.ttf", "Orbitron");
                fonts.AddFont("Baloo2-Regular.ttf", "Inter");
                fonts.AddFont("Baloo2-Medium.ttf", "InterMedium");
                fonts.AddFont("Baloo2-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Baloo2-Bold.ttf", "InterBold");
                fonts.AddFont("Baloo2-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Baloo2-SemiBold.ttf", "OpenSansSemibold");
                fonts.AddFont("Baloo2-ExtraBold.ttf", "PressStart2P");
                fonts.AddFont("Baloo2-Medium.ttf", "Fredoka");
                fonts.AddFont("Baloo2-SemiBold.ttf", "FredokaSemiBold");
                fonts.AddFont("Baloo2-Regular.ttf", "Nunito");
                fonts.AddFont("Baloo2-Bold.ttf", "NunitoBold");
                fonts.AddFont("Baloo2-ExtraBold.ttf", "NunitoExtraBold");
                fonts.AddFont("Baloo2-SemiBold.ttf", "BrickSans");
                fonts.AddFont("Baloo2-Medium.ttf", "SagoMini");
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