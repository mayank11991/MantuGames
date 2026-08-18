namespace MantuGames.Helpers;

public static class ThemeHelper
{
    public static bool IsDarkMode
    {
        get => Preferences.Get("dark_mode", true);
        set
        {
            Preferences.Set("dark_mode", value);
            ApplyTheme(value);
        }
    }

    public static void Init()
    {
        ApplyTheme(IsDarkMode);
    }

    public static void ApplyTheme(bool dark)
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
    }
}
