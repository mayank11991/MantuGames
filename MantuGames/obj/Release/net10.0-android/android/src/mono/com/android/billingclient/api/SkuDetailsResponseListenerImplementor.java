package mono.com.android.billingclient.api;


public class SkuDetailsResponseListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.android.billingclient.api.SkuDetailsResponseListener
{

	public SkuDetailsResponseListenerImplementor ()
	{
		super ();
		if (getClass () == SkuDetailsResponseListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.BillingClient.Api.ISkuDetailsResponseListenerImplementor, Xamarin.Android.Google.BillingClient", "", this, new java.lang.Object[] {  });
		}
	}

	public void onSkuDetailsResponse (com.android.billingclient.api.BillingResult p0, java.util.List p1)
	{
		n_onSkuDetailsResponse (p0, p1);
	}

	private native void n_onSkuDetailsResponse (com.android.billingclient.api.BillingResult p0, java.util.List p1);

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
