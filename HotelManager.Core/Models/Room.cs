using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Models
{
    public class Room
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomStatus RoomStatus { get; set; } = RoomStatus.Available;
        public RoomType RoomType { get; set; } = RoomType.Standard;
        public decimal PricePerNight { get; set; } = 0m;
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public ICollection<MaintenanceReport> MaintenanceReports { get; set; }
        public ICollection<Cleaning> Cleanings { get; set; }


        [NotMapped] // Hiển thị status bằng tiếng Việt, không lưu vào cơ sở dữ liệu
        public string RoomStatusDisplay => RoomStatus switch
        {
            RoomStatus.Available => "Còn trống",
            RoomStatus.Occupied => "Đã thuê",
            RoomStatus.Pending => "Chờ dọn dẹp",
            RoomStatus.UnderMaintenance => "Đang bảo trì",
            RoomStatus.Reserved => "Đã đặt trước",
            RoomStatus.OutOfService => "Ngừng sử dụng",
            _ => "Không xác định"
        };

    }
}
