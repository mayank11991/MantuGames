using Android.Views;
using Android.Util;

namespace MantuGames.Platforms.Android;

public class CtdNativeTouchListener : Java.Lang.Object, global::Android.Views.View.IOnTouchListener
{
    private readonly Action<float, float, bool, bool, bool> _callback;
    private readonly float _density;

    public CtdNativeTouchListener(Action<float, float, bool, bool, bool> callback)
    {
        _callback = callback;
    }

    public CtdNativeTouchListener(global::Android.Views.View hostView, Action<float, float, bool, bool, bool> callback)
    {
        _callback = callback;
        _density = hostView.Resources.DisplayMetrics.Density;
    }

    public bool OnTouch(global::Android.Views.View v, MotionEvent e)
    {
        float x = e.GetX() / _density;
        float y = e.GetY() / _density;

        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:
                _callback?.Invoke(x, y, true, false, false);
                return true;
            case MotionEventActions.Move:
                _callback?.Invoke(x, y, false, true, false);
                return true;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                _callback?.Invoke(x, y, false, false, true);
                return true;
        }
        return false;
    }
}
