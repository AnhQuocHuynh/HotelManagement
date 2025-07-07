using HotelManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManager.Models.Enums;

namespace HotelManager.Services.Manager
{
    public class MaintenanceDataService
    {
        private readonly HotelDbContext _dbContext;

        public MaintenanceDataService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Đếm số phòng từng loại mà mỗi technician đã sửa
        public async Task<Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>>
            GetCountNumbersOfRoomEachTechnicianMaintained(DateTime start, DateTime end)
        {
            // Lấy danh sách tất cả technician
            var allTechnicians = await _dbContext.Employees
                .Where(e => e.Position == EmployeePosition.Technician)
                .Select(e => e.FullName)
                .ToListAsync();

            // Đếm số lần sửa theo loại phòng
            var maintenanceCounts = await _dbContext.Maintenances
                .Include(m => m.MaintenanceReport)
                    .ThenInclude(r => r.Room)
                .Include(m => m.Employee)
                .Where(m => m.RepairDate >= start && m.RepairDate <= end)
                .GroupBy(m => m.Employee.FullName)
                .Select(g => new
                {
                    TechnicianName = g.Key,
                    DeluxeCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Deluxe),
                    StandardCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Standard),
                    SuiteCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Suite)
                })
                .ToListAsync();

            // Ghép để đảm bảo technician không sửa phòng nào cũng có 0
            var result = allTechnicians.ToDictionary(
                tech => tech,
                tech =>
                {
                    var record = maintenanceCounts.FirstOrDefault(x => x.TechnicianName == tech);
                    return record != null
                        ? (record.DeluxeCount, record.StandardCount, record.SuiteCount)
                        : (0, 0, 0);
                }
            );

            return result;
        }


        // Đếm số lần từng phòng được sửa
        public async Task<Dictionary<string, (string RoomType, int MaintenanceCount)>>
            GetCountNumbersOfMaintenanceEachRoom(DateTime start, DateTime end)
        {
            // Lấy danh sách tất cả phòng
            var allRooms = await _dbContext.Rooms
                .Select(r => new { r.RoomNumber, r.RoomType })
                .ToListAsync();

            // Đếm số lần sửa của từng phòng
            var maintenanceCounts = await _dbContext.Maintenances
                .Include(m => m.MaintenanceReport)
                    .ThenInclude(r => r.Room)
                .Where(m => m.RepairDate >= start && m.RepairDate <= end)
                .GroupBy(m => m.MaintenanceReport.Room.RoomNumber)
                .Select(g => new
                {
                    RoomNumber = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Ghép để đảm bảo phòng chưa sửa lần nào cũng có 0
            var result = allRooms.ToDictionary(
                room => room.RoomNumber,
                room =>
                {
                    var record = maintenanceCounts.FirstOrDefault(x => x.RoomNumber == room.RoomNumber);
                    return (room.RoomType.ToString(), record?.Count ?? 0);
                }
            );

            return result;
        }

        public async Task<Dictionary<string, (int Deluxe, int Standard, int Suite)>> GetMonthlyMaintenanceCountsByRoomType(DateTime start, DateTime end)
        {
            var monthlyCounts = await _dbContext.Maintenances
                .Include(m => m.MaintenanceReport)
                    .ThenInclude(r => r.Room)
                .Where(m => m.RepairDate >= start && m.RepairDate <= end)
                .GroupBy(m => new { m.RepairDate.Year, m.RepairDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    DeluxeCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Deluxe),
                    StandardCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Standard),
                    SuiteCount = g.Count(m => m.MaintenanceReport.Room.RoomType == RoomType.Suite)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var result = monthlyCounts.ToDictionary(
                x => $"{x.Year}-{x.Month:D2}",
                x => (x.DeluxeCount, x.StandardCount, x.SuiteCount)
            );

            return result;
        }


    }
}
