package mono.com.google.android.gms.ads.rewarded;


public class OnAdMetadataChangedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.rewarded.OnAdMetadataChangedListener
{

	public OnAdMetadataChangedListenerImplementor ()
	{
		super ();
		if (getClass () == OnAdMetadataChangedListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.Gms.Ads.Rewarded.IOnAdMetadataChangedListenerImplementor, Xamarin.GooglePlayServices.Ads.Lite", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAdMetadataChanged ()
	{
		n_onAdMetadataChanged ();
	}

	private native void n_onAdMetadataChanged ();

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
