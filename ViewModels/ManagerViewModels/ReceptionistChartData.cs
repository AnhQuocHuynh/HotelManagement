using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class ReceptionistChartData
    {
        public string[] Labels { get; set; }
        public int[] BookingCounts { get; set; }
        public int[] CheckInCounts { get; set; }
        public int[] CheckOutCounts { get; set; }

        public decimal[] BookingRevenues { get; set; }
        public decimal[] CheckInRevenues { get; set; }
        public decimal[] CheckOutRevenues { get; set; }
    }

}
