using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace MantuGames;

/// <summary>Shows the daily-challenge reminder scheduled by <see cref="Services.ReminderService"/>.</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class ReminderReceiver : BroadcastReceiver
{
    public const string ChannelId = "daily_reminder";
    public const string ExtraTitle = "title";
    public const string ExtraText = "text";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;
        try
        {
            var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
            if (manager == null) return;

            if (OperatingSystem.IsAndroidVersionAtLeast(26) && manager.GetNotificationChannel(ChannelId) == null)
            {
                manager.CreateNotificationChannel(new NotificationChannel(ChannelId, "Daily challenge reminder",
                    NotificationImportance.Default) { Description = "A friendly reminder when today's challenge is waiting." });
            }

            var open = new Intent(context, typeof(MainActivity));
            open.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            var tap = PendingIntent.GetActivity(context, 0, open, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(context, ChannelId)
                .SetSmallIcon(Resource.Drawable.ic_notify_star)
                .SetContentTitle(intent.GetStringExtra(ExtraTitle) ?? "Mantu")
                .SetContentText(intent.GetStringExtra(ExtraText) ?? "Today's challenge is waiting!")
                .SetAutoCancel(true)
                .SetContentIntent(tap)
                .SetPriority(NotificationCompat.PriorityDefault)
                .Build();

            manager.Notify(1001, notification);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("Mantu", "Reminder failed: " + ex.Message);
        }
    }
}
