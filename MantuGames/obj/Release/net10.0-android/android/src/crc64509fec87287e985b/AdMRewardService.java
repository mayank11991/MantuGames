package crc64509fec87287e985b;


public class AdMRewardService
	extends com.google.android.gms.ads.rewarded.RewardedAdLoadCallback
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.OnUserEarnedRewardListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAdLoaded:(Lcom/google/android/gms/ads/rewarded/RewardedAd;)V:GetOnAdLoadedHandler\n" +
			"";
		mono.android.Runtime.register ("Plugin.MauiMtAdmob.Platforms.Android.AdMRewardService, Plugin.MauiMtAdmob", AdMRewardService.class, __md_methods);
	}

	public AdMRewardService ()
	{
		super ();
		if (getClass () == AdMRewardService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.AdMRewardService, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public AdMRewardService (crc6491bfa4099bd0c3ba.MauiMTAdmob p0)
	{
		super ();
		if (getClass () == AdMRewardService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.AdMRewardService, Plugin.MauiMtAdmob", "Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob", this, new java.lang.Object[] { p0 });
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

	public void onUserEarnedReward (com.google.android.gms.ads.rewarded.RewardItem p0)
	{
		n_onUserEarnedReward (p0);
	}

	private native void n_onUserEarnedReward (com.google.android.gms.ads.rewarded.RewardItem p0);

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
