using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Models
{
    public class Room
    {
        public string RoomNumber { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public RoomType RoomType { get; set; } = RoomType.Standard;
        public decimal PricePerNight { get; set; } = 0m;
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public ICollection<MaintenanceReport> MaintenanceReports { get; set; }

    }
}
