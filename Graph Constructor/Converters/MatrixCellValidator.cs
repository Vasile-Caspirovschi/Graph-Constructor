using System;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Graph_Constructor.Convertes
{
    public class MatrixCellValidator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value == int.MaxValue) return "∞";
            if ((int)value == -1) return "-";
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string v = value.ToString();
            if (!string.IsNullOrEmpty(v))
            {
                if (v == "-")
                    return -1;
                Regex regex = new Regex(@"[^0-9]+$");
                if (regex.IsMatch(v))
                    return Binding.DoNothing;
                return int.Parse(v);
            }
            return int.MaxValue;
        }
    }
}
