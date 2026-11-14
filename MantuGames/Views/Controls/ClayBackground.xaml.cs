namespace MantuGames.Views.Controls;

public enum BackdropVariant
{
    Lime,
    Sky,
    Night,
    /// <summary>Gradient from <see cref="ClayBackground.AccentColor"/> down to white.</summary>
    Accent,
}

public partial class ClayBackground : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(BackdropVariant), typeof(ClayBackground),
            BackdropVariant.Lime, propertyChanged: (b, _, _) => ((ClayBackground)b).Apply());

    public BackdropVariant Variant
    {
        get => (BackdropVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly BindableProperty AccentColorProperty =
        BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(ClayBackground),
            Colors.White, propertyChanged: (b, _, _) => ((ClayBackground)b).Apply());

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public ClayBackground()
    {
        InitializeComponent();
        Apply();
        // Each screen has one backdrop: it colours the system bars whenever its page shows
        // (including when returning from another page, e.g. dark game -> light level map).
        Loaded += OnLoaded;
    }

    private Page? _page;

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplySystemBars();
        if (_page != null) return;
        Element? p = Parent;
        while (p != null && p is not Page) p = p.Parent;
        _page = p as Page;
        if (_page != null)
            _page.Appearing += (s, a) => ApplySystemBars();
    }

    public void ApplySystemBars()
    {
        switch (Variant)
        {
            case BackdropVariant.Night:
                Helpers.SystemBars.Apply(Color.FromArgb("#221B4D"), Color.FromArgb("#130F2B"), darkIcons: false);
                break;
            case BackdropVariant.Accent:
                Helpers.SystemBars.Apply(AccentColor, Color.FromArgb("#FFFFFF"), darkIcons: true);
                break;
            case BackdropVariant.Sky:
                Helpers.SystemBars.Apply(Color.FromArgb("#5AA6FF"), Color.FromArgb("#F4F8FF"), darkIcons: true);
                break;
            default:
                Helpers.SystemBars.Apply(Color.FromArgb("#E9F95A"), Color.FromArgb("#FFFFFF"), darkIcons: true);
                break;
        }
    }

    private void Apply()
    {
        string key = Variant switch
        {
            BackdropVariant.Sky => "GradSky",
            BackdropVariant.Night => "GradNight",
            _ => "GradLime",
        };
        if (Variant == BackdropVariant.Accent)
        {
            var a = AccentColor;
            Root.Background = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop(a, 0f),
                new GradientStop(a.WithAlpha(0.35f), 0.55f),
                new GradientStop(Colors.White, 1f),
            }, new Point(0, 0), new Point(0, 1));
        }
        else if (Application.Current?.Resources.TryGetValue(key, out var brush) == true && brush is Brush b)
        {
            Root.Background = b;
        }

        (Color g1, Color g2) = Variant switch
        {
            BackdropVariant.Night => (Color.FromArgb("#339B6BFF"), Color.FromArgb("#264C8DFF")),
            BackdropVariant.Sky => (Color.FromArgb("#40FFFFFF"), Color.FromArgb("#30D9F84A")),
            _ => (Color.FromArgb("#55FFFFFF"), Color.FromArgb("#3373E6FF")),
        };
        Glow1.Fill = new RadialGradientBrush(new GradientStopCollection
        {
            new GradientStop(g1, 0f), new GradientStop(Colors.Transparent, 1f),
        });
        Glow2.Fill = new RadialGradientBrush(new GradientStopCollection
        {
            new GradientStop(g2, 0f), new GradientStop(Colors.Transparent, 1f),
        });

        if (IsLoaded) ApplySystemBars();
    }
}
