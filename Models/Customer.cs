using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public CustomerType Type { get; set; } = CustomerType.Single;
        public ICollection<Booking> Bookings { get; set; }
    }
}
