using System;
using System.Windows.Data;

namespace Graph_Constructor.Convertes
{
    public class IsGreaterThanZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value == 0) return "∞";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int result = 0;
            if (value != null)
                if (int.TryParse(value.ToString(), out result))
                    return result;
            return result;
        }
    }
}
