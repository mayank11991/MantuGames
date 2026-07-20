using System.Globalization;

namespace MantuGames.Converters;

public class TitleFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrEmpty(text))
            return 16;

        int len = text.Length;
        if (len <= 8)  return 16;
        if (len <= 11) return 14;
        if (len <= 14) return 12;
        return 11;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
