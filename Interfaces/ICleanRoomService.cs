using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Models;

namespace HotelManager.Interfaces
{
    public interface ICleanRoomService
    {
        Task<List<Room>> GetAllAsync();
        Task MarkRoomAsCleanedAsync(Room room);
        Task SendDamageReportAsync(MaintenanceReport report);



    }
}
