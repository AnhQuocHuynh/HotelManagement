using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models.Enums;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Services
{
    public class CleanRoomService : ICleanRoomService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanRoomService> _logger;

        public CleanRoomService(IServiceScopeFactory scopeFactory, ILogger<CleanRoomService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public async Task<List<Room>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all rooms for cleaning workflow");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    
                    // Lấy tất cả rooms có status Pending (đợi dọn dẹp)
                    var rooms = await context.Rooms
                        .Where(r => r.RoomStatus == RoomStatus.Pending)
                        .Include(r => r.Bookings)
                        .ToListAsync();

                    _logger.LogInformation("Found {RoomCount} rooms pending for cleaning", rooms.Count);
                    return rooms;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting all rooms for cleaning: {Message}", ex.Message);
                throw;
            }
        }
        public async Task MarkRoomAsCleanedAsync(Room room)
        {
            try
            {
                _logger.LogInformation("Marking room as cleaned. RoomNumber: {RoomNumber}", room.RoomNumber);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    
                    // Cập nhật room status từ Pending sang Available
                    var roomEntity = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == room.RoomNumber);
                    if (roomEntity != null)
                    {
                        roomEntity.RoomStatus = RoomStatus.Available;
                        _logger.LogInformation("Updated room {RoomNumber} status from {OldStatus} to Available", 
                            room.RoomNumber, roomEntity.RoomStatus);
                    }

                    // Cập nhật booking status thành Completed (đã hoàn thành)
                    var latestBooking = await context.Bookings
                        .Where(b => b.RoomNumber == room.RoomNumber && b.Status == BookingStatus.CheckedOut)
                        .OrderByDescending(b => b.CheckOutDate)
                        .FirstOrDefaultAsync();

                    if (latestBooking != null)
                    {
                        latestBooking.Status = BookingStatus.Completed; // Hoàn thành
                        _logger.LogInformation("Updated booking {BookingId} status to Completed", latestBooking.Id);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Room marked as cleaned successfully. RoomNumber: {RoomNumber}", room.RoomNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when marking room as cleaned: {Message}", ex.Message);
                throw;
            }
        }
        public async Task SendDamageReportAsync(MaintenanceReport report)
        {
            try
            {
                _logger.LogInformation("Sending damage report for RoomNumber: {RoomNumber}", report.RoomNumber);
                if (string.IsNullOrEmpty(report.RoomNumber))
                    throw new ArgumentException("Room number is required");
                if (string.IsNullOrEmpty(report.Description))
                    throw new ArgumentException("Description is required");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    var room = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == report.RoomNumber);
                    if (room == null)
                        throw new ArgumentException($"Room {report.RoomNumber} does not exist");

                    report.ReportedDate = DateTime.Now;
                    report.IsResolved = false;
                    report.Room = null;

                    context.MaintenanceReports.Add(report);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Damage report sent successfully for RoomNumber: {RoomNumber}", report.RoomNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when sending damage report: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<List<MaintenanceReport>> GetDamageReportsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all damage reports");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    return await context.MaintenanceReports
                        .OrderByDescending(r => r.ReportedDate)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting damage reports: {Message}", ex.Message);
                throw;
            }
        }
    }
}
