using System.Globalization;

namespace MantuGames.Converters;

public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrEmpty(hex))
        {
            try
            {
                return Color.FromArgb(hex);
            }
            catch
            {
            }
        }

        return Color.FromArgb("#7C4DFF");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}