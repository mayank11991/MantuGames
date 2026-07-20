package crc64509fec87287e985b;


public class RewardService_RewardLoadCallbackImpl
	extends com.google.android.gms.ads.rewarded.RewardedAdLoadCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAdFailedToLoad:(Lcom/google/android/gms/ads/LoadAdError;)V:GetOnAdFailedToLoad_Lcom_google_android_gms_ads_LoadAdError_Handler\n" +
			"n_onAdLoaded:(Lcom/google/android/gms/ads/rewarded/RewardedAd;)V:GetOnAdLoadedHandler\n" +
			"";
		mono.android.Runtime.register ("Plugin.MauiMtAdmob.Platforms.Android.RewardService+RewardLoadCallbackImpl, Plugin.MauiMtAdmob", RewardService_RewardLoadCallbackImpl.class, __md_methods);
	}

	public RewardService_RewardLoadCallbackImpl ()
	{
		super ();
		if (getClass () == RewardService_RewardLoadCallbackImpl.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.RewardService+RewardLoadCallbackImpl, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0)
	{
		n_onAdFailedToLoad (p0);
	}

	private native void n_onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0);

	public void onAdLoaded (com.google.android.gms.ads.rewarded.RewardedAd p0)
	{
		n_onAdLoaded (p0);
	}

	private native void n_onAdLoaded (com.google.android.gms.ads.rewarded.RewardedAd p0);

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
