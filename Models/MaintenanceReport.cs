using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Models
{
    public class MaintenanceReport
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public Room Room { get; set; }
        public string Description { get; set; }
        public DateTime ReportedDate { get; set; }
        public string ImagePath { get; set; } // Đường dẫn ảnh

        public bool IsResolved { get; set; } = false;
    }
}
