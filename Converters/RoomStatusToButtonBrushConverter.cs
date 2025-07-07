using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HotelManager.Models.Enums;

namespace HotelManager.Converters
{
    public class RoomStatusToButtonBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                if (status == RoomStatus.Available)
                    return (SolidColorBrush)App.Current.FindResource("Gray300Brush");
                if (status == RoomStatus.Pending)
                    return (SolidColorBrush)App.Current.FindResource("SuccessBrush");
            }
            return (SolidColorBrush)App.Current.FindResource("Gray300Brush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 