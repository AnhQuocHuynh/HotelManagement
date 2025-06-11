using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
            try
            {
                System.Diagnostics.Debug.WriteLine("MaintenanceService: Starting GetAllReportsAsync...");
                
                // Lấy tất cả báo cáo (cả resolved và unresolved) theo yêu cầu mới
                var reportsWithRoom = await _context.MaintenanceReports
                    .Include(r => r.Room)
                    .OrderByDescending(r => r.ReportedDate) // Báo cáo mới nhất trước
                    .ThenBy(r => r.IsResolved) // Báo cáo chưa sửa lên trước
                    .ToListAsync();
                
                System.Diagnostics.Debug.WriteLine($"MaintenanceService: Found {reportsWithRoom.Count} reports with Room data");
                
                return reportsWithRoom;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MaintenanceService Error: {ex.Message}");
                
                // Fallback: thử query đơn giản không Include
                try
                {
                    System.Diagnostics.Debug.WriteLine("MaintenanceService: Trying fallback query without Include...");
                    return await _context.MaintenanceReports
                        .OrderByDescending(r => r.ReportedDate)
                        .ThenBy(r => r.IsResolved)
                        .ToListAsync();
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"MaintenanceService Fallback Error: {fallbackEx.Message}");
                    throw; // Re-throw the original exception
                }
            }
        }

        public async Task UpdateReportAsync(MaintenanceReport report)
        {
            report.IsResolved = true; // Đánh dấu đã sửa
            _context.MaintenanceReports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}
