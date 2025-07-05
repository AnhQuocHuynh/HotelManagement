using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HotelManager.Converters
{
    public class BooleanToResolvedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isResolved)
            {
                return isResolved ? "Đã xử lý" : "Chưa xử lý";
            }
            return "Chưa xử lý";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str == "Đã xử lý";
            }
            return false;
        }
    }
}
