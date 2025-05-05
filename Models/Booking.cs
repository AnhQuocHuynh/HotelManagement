using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public string RoomNumber { get; set; }
        public Room Room { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
    }
}
