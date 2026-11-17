namespace MantuGames.Services;

/// <summary>
/// One local reminder a day at 6 pm while today's daily challenge is not done yet
/// ("Your 3-day streak is waiting!"). Nothing leaves the device. Parents can turn it
/// off in Settings; Android 13+ also asks for notification permission once.
/// </summary>
public static class ReminderService
{
    private const string EnabledKey = "reminder_enabled";
    private const string AskedKey = "reminder_permission_asked";
    public const int ReminderHour = 18;

    public static bool Enabled
    {
        get => Preferences.Default.Get(EnabledKey, true);
        set
        {
            Preferences.Default.Set(EnabledKey, value);
            Reschedule();
        }
    }

    /// <summary>Asks for notification permission once (Android 13+), then schedules.</summary>
    public static async Task AskOnceAndScheduleAsync()
    {
        try
        {
            if (Enabled && !Preferences.Default.Get(AskedKey, false))
            {
                Preferences.Default.Set(AskedKey, true);
                await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
        Reschedule();
    }

    /// <summary>Next reminder time: today 6 pm if the challenge is still open and it's earlier, else tomorrow 6 pm.</summary>
    public static DateTime NextReminder(DateTime now, bool doneToday)
    {
        var today = now.Date.AddHours(ReminderHour);
        return !doneToday && now < today ? today : today.AddDays(1);
    }

    public static void Reschedule()
    {
#if ANDROID
        try
        {
            var ctx = Android.App.Application.Context;
            var alarms = (Android.App.AlarmManager?)ctx.GetSystemService(Android.Content.Context.AlarmService);
            if (alarms == null) return;

            var intent = new Android.Content.Intent(ctx, typeof(ReminderReceiver));
            int streak = DailyService.CurrentStreak;
            var game = DailyService.TodayGame;
            intent.PutExtra(ReminderReceiver.ExtraTitle, streak > 0
                ? $"🔥 Your {streak}-day streak is waiting!"
                : "🌟 Today's challenge is ready!");
            intent.PutExtra(ReminderReceiver.ExtraText, $"Play today's level in {game.Title} with {SkinService.Current.Name}.");

            var pending = Android.App.PendingIntent.GetBroadcast(ctx, 7, intent,
                Android.App.PendingIntentFlags.Immutable | Android.App.PendingIntentFlags.UpdateCurrent)!;
            alarms.Cancel(pending);
            if (!Enabled || ProfileService.Active == null) return;

            var when = NextReminder(DateTime.Now, DailyService.IsDoneToday);
            long millis = new DateTimeOffset(when).ToUnixTimeMilliseconds();
            // Inexact is fine for a friendly reminder and needs no exact-alarm permission.
            alarms.SetAndAllowWhileIdle(Android.App.AlarmType.RtcWakeup, millis, pending);
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
#endif
    }
}
