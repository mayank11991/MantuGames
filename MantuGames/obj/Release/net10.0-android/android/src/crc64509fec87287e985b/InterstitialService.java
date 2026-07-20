package crc64509fec87287e985b;


public class InterstitialService
	extends com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAdLoaded:(Lcom/google/android/gms/ads/interstitial/InterstitialAd;)V:GetOnAdLoadedHandler\n" +
			"";
		mono.android.Runtime.register ("Plugin.MauiMtAdmob.Platforms.Android.InterstitialService, Plugin.MauiMtAdmob", InterstitialService.class, __md_methods);
	}

	public InterstitialService ()
	{
		super ();
		if (getClass () == InterstitialService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.InterstitialService, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public InterstitialService (crc6491bfa4099bd0c3ba.MauiMTAdmob p0)
	{
		super ();
		if (getClass () == InterstitialService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.InterstitialService, Plugin.MauiMtAdmob", "Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob", this, new java.lang.Object[] { p0 });
		}
	}

	public void onAdLoaded (com.google.android.gms.ads.interstitial.InterstitialAd p0)
	{
		n_onAdLoaded (p0);
	}

	private native void n_onAdLoaded (com.google.android.gms.ads.interstitial.InterstitialAd p0);

	public void onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0)
	{
		n_onAdFailedToLoad (p0);
	}

	private native void n_onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0);

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
