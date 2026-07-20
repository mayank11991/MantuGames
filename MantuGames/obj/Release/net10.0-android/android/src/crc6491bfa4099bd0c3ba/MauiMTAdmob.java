package crc6491bfa4099bd0c3ba;


public class MauiMTAdmob
	extends com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.initialization.OnInitializationCompleteListener
{

	public MauiMTAdmob ()
	{
		super ();
		if (getClass () == MauiMTAdmob.class) {
			mono.android.TypeManager.Activate ("Plugin.MauiMtAdmob.MauiMTAdmob, Plugin.MauiMtAdmob", "", this, new java.lang.Object[] {  });
		}
	}

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
