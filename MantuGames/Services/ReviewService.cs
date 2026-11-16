#if ANDROID
using Android.Gms.Tasks;
using Android.Runtime;
using Google.Android.Play.Core.Review;
#endif

namespace MantuGames.Services;

/// <summary>
/// Shows Google Play's own in-app rating card at a happy moment (after a level is won),
/// so players can rate without leaving the game. Google decides whether the card
/// actually appears and limits how often, so asking here never nags the player.
/// </summary>
public static class ReviewService
{
    private const string WinsKey = "review_wins";
    private const string LastAskKey = "review_last_ask_utc";
    private const int FirstAskAfterWins = 3;   // let players enjoy a few levels first
    private const int AskEveryWins = 10;       // then only now and then
    private static readonly TimeSpan MinGap = TimeSpan.FromDays(30);
    private static bool _busy;

    /// <summary>Call after a level is won for the first time.</summary>
    public static async System.Threading.Tasks.Task OnLevelWonAsync()
    {
        int wins = Preferences.Get(WinsKey, 0) + 1;
        Preferences.Set(WinsKey, wins);

        bool due = wins == FirstAskAfterWins ||
                   (wins > FirstAskAfterWins && (wins - FirstAskAfterWins) % AskEveryWins == 0);
        if (!due) return;

        long last = Preferences.Get(LastAskKey, 0L);
        if (last > 0 && DateTime.UtcNow - new DateTime(last, DateTimeKind.Utc) < MinGap) return;

        if (await RequestAsync())
            Preferences.Set(LastAskKey, DateTime.UtcNow.Ticks);
    }

    /// <summary>Asks Play for the rating card. Returns true if the flow ran.</summary>
    public static async System.Threading.Tasks.Task<bool> RequestAsync()
    {
#if ANDROID
        if (_busy) return false;
        _busy = true;
        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return false;

            var manager = ReviewManagerFactory.Create(activity);
            var request = await AwaitTask(manager.RequestReviewFlow());
            if (!request.IsSuccessful || request.Result == null) return false;

            var info = request.Result.JavaCast<ReviewInfo>();
            if (info == null) return false;
            await AwaitTask(manager.LaunchReviewFlow(activity, info));
            Android.Util.Log.Info("MantuGames", "In-app review flow finished");
            return true;
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("MantuGames", $"In-app review failed: {ex.Message}");
            return false;
        }
        finally
        {
            _busy = false;
        }
#else
        await System.Threading.Tasks.Task.CompletedTask;
        return false;
#endif
    }

#if ANDROID
    private static System.Threading.Tasks.Task<Android.Gms.Tasks.Task> AwaitTask(Android.Gms.Tasks.Task task)
    {
        var listener = new CompleteListener();
        task.AddOnCompleteListener(listener);
        return listener.Completion;
    }

    private sealed class CompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private readonly TaskCompletionSource<Android.Gms.Tasks.Task> _tcs = new();
        public System.Threading.Tasks.Task<Android.Gms.Tasks.Task> Completion => _tcs.Task;
        public void OnComplete(Android.Gms.Tasks.Task task) => _tcs.TrySetResult(task);
    }
#endif
}
