package mono.com.google.android.gms.ads;


public class OnPaidEventListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.OnPaidEventListener
{

	public OnPaidEventListenerImplementor ()
	{
		super ();
		if (getClass () == OnPaidEventListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.Gms.Ads.IOnPaidEventListenerImplementor, Xamarin.GooglePlayServices.Ads.Lite", "", this, new java.lang.Object[] {  });
		}
	}

	public void onPaidEvent (com.google.android.gms.ads.AdValue p0)
	{
		n_onPaidEvent (p0);
	}

	private native void n_onPaidEvent (com.google.android.gms.ads.AdValue p0);

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
