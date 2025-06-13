using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<EmployeePosition> EmployeePositions => Enum.GetValues(typeof(EmployeePosition)).Cast<EmployeePosition>();
    }
}
