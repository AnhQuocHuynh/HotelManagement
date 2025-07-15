using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Core.Models;
using HotelManager.Models;

namespace HotelManager.Interfaces
{
    public interface IMaintenanceService
    {
        Task<List<MaintenanceReport>> GetAllReportsAsync();
        Task UpdateReportAsync(MaintenanceReport report);
        Task AddMaintenanceAsync(Maintenance maintenance);

    }
}
