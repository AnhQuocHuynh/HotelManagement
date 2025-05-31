using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly HotelDbContext _context;

        public MaintenanceService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaintenanceReport>> GetAllReportsAsync()
        {
            return await _context.MaintenanceReports
                .Include(r => r.Room)
                .Where(r => !r.IsResolved) // Chỉ lấy các báo cáo chưa được sửa
                .OrderByDescending(r => r.ReportedDate)
                .ToListAsync();
        }

        public async Task UpdateReportAsync(MaintenanceReport report)
        {
            report.IsResolved = true; // Đánh dấu đã sửa
            _context.MaintenanceReports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}
