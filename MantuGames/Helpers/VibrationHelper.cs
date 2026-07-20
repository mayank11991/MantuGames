namespace MantuGames.Helpers;

public static class VibrationHelper
{
    public static bool Enabled
    {
        get => Preferences.Get("vibration_enabled", true);
        set => Preferences.Set("vibration_enabled", value);
    }

    public static void Click()
    {
        if (!Enabled) return;
        try
        {
#if ANDROID
            var ctx = Android.App.Application.Context;
            var vibrator = ctx.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
            if (vibrator?.HasVibrator == true)
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    vibrator.Vibrate(Android.OS.VibrationEffect.CreateOneShot(20, Android.OS.VibrationEffect.DefaultAmplitude));
                else
                    vibrator.Vibrate(20);
                return;
            }
#endif
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch { }
    }

    public static void LongPress()
    {
        if (!Enabled) return;
        try
        {
#if ANDROID
            var ctx = Android.App.Application.Context;
            var vibrator = ctx.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
            if (vibrator?.HasVibrator == true)
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    vibrator.Vibrate(Android.OS.VibrationEffect.CreateOneShot(50, Android.OS.VibrationEffect.DefaultAmplitude));
                else
                    vibrator.Vibrate(50);
                return;
            }
#endif
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch { }
    }
}
