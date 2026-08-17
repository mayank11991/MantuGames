package crc64a0e0a82d0db9a07d;


public class GyroscopeListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.hardware.SensorEventListener
{

	public GyroscopeListener ()
	{
		super ();
		if (getClass () == GyroscopeListener.class) {
			mono.android.TypeManager.Activate ("Xamarin.Essentials.GyroscopeListener, Xamarin.Essentials", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAccuracyChanged (android.hardware.Sensor p0, int p1)
	{
		n_onAccuracyChanged (p0, p1);
	}

	private native void n_onAccuracyChanged (android.hardware.Sensor p0, int p1);

	public void onSensorChanged (android.hardware.SensorEvent p0)
	{
		n_onSensorChanged (p0);
	}

	private native void n_onSensorChanged (android.hardware.SensorEvent p0);

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
