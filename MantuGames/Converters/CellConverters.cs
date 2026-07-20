using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MantuGames.Converters
{
    /// <summary>Returns background color for a Sudoku cell based on its state.</summary>
    public class CellBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFixed = values[0] is bool f && f;
            bool isSelected = values[1] is bool s && s;
            bool isError = values[2] is bool e && e;
            bool isHighlighted = values[3] is bool h && h;

            if (isSelected) return new SolidColorBrush(Color.FromArgb("#FFE066"));
            if (isError) return new SolidColorBrush(Color.FromArgb("#FF6B6B"));
            if (isFixed) return new SolidColorBrush(Color.FromArgb("#B8E4FF"));
            if (isHighlighted) return new SolidColorBrush(Color.FromArgb("#E8F5FF"));
            return new SolidColorBrush(Color.FromArgb("#FFFFFF")); // white
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns text color for a cell.</summary>
    public class CellTextColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFixed = values[0] is bool f && f;
            bool isError = values[1] is bool e && e;

            if (isError) return Color.FromArgb("#FFFFFF");
            if (isFixed) return Color.FromArgb("#1565C0"); // deep blue for fixed
            return Color.FromArgb("#2E7D32"); // green for player input
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Timer bar color — green → orange → red as time runs out.</summary>
    public class TimerColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                if (progress > 0.5) return Color.FromArgb("#4CAF50");
                if (progress > 0.25) return Color.FromArgb("#FF9800");
                return Color.FromArgb("#F44336");
            }

            return Color.FromArgb("#4CAF50");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Bool to visibility (IsVisible).</summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}