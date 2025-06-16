using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int? BookingEmployeeId { get; set; }
        public Employee? BookingEmployee { get; set; }
        public int? CheckInEmployeeID { get; set; }
        public Employee? CheckInEmployee { get; set; }
        public int? CheckOutEmployeeID { get; set; }
        public Employee? CheckOutEmployee { get; set; }

        public string RoomNumber { get; set; }
        public Room Room { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
    }
}
