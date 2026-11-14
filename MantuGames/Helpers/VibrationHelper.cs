namespace MantuGames.Helpers;

public static class VibrationHelper
{
    public static bool Enabled
    {
        get => Preferences.Get("vibration_enabled", true);
        set => Preferences.Set("vibration_enabled", value);
    }

    public static void Click() => Vibrate(20, HapticFeedbackType.Click);

    public static void LongPress() => Vibrate(50, HapticFeedbackType.LongPress);

    private static void Vibrate(long milliseconds, HapticFeedbackType fallback)
    {
        if (!Enabled) return;
        try
        {
#if ANDROID
            if (TryAndroidVibrate(milliseconds)) return;
#endif
            HapticFeedback.Default.Perform(fallback);
        }
        catch { }
    }

#if ANDROID
    private static bool TryAndroidVibrate(long milliseconds)
    {
        var ctx = Android.App.Application.Context;

        Android.OS.Vibrator? vibrator;
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var manager = ctx.GetSystemService(Android.Content.Context.VibratorManagerService) as Android.OS.VibratorManager;
            vibrator = manager?.DefaultVibrator;
        }
        else
        {
#pragma warning disable CA1422 // VibratorService is the only option below Android 12
            vibrator = ctx.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
#pragma warning restore CA1422
        }

        if (vibrator?.HasVibrator != true) return false;

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            vibrator.Vibrate(Android.OS.VibrationEffect.CreateOneShot(milliseconds, Android.OS.VibrationEffect.DefaultAmplitude));
        }
        else
        {
#pragma warning disable CA1422 // Vibrate(long) is the only option below Android 8
            vibrator.Vibrate(milliseconds);
#pragma warning restore CA1422
        }
        return true;
    }
#endif
}
