package crc64509fec87287e985b;


public class UMPImplementation
	extends com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAdLoadCallback
	implements
		mono.android.IGCUserPeer,
		com.google.android.ump.ConsentInformation.OnConsentInfoUpdateSuccessListener,
		com.google.android.ump.ConsentInformation.OnConsentInfoUpdateFailureListener,
		com.google.android.ump.ConsentForm.OnConsentFormDismissedListener,
		com.google.android.ump.UserMessagingPlatform.OnConsentFormLoadSuccessListener,
		com.google.android.ump.UserMessagingPlatform.OnConsentFormLoadFailureListener,
		com.google.android.gms.ads.initialization.OnInitializationCompleteListener
{

	public UMPImplementation ()
	{
		super ();
		if (getClass () == UMPImplementation.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.UMPImplementation, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public UMPImplementation (crc6491bfa4099bd0c3ba.MauiMTAdmob p0, crc6488302ad6e9e4df1a.MauiAppCompatActivity p1, java.lang.String p2, java.lang.String p3, java.lang.String p4, boolean p5, java.lang.String p6, boolean p7, int p8, boolean p9)
	{
		super ();
		if (getClass () == UMPImplementation.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.UMPImplementation, Plugin.MauiMtAdmob", "Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob:Microsoft.Maui.MauiAppCompatActivity, Microsoft.Maui:System.String, System.Private.CoreLib:System.String, System.Private.CoreLib:System.String, System.Private.CoreLib:System.Boolean, System.Private.CoreLib:System.String, System.Private.CoreLib:System.Boolean, System.Private.CoreLib:Plugin.MauiMtAdmob.Extra.DebugGeography, Plugin.MauiMtAdmob:System.Boolean, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9 });
		}
	}

	public void onConsentInfoUpdateSuccess ()
	{
		n_onConsentInfoUpdateSuccess ();
	}

	private native void n_onConsentInfoUpdateSuccess ();

	public void onConsentInfoUpdateFailure (com.google.android.ump.FormError p0)
	{
		n_onConsentInfoUpdateFailure (p0);
	}

	private native void n_onConsentInfoUpdateFailure (com.google.android.ump.FormError p0);

	public void onConsentFormDismissed (com.google.android.ump.FormError p0)
	{
		n_onConsentFormDismissed (p0);
	}

	private native void n_onConsentFormDismissed (com.google.android.ump.FormError p0);

	public void onConsentFormLoadSuccess (com.google.android.ump.ConsentForm p0)
	{
		n_onConsentFormLoadSuccess (p0);
	}

	private native void n_onConsentFormLoadSuccess (com.google.android.ump.ConsentForm p0);

	public void onConsentFormLoadFailure (com.google.android.ump.FormError p0)
	{
		n_onConsentFormLoadFailure (p0);
	}

	private native void n_onConsentFormLoadFailure (com.google.android.ump.FormError p0);

	public void onInitializationComplete (com.google.android.gms.ads.initialization.InitializationStatus p0)
	{
		n_onInitializationComplete (p0);
	}

	private native void n_onInitializationComplete (com.google.android.gms.ads.initialization.InitializationStatus p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
