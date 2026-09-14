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
                // Primary font: MomoTrustDisplay for all screens
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "MomoTrustDisplay");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Milkyway");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Orbitron");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Inter");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "InterMedium");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "InterSemiBold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "InterBold");

                // Legacy aliases mapped to MomoTrustDisplay for backward compatibility
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "OpenSansSemibold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "PressStart2P");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Baloo2");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Baloo2SemiBold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Baloo2Bold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Fredoka");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "FredokaSemiBold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "Nunito");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "NunitoBold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "NunitoExtraBold");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "BrickSans");
                fonts.AddFont("MomoTrustDisplay-Regular.ttf", "SagoMini");
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

        Microsoft.Maui.Handlers.GraphicsViewHandler.Mapper.AppendToMapping("NativeTouch", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.SetOnTouchListener(new MantuGames.Platforms.Android.GraphicsTouchListener(view as Microsoft.Maui.Controls.GraphicsView));
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}