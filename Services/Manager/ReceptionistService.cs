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

        public async Task<Dictionary<string, (int BookingCount, int CheckInCount, int CheckOutCount)>> GetCountBookingByReceptionistAsync(DateTime start, DateTime end, String roomType)
        {
            // Lấy tất cả lễ tân
            var receptionists = await _dbContext.Employees
                .Where(e => e.Position == EmployeePosition.Receptionist)
                .ToListAsync();

            var receptionistMap = receptionists.ToDictionary(e => e.Id, e => e.FullName);

            // Đếm booking được tạo bởi lễ tân
            var bookingCreatedCounts = await _dbContext.Bookings
                .Where(b => b.BookingDate >= start && b.BookingDate <= end
                            && b.BookingEmployeeId != null
                            && b.BookingEmployee.Position == EmployeePosition.Receptionist
                            && (string.IsNullOrEmpty(roomType) || roomType == "All" || b.Room.RoomType.ToString() == roomType))
                .GroupBy(b => b.BookingEmployeeId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            // Đếm check-in bởi lễ tân
            var checkInCounts = await _dbContext.Bookings
                .Where(b => b.CheckInDate >= start && b.CheckInDate <= end
                            && b.CheckInEmployeeID != null
                            && b.CheckInEmployee.Position == EmployeePosition.Receptionist
                            && (string.IsNullOrEmpty(roomType) || roomType == "All" || b.Room.RoomType.ToString() == roomType))
                .GroupBy(b => b.CheckInEmployeeID!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            // Đếm check-out bởi lễ tân
            var checkOutCounts = await _dbContext.Bookings
                .Where(b => b.CheckOutDate >= start && b.CheckOutDate <= end
                            && b.CheckOutEmployeeID != null
                            && b.CheckOutEmployee.Position == EmployeePosition.Receptionist
                            && (string.IsNullOrEmpty(roomType) || roomType == "All" || b.Room.RoomType.ToString() == roomType))
                .GroupBy(b => b.CheckOutEmployeeID!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            // Kết hợp vào kết quả
            var result = receptionistMap.ToDictionary(
                pair => pair.Value, // FullName
                pair =>
                {
                    int created = bookingCreatedCounts.GetValueOrDefault(pair.Key);
                    int checkIn = checkInCounts.GetValueOrDefault(pair.Key);
                    int checkOut = checkOutCounts.GetValueOrDefault(pair.Key);
                    return (created, checkIn, checkOut);
                });

            return result;
        }


        public async Task<Dictionary<string, (decimal BookingRevenue, decimal CheckInRevenue, decimal CheckOutRevenue)>> GetRevenueBreakdownByReceptionistAsync(DateTime start, DateTime end, string roomType)
        {
            var receptionists = await _dbContext.Employees
                .Where(e => e.Position == EmployeePosition.Receptionist)
                .ToListAsync();

            var receptionistMap = receptionists.ToDictionary(e => e.Id, e => e.FullName);

            // Booking Revenue
            var bookingCreatedQuery = _dbContext.Bookings
                .Include(b => b.Room)
                .Include(b => b.Invoices)
                .Where(b => b.BookingEmployeeId != null && b.BookingEmployee.Position == EmployeePosition.Receptionist)
                .Where(b => b.BookingDate >= start && b.BookingDate <= end);

            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
                bookingCreatedQuery = bookingCreatedQuery.Where(b => b.Room.RoomType.ToString() == roomType);

            var bookingRevenue = await bookingCreatedQuery
                .GroupBy(b => b.BookingEmployeeId!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(b => b.Invoices.Sum(i => i.TotalAmount)) })
                .ToDictionaryAsync(x => x.Id, x => x.Total);

            // Check-In Revenue
            var checkInQuery = _dbContext.Bookings
                .Include(b => b.Room)
                .Include(b => b.Invoices)
                .Where(b => b.CheckInDate >= start && b.CheckInDate <= end)
                .Where(b => b.CheckInEmployeeID != null && b.CheckInEmployee.Position == EmployeePosition.Receptionist);

            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
                checkInQuery = checkInQuery.Where(b => b.Room.RoomType.ToString() == roomType);

            var checkInRevenue = await checkInQuery
                .GroupBy(b => b.CheckInEmployeeID!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(b => b.Invoices.Sum(i => i.TotalAmount)) })
                .ToDictionaryAsync(x => x.Id, x => x.Total);

            // Check-Out Revenue
            var checkOutQuery = _dbContext.Bookings
                .Include(b => b.Room)
                .Include(b => b.Invoices)
                .Where(b => b.CheckOutDate >= start && b.CheckOutDate <= end)
                .Where(b => b.CheckOutEmployeeID != null && b.CheckOutEmployee.Position == EmployeePosition.Receptionist);

            if (!string.IsNullOrEmpty(roomType) && roomType != "All")
                checkOutQuery = checkOutQuery.Where(b => b.Room.RoomType.ToString() == roomType);

            var checkOutRevenue = await checkOutQuery
                .GroupBy(b => b.CheckOutEmployeeID!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(b => b.Invoices.Sum(i => i.TotalAmount)) })
                .ToDictionaryAsync(x => x.Id, x => x.Total);

            // Combine all
            var result = receptionistMap.ToDictionary(
                pair => pair.Value,
                pair =>
                {
                    var id = pair.Key;
                    var booking = bookingRevenue.GetValueOrDefault(id);
                    var checkIn = checkInRevenue.GetValueOrDefault(id);
                    var checkOut = checkOutRevenue.GetValueOrDefault(id);
                    return (booking, checkIn, checkOut);
                });

            return result;
        }

    }
}
