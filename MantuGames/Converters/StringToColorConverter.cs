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
                var color = Color.FromArgb(hex.StartsWith('#') ? hex : "#" + hex);
                if (parameter != null && color != null)
                {
                    var raw = parameter.ToString();
                    if (byte.TryParse(raw.TrimStart('#'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var alpha))
                    {
                        color = Color.FromArgb($"#{alpha:X2}{color.ToHex().Substring(1)}");
                    }
                }
                return color;
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