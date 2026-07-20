package androidx.browser.customtabs;


public class CustomTabsClient_CustomTabsCallbackImpl
	extends androidx.browser.customtabs.CustomTabsCallback
	implements
		mono.android.IGCUserPeer
{

	public CustomTabsClient_CustomTabsCallbackImpl ()
	{
		super ();
		if (getClass () == CustomTabsClient_CustomTabsCallbackImpl.class) {
			mono.android.TypeManager.Activate ("AndroidX.Browser.CustomTabs.CustomTabsClient+CustomTabsCallbackImpl, Xamarin.AndroidX.Browser", "", this, new java.lang.Object[] {  });
		}
	}

	public void onNavigationEvent (int p0, android.os.Bundle p1)
	{
		n_onNavigationEvent (p0, p1);
	}

	private native void n_onNavigationEvent (int p0, android.os.Bundle p1);

	public void extraCallback (java.lang.String p0, android.os.Bundle p1)
	{
		n_extraCallback (p0, p1);
	}

	private native void n_extraCallback (java.lang.String p0, android.os.Bundle p1);

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
