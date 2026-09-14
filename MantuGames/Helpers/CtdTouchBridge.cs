namespace MantuGames.Helpers;

public static class CtdTouchBridge
{
    public static Action<float, float> OnPointerPressed { get; set; }
    public static Action<float, float> OnPointerMoved { get; set; }
    public static Action<float, float> OnPointerReleased { get; set; }
}
