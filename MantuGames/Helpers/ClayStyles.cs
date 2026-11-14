namespace MantuGames.Helpers;

/// <summary>Access to shared styles for views built in code.</summary>
public static class ClayStyles
{
    /// <summary>The "ClayFrame" Border style (replacement for the old implicit Frame look).</summary>
    public static Style? ClayFrame =>
        Application.Current?.Resources.TryGetValue("ClayFrame", out var style) == true ? style as Style : null;
}
