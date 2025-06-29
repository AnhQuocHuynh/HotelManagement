using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace HotelManager.Converters
{
    public class SplitImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
                return s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return new List<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> list)
                return string.Join(";", list);
            return string.Empty;
        }
    }
} 