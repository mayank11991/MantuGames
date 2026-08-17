package mono.com.android.billingclient.api;


public class PurchaseHistoryResponseListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.android.billingclient.api.PurchaseHistoryResponseListener
{

	public PurchaseHistoryResponseListenerImplementor ()
	{
		super ();
		if (getClass () == PurchaseHistoryResponseListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.BillingClient.Api.IPurchaseHistoryResponseListenerImplementor, Xamarin.Android.Google.BillingClient", "", this, new java.lang.Object[] {  });
		}
	}

	public void onPurchaseHistoryResponse (com.android.billingclient.api.BillingResult p0, java.util.List p1)
	{
		n_onPurchaseHistoryResponse (p0, p1);
	}

	private native void n_onPurchaseHistoryResponse (com.android.billingclient.api.BillingResult p0, java.util.List p1);

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
