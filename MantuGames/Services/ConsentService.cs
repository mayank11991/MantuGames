#if ANDROID
using Android.App;
using Plugin.MauiMtAdmob;
using Xamarin.Google.UserMesssagingPlatform;
#endif

namespace MantuGames.Services;

/// <summary>
/// Google UMP (User Messaging Platform) consent flow, required by AdMob for
/// EEA / UK / Switzerland users. MTAdmob only runs UMP in its paid licensed
/// version, so we drive the UMP SDK directly and initialise ads only once
/// consent allows it.
///
/// Flow (per Google's guidance):
///   1. RequestConsentInfoUpdate on every launch.
///   2. LoadAndShowConsentFormIfRequired (no-op outside regulated regions).
///   3. Initialise the Mobile Ads SDK only when CanRequestAds() is true.
///      If a previous session already gathered consent, start immediately.
/// </summary>
public static class ConsentService
{
    /// <summary>True once consent allows ad requests and the ads SDK is initialised.</summary>
    public static bool CanRequestAds { get; private set; }

    /// <summary>True when the user must be offered a "Privacy choices" entry point (regulated regions).</summary>
    public static bool IsPrivacyOptionsRequired { get; private set; }

    /// <summary>Raised once (on the main thread) when ads become allowed.</summary>
    public static event EventHandler? AdsAllowed;

#if ANDROID
    private static IConsentInformation? _info;
    private static MauiAppCompatActivity? _activity;
    private static bool _adsInitialised;
    private static bool _gathering;

    public static void Gather(MauiAppCompatActivity activity)
    {
        _activity = activity;
        if (_gathering) return;
        _gathering = true;

        try
        {
            _info = UserMessagingPlatform.GetConsentInformation(activity);

            var parameters = new ConsentRequestParameters.Builder()
                .SetTagForUnderAgeOfConsent(false) // audience is 13+
                .Build();

            _info.RequestConsentInfoUpdate(
                activity,
                parameters,
                new UpdateSuccessListener(() =>
                {
                    try
                    {
                        UserMessagingPlatform.LoadAndShowConsentFormIfRequired(
                            activity, new DismissListener(_ => Finish()));
                    }
                    catch (Exception ex)
                    {
                        CrashGuardService.Log(ex);
                        Finish();
                    }
                }),
                new UpdateFailureListener(_ => Finish()));

            // Consent gathered in a previous session: start ads right away.
            if (_info.CanRequestAds())
                Finish();
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
            _gathering = false;
        }
    }

    private static void Finish()
    {
        _gathering = false;
        if (_info == null) return;

        try
        {
            IsPrivacyOptionsRequired = _info.PrivacyOptionsRequirementStatus
                                       == ConsentInformationPrivacyOptionsRequirementStatus.Required;

            if (!_info.CanRequestAds() || _adsInitialised || _activity == null) return;
            _adsInitialised = true;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    CrossMauiMTAdmob.Current.Init(
                        activity: _activity,
                        appId: AppConfig.AdMobAppId,
#if DEBUG
                        forceTesting: true,
                        debugMode: true
#else
                        forceTesting: false,
                        debugMode: false
#endif
                    );
                    CanRequestAds = true;
                    AdsAllowed?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _adsInitialised = false;
                    CrashGuardService.Log(ex);
                }
            });
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
    }

    /// <summary>Opens Google's privacy options form so the user can change consent.</summary>
    public static Task ShowPrivacyOptionsAsync()
    {
        var tcs = new TaskCompletionSource();
        try
        {
            if (_activity == null) { tcs.TrySetResult(); return tcs.Task; }
            UserMessagingPlatform.ShowPrivacyOptionsForm(_activity, new DismissListener(_ =>
            {
                Finish();
                tcs.TrySetResult();
            }));
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
            tcs.TrySetResult();
        }
        return tcs.Task;
    }

    private sealed class UpdateSuccessListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateSuccessListener
    {
        private readonly Action _onSuccess;
        public UpdateSuccessListener(Action onSuccess) => _onSuccess = onSuccess;
        public void OnConsentInfoUpdateSuccess() => _onSuccess();
    }

    private sealed class UpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
    {
        private readonly Action<FormError> _onFailure;
        public UpdateFailureListener(Action<FormError> onFailure) => _onFailure = onFailure;
        public void OnConsentInfoUpdateFailure(FormError error) => _onFailure(error);
    }

    private sealed class DismissListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
    {
        private readonly Action<FormError?> _onDismissed;
        public DismissListener(Action<FormError?> onDismissed) => _onDismissed = onDismissed;
        public void OnConsentFormDismissed(FormError? error) => _onDismissed(error);
    }
#else
    public static Task ShowPrivacyOptionsAsync() => Task.CompletedTask;
#endif
}
