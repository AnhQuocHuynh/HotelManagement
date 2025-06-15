using HotelManager.Data;
using HotelManager.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace HotelManager.Services.Manager
{
    public class ReceptionistService
    {
        private readonly HotelDbContext _dbContext;
        public ReceptionistService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, (int CheckInCount, int CheckOutCount)>> GetCheckInOutByReceptionistAsync(DateTime start, DateTime end)
        {
            // Lấy tất cả lễ tân
            var receptionists = await _dbContext.Employees
                .Where(e => e.Position == EmployeePosition.Receptionist)
                .ToListAsync();

            // Lấy số lần check-in theo nhân viên
            var checkInCounts = await _dbContext.Bookings
                .Where(b => b.CheckInDate >= start && b.CheckInDate <= end)
                .Where(b => b.CheckInEmployee != null && b.CheckInEmployee.Position == EmployeePosition.Receptionist)
                .GroupBy(b => b.CheckInEmployee.FullName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            // Lấy số lần check-out theo nhân viên
            var checkOutCounts = await _dbContext.Bookings
                .Where(b => b.CheckOutDate >= start && b.CheckOutDate <= end)
                .Where(b => b.CheckOutEmployee != null && b.CheckOutEmployee.Position == EmployeePosition.Receptionist)
                .GroupBy(b => b.CheckOutEmployee.FullName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            // Kết hợp vào kết quả
            var result = new Dictionary<string, (int CheckInCount, int CheckOutCount)>();

            foreach (var receptionist in receptionists)
            {
                string name = receptionist.FullName;
                checkInCounts.TryGetValue(name, out int checkIn);
                checkOutCounts.TryGetValue(name, out int checkOut);
                result[name] = (checkIn, checkOut);
            }

            return result;
        }



    }
}
