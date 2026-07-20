package crc64509fec87287e985b;


public class RewardInterstitialService
	extends com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAdLoadCallback
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.OnUserEarnedRewardListener
{

	public RewardInterstitialService ()
	{
		super ();
		if (getClass () == RewardInterstitialService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.RewardInterstitialService, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public RewardInterstitialService (crc6491bfa4099bd0c3ba.MauiMTAdmob p0)
	{
		super ();
		if (getClass () == RewardInterstitialService.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.RewardInterstitialService, Plugin.MauiMtAdmob", "Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob", this, new java.lang.Object[] { p0 });
		}
	}

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
