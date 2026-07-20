using System.Globalization;

namespace MantuGames.Converters;

// Turns a card's flat hex color into a light-to-dark diagonal gradient brush
// so cards read as glossy tiles instead of flat swatches.
public class CardColorToGradientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var baseColor = value is string hex && !string.IsNullOrEmpty(hex)
            ? Color.FromArgb("#222A39")
            : Color.FromArgb("#222A39");

        var light = baseColor.AddLuminosity(0.14f);
        var dark = baseColor.AddLuminosity(-0.10f);

        return new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(light, 0f),
                new GradientStop(dark, 1f)
            },
            new Point(0, 0), new Point(1, 1));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Turns a card's flat hex color into a translucent brush of the same hue,
// used behind the card as a colored glow instead of a plain black shadow.
public class CardColorToGlowBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var baseColor = value is string hex && !string.IsNullOrEmpty(hex)
            ? Color.FromArgb("#222A39")
            : Color.FromArgb("#222A39");

        return new SolidColorBrush(baseColor.WithAlpha(0.55f));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
