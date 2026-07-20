package crc64509fec87287e985b;


public class MyFullScreenContentCallback
	extends com.google.android.gms.ads.FullScreenContentCallback
	implements
		mono.android.IGCUserPeer
{

	public MyFullScreenContentCallback ()
	{
		super ();
		if (getClass () == MyFullScreenContentCallback.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.MyFullScreenContentCallback, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public MyFullScreenContentCallback (crc6491bfa4099bd0c3ba.MauiMTAdmob p0, int p1, crc64509fec87287e985b.AppOpenAdManager p2)
	{
		super ();
		if (getClass () == MyFullScreenContentCallback.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.MyFullScreenContentCallback, Plugin.MauiMtAdmob", "Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob:Plugin.MauiMtAdmob.Extra.AdType, Plugin.MauiMtAdmob:Plugin.MauiMtAdmob.Platforms.Android.AppOpenAdManager, Plugin.MauiMtAdmob", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}

	public void onAdDismissedFullScreenContent ()
	{
		n_onAdDismissedFullScreenContent ();
	}

	private native void n_onAdDismissedFullScreenContent ();

	public void onAdFailedToShowFullScreenContent (com.google.android.gms.ads.AdError p0)
	{
		n_onAdFailedToShowFullScreenContent (p0);
	}

	private native void n_onAdFailedToShowFullScreenContent (com.google.android.gms.ads.AdError p0);

	public void onAdShowedFullScreenContent ()
	{
		n_onAdShowedFullScreenContent ();
	}

	private native void n_onAdShowedFullScreenContent ();

	public void onAdImpression ()
	{
		n_onAdImpression ();
	}

	private native void n_onAdImpression ();

	public void onAdClicked ()
	{
		n_onAdClicked ();
	}

	private native void n_onAdClicked ();

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
