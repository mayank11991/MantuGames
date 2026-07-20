package crc64509fec87287e985b;


public class MyAdBannerListener
	extends com.google.android.gms.ads.AdListener
	implements
		mono.android.IGCUserPeer
{

	public MyAdBannerListener ()
	{
		super ();
		if (getClass () == MyAdBannerListener.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.Platforms.Android.MyAdBannerListener, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAdClicked ()
	{
		n_onAdClicked ();
	}

	private native void n_onAdClicked ();

	public void onAdClosed ()
	{
		n_onAdClosed ();
	}

	private native void n_onAdClosed ();

	public void onAdImpression ()
	{
		n_onAdImpression ();
	}

	private native void n_onAdImpression ();

	public void onAdOpened ()
	{
		n_onAdOpened ();
	}

	private native void n_onAdOpened ();

	public void onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0)
	{
		n_onAdFailedToLoad (p0);
	}

	private native void n_onAdFailedToLoad (com.google.android.gms.ads.LoadAdError p0);

	public void onAdLoaded ()
	{
		n_onAdLoaded ();
	}

	private native void n_onAdLoaded ();

	public void onAdSwipeGestureClicked ()
	{
		n_onAdSwipeGestureClicked ();
	}

	private native void n_onAdSwipeGestureClicked ();

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
