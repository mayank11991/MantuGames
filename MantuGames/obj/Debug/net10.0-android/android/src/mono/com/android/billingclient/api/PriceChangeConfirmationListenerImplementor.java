package mono.com.android.billingclient.api;


public class PriceChangeConfirmationListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.android.billingclient.api.PriceChangeConfirmationListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPriceChangeConfirmationResult:(Lcom/android/billingclient/api/BillingResult;)V:GetOnPriceChangeConfirmationResult_Lcom_android_billingclient_api_BillingResult_Handler:Android.BillingClient.Api.IPriceChangeConfirmationListenerInvoker, Xamarin.Android.Google.BillingClient\n" +
			"";
		mono.android.Runtime.register ("Android.BillingClient.Api.IPriceChangeConfirmationListenerImplementor, Xamarin.Android.Google.BillingClient", PriceChangeConfirmationListenerImplementor.class, __md_methods);
	}

	public PriceChangeConfirmationListenerImplementor ()
	{
		super ();
		if (getClass () == PriceChangeConfirmationListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.BillingClient.Api.IPriceChangeConfirmationListenerImplementor, Xamarin.Android.Google.BillingClient", "", this, new java.lang.Object[] {  });
		}
	}

	public void onPriceChangeConfirmationResult (com.android.billingclient.api.BillingResult p0)
	{
		n_onPriceChangeConfirmationResult (p0);
	}

	private native void n_onPriceChangeConfirmationResult (com.android.billingclient.api.BillingResult p0);

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
