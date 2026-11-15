namespace MantuGames.Views.Controls;

/// <summary>
/// One candy block, drawn like the Block Stack icon: a darker lip at the bottom,
/// the bright face on top and a small white shine in the top-left corner.
/// Empty and ghost cells stay flat.
/// </summary>
public class GlossyBlock : Grid
{
    private readonly BoxView _lip;
    private readonly BoxView _face;
    private readonly BoxView _shine;
    private Color _color = Colors.Transparent;

    public GlossyBlock(double size, Color empty)
    {
        Margin = new Thickness(1);
        double inner = Math.Max(4, size - 2);
        float radius = (float)Math.Max(3, inner * 0.24);

        _lip = new BoxView { CornerRadius = radius };
        _lipHeight = Math.Max(1.5, inner * 0.13);
        _face = new BoxView { CornerRadius = radius };
        _shine = new BoxView
        {
            Color = Colors.White,
            Opacity = 0.55,
            CornerRadius = (float)(inner * 0.07),
            WidthRequest = inner * 0.32,
            HeightRequest = Math.Max(2, inner * 0.14),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(inner * 0.15, inner * 0.13, 0, 0),
        };
        Children.Add(_lip);
        Children.Add(_face);
        Children.Add(_shine);
        InputTransparent = true;
        _flatColor = empty;
        _color = Colors.Black;   // force the first assignment through
        Color = empty;
    }

    private readonly Color _flatColor;
    private readonly double _lipHeight;

    /// <summary>Solid colours render glossy; the empty colour and translucent (ghost) colours render flat.</summary>
    public Color Color
    {
        get => _color;
        set
        {
            if (_color.Equals(value)) return;
            _color = value;
            bool flat = value.Alpha < 0.99f || value.Equals(_flatColor);
            _face.Color = value;
            _lip.Color = flat ? value : value.WithLuminosity(value.GetLuminosity() * 0.72f);
            _face.Margin = flat ? new Thickness(0) : new Thickness(0, 0, 0, _lipHeight);
            _shine.IsVisible = !flat;
        }
    }
}
