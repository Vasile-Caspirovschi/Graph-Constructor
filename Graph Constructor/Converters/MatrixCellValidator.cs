using Graph_Constructor.Enums;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Graph_Constructor.Converters
{
    public class MatrixCellValidator : IMultiValueConverter
    {
        private GraphType _graphType;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var graphType = (GraphType)values[1];
            _graphType = graphType;
            if ((int)values[0] == -1) return "-";
            if ((int)values[0] == int.MaxValue && graphType == GraphType.Weighted) return "∞";
            return values[0].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string? v = value.ToString();
            if (!string.IsNullOrEmpty(v))
            {
                if (v == "-")
                    return new object[] { -1, _graphType };
                Regex regex = new Regex(@"[^0-9]+$");
                if (regex.IsMatch(v))
                    return null;
                if (_graphType != GraphType.Weighted)
                {
                    return new object[] { 1, _graphType };
                }
                return new object[] { v, _graphType }; 
            }
            return new object[] { _graphType == GraphType.Weighted ? int.MaxValue : 0, _graphType };
        }
    }
}
