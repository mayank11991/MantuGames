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
                // Primary fonts matching website: Orbitron (display) + Inter (body)
                fonts.AddFont("Orbitron.ttf", "Orbitron");
                fonts.AddFont("Inter.ttf", "Inter");
                fonts.AddFont("Inter.ttf", "InterMedium");
                fonts.AddFont("Inter.ttf", "InterSemiBold");
                fonts.AddFont("Inter.ttf", "InterBold");

                // Legacy aliases mapped to basenji for backward compatibility
                fonts.AddFont("basenji_semibold.otf", "OpenSansRegular");
                fonts.AddFont("basenji_semibold.otf", "OpenSansSemibold");
                fonts.AddFont("basenji_semibold.otf", "PressStart2P");
                fonts.AddFont("basenji_semibold.otf", "Baloo2");
                fonts.AddFont("basenji_semibold.otf", "Baloo2SemiBold");
                fonts.AddFont("basenji_semibold.otf", "Baloo2Bold");
                fonts.AddFont("basenji_semibold.otf", "Fredoka");
                fonts.AddFont("basenji_semibold.otf", "FredokaSemiBold");
                fonts.AddFont("basenji_semibold.otf", "Nunito");
                fonts.AddFont("basenji_semibold.otf", "NunitoBold");
                fonts.AddFont("basenji_semibold.otf", "NunitoExtraBold");
                fonts.AddFont("basenji_semibold.otf", "BrickSans");
                fonts.AddFont("basenji_semibold.otf", "SagoMini");
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