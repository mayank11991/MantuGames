using Android.Views;

namespace MantuGames.Platforms.Android;

public class GraphicsTouchListener : Java.Lang.Object, global::Android.Views.View.IOnTouchListener
{
    private readonly global::Android.Views.View _nativeView;
    private readonly Action<float, float, bool, bool, bool> _callback;

    public GraphicsTouchListener(global::Android.Views.View view, Action<float, float, bool, bool, bool> callback)
    {
        _nativeView = view;
        _callback = callback;
    }

    public bool OnTouch(global::Android.Views.View v, MotionEvent e)
    {
        float x = e.GetX();
        float y = e.GetY();

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
