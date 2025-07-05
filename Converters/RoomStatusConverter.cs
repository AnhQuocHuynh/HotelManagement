using HotelManager.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HotelManager.Converters
{
    public class RoomStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                return status switch
                {
                    RoomStatus.Available => "Còn trống",
                    RoomStatus.UnderMaintenance => "Đang bảo trì",
                    RoomStatus.Occupied => "Đã thuê",
                    RoomStatus.Reserved => "Đã đặt trước",
                    RoomStatus.OutOfService => "Ngừng sử dụng",
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    "Còn trống" => RoomStatus.Available,
                    "Đang bảo trì" => RoomStatus.UnderMaintenance,
                    "Đã thuê" => RoomStatus.Occupied,
                    "Đã đặt trước" => RoomStatus.Reserved,
                    "Ngừng sử dụng" => RoomStatus.OutOfService,
                    _ => RoomStatus.Available
                };
            }
            return RoomStatus.Available;
        }
    }
}
