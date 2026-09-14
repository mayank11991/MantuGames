using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;

namespace MantuGames.Platforms.Android;

public class GraphicsTouchListener : Java.Lang.Object, global::Android.Views.View.IOnTouchListener
{
    private readonly GraphicsView _graphicsView;

    public GraphicsTouchListener(GraphicsView gv) => _graphicsView = gv;

    public bool OnTouch(global::Android.Views.View v, MotionEvent e)
    {
        if (_graphicsView == null) return false;

        float x = e.GetX();
        float y = e.GetY();

        Console.WriteLine($"[CTD-NATIVE] Action={e.ActionMasked} ({x:F1},{y:F1})");

        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:
                MantuGames.Helpers.CtdTouchBridge.OnPointerPressed?.Invoke(x, y);
                return true;
            case MotionEventActions.Move:
                MantuGames.Helpers.CtdTouchBridge.OnPointerMoved?.Invoke(x, y);
                return true;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                MantuGames.Helpers.CtdTouchBridge.OnPointerReleased?.Invoke(x, y);
                return true;
        }
        return false;
    }
}
