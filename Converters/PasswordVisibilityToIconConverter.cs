using System;
using System.Globalization;
using System.Windows.Data;

namespace HotelManager.Converters
{
    public class PasswordVisibilityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
                return isVisible ? "Eye" : "EyeOff";
            return "EyeOff";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 