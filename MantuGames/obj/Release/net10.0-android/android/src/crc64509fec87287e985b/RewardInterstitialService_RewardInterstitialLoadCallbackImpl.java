package crc64509fec87287e985b;


public class RewardInterstitialService_RewardInterstitialLoadCallbackImpl
	extends com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAdLoadCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAdLoaded:(Lcom/google/android/gms/ads/rewardedinterstitial/RewardedInterstitialAd;)V:GetOnAdLoadedHandler\n" +
			"";
		mono.android.Runtime.register ("Plugin.MauiMtAdmob.Platforms.Android.RewardInterstitialService+RewardInterstitialLoadCallbackImpl, Plugin.MauiMtAdmob", RewardInterstitialService_RewardInterstitialLoadCallbackImpl.class, __md_methods);
	}

	public RewardInterstitialService_RewardInterstitialLoadCallbackImpl ()
	{
		super ();
		if (getClass () == RewardInterstitialService_RewardInterstitialLoadCallbackImpl.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.RewardInterstitialService+RewardInterstitialLoadCallbackImpl, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0)
	{
		n_onAdFailedToLoad (p0);
	}

	private native void n_onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0);

	public void onAdLoaded (com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAd p0)
	{
		n_onAdLoaded (p0);
	}

	private native void n_onAdLoaded (com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAd p0);

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
