package mono.com.android.billingclient.api;


public class BillingClientStateListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.android.billingclient.api.BillingClientStateListener
{

	public BillingClientStateListenerImplementor ()
	{
		super ();
		if (getClass () == BillingClientStateListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.BillingClient.Api.IBillingClientStateListenerImplementor, Xamarin.Android.Google.BillingClient", "", this, new java.lang.Object[] {  });
		}
	}

	public void onBillingServiceDisconnected ()
	{
		n_onBillingServiceDisconnected ();
	}

	private native void n_onBillingServiceDisconnected ();

	public void onBillingSetupFinished (com.android.billingclient.api.BillingResult p0)
	{
		n_onBillingSetupFinished (p0);
	}

	private native void n_onBillingSetupFinished (com.android.billingclient.api.BillingResult p0);

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
