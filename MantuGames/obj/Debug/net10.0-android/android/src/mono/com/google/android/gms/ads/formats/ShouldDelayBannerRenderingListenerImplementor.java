package mono.com.google.android.gms.ads.formats;


public class ShouldDelayBannerRenderingListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.ads.formats.ShouldDelayBannerRenderingListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Android.Gms.Ads.Formats.IShouldDelayBannerRenderingListenerImplementor, Xamarin.GooglePlayServices.Ads.Lite", ShouldDelayBannerRenderingListenerImplementor.class, __md_methods);
	}

	public ShouldDelayBannerRenderingListenerImplementor ()
	{
		super ();
		if (getClass () == ShouldDelayBannerRenderingListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.Gms.Ads.Formats.IShouldDelayBannerRenderingListenerImplementor, Xamarin.GooglePlayServices.Ads.Lite", "", this, new java.lang.Object[] {  });
		}
	}

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
