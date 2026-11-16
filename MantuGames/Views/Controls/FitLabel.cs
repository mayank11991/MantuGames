namespace MantuGames.Views.Controls;

/// <summary>
/// A single-line label that shrinks its font until the text fits, so headings never wrap
/// or get cut off on narrow phones. FontSize is the largest size it will use.
/// The space it fits into is its parent's content width, or, when <see cref="SideInset"/>
/// is set, the page width minus that inset (for labels inside auto-sized cards and tags).
/// </summary>
public class FitLabel : Label
{
    public static readonly BindableProperty SideInsetProperty =
        BindableProperty.Create(nameof(SideInset), typeof(double), typeof(FitLabel), -1d,
            propertyChanged: (b, _, _) => ((FitLabel)b).QueueFit());

    public static readonly BindableProperty MinFontSizeProperty =
        BindableProperty.Create(nameof(MinFontSize), typeof(double), typeof(FitLabel), 10d);

    /// <summary>Page width minus this is the space to fit into. Negative = use the parent's width.</summary>
    public double SideInset
    {
        get => (double)GetValue(SideInsetProperty);
        set => SetValue(SideInsetProperty, value);
    }

    public double MinFontSize
    {
        get => (double)GetValue(MinFontSizeProperty);
        set => SetValue(MinFontSizeProperty, value);
    }

    private double _maxFontSize = -1;
    private bool _fitting;
    private VisualElement? _parentHooked;
    private Page? _pageHooked;

    public FitLabel()
    {
        MaxLines = 1;
        LineBreakMode = LineBreakMode.NoWrap;
        Loaded += (_, _) => { Hook(); QueueFit(); };
        Unloaded += (_, _) => Unhook();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(FontSize) && !_fitting)
            _maxFontSize = FontSize;               // a size set from XAML/code is the ceiling
        else if (propertyName == nameof(Text) || propertyName == nameof(FontFamily))
            QueueFit();
    }

    private void Hook()
    {
        Unhook();
        _parentHooked = Parent as VisualElement;
        if (_parentHooked != null) _parentHooked.SizeChanged += OnHostSizeChanged;
        Element? e = this;
        while (e != null && e is not Page) e = e.Parent;
        _pageHooked = e as Page;
        if (_pageHooked != null) _pageHooked.SizeChanged += OnHostSizeChanged;
    }

    private void Unhook()
    {
        if (_parentHooked != null) _parentHooked.SizeChanged -= OnHostSizeChanged;
        if (_pageHooked != null) _pageHooked.SizeChanged -= OnHostSizeChanged;
        _parentHooked = null;
        _pageHooked = null;
    }

    private void OnHostSizeChanged(object? sender, EventArgs e) => QueueFit();

    private void QueueFit() => Dispatcher?.Dispatch(Fit);

    private double Available()
    {
        if (SideInset >= 0)
            return _pageHooked is { Width: > 0 } p ? p.Width - SideInset : -1;

        if (_parentHooked is not { Width: > 0 } parent) return -1;
        var pad = parent switch
        {
            Layout l => l.Padding.HorizontalThickness,
            Border b => b.Padding.HorizontalThickness + b.StrokeThickness * 2,
            ContentView c => c.Padding.HorizontalThickness,
            _ => 0,
        };
        return parent.Width - pad - Margin.HorizontalThickness;
    }

    private void Fit()
    {
        if (_fitting || Handler == null || string.IsNullOrEmpty(Text)) return;
        double avail = Available();
        if (avail <= 0) return;
        if (_maxFontSize <= 0) _maxFontSize = FontSize;

        _fitting = true;
        try
        {
            FontSize = _maxFontSize;
            double w = Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
            if (w > avail && w > 0)
                FontSize = Math.Max(MinFontSize, Math.Floor(_maxFontSize * avail / w * 0.97 * 2) / 2);
        }
        finally { _fitting = false; }
    }
}
