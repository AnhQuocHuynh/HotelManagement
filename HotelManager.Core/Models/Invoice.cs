using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
