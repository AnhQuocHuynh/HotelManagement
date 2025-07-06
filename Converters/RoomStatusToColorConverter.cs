using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HotelManager.Models.Enums;

namespace HotelManager.Converters
{
    public class RoomStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                return status switch
                {
                    RoomStatus.Available => new SolidColorBrush(Colors.Green),
                    RoomStatus.Occupied => new SolidColorBrush(Colors.Red),
                    RoomStatus.Maintenance => new SolidColorBrush(Colors.Orange),
                    RoomStatus.Cleaning => new SolidColorBrush(Colors.Yellow),
                    RoomStatus.Reserved => new SolidColorBrush(Colors.Blue),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 