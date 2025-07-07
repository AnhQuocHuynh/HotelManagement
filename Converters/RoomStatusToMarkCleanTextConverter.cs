using System;
using System.Globalization;
using System.Windows.Data;
using HotelManager.Models.Enums;

namespace HotelManager.Converters
{
    public class RoomStatusToMarkCleanTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                if (status == RoomStatus.Available)
                    return "Cleaned";
                if (status == RoomStatus.Pending)
                    return "Mark Clean";
            }
            return "Mark Clean";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 