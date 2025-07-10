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
    public static IEnumerable<RoomType> RoomTypes => Enum.GetValues(typeof(RoomType)).Cast<RoomType>();
    public static IEnumerable<RoomStatus> RoomStatuses => Enum.GetValues(typeof(RoomStatus)).Cast<RoomStatus>();
    public static IEnumerable<WorkDay> WorkDays => Enum.GetValues(typeof(WorkDay)).Cast<WorkDay>();
    public static IEnumerable<WorkShift> WorkShifts => Enum.GetValues(typeof(WorkShift)).Cast<WorkShift>();
    public static IEnumerable<ScheduleStatus> ScheduleStatuses => Enum.GetValues(typeof(ScheduleStatus)).Cast<ScheduleStatus>();
    }
}
