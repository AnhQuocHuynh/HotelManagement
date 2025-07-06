using HotelManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Core.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int MaintenanceReportId { get; set; }
        public MaintenanceReport MaintenanceReport { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public DateTime RepairDate { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
    }
}
