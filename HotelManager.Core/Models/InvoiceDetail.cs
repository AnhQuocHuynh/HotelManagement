using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Models
{
    public class InvoiceDetail
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public string RoomNumber { get; set; }
        public Room Room { get; set; }
    }
}
