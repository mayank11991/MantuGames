package crc64f0146600faa7a777;


public class InternalConsumeResponseListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.android.billingclient.api.ConsumeResponseListener
{

	public InternalConsumeResponseListener ()
	{
		super ();
		if (getClass () == InternalConsumeResponseListener.class) {
			mono.android.TypeManager.Activate ("Android.BillingClient.Api.InternalConsumeResponseListener, Xamarin.Android.Google.BillingClient", "", this, new java.lang.Object[] {  });
		}
	}

	public void onConsumeResponse (com.android.billingclient.api.BillingResult p0, java.lang.String p1)
	{
		n_onConsumeResponse (p0, p1);
	}

	private native void n_onConsumeResponse (com.android.billingclient.api.BillingResult p0, java.lang.String p1);

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
