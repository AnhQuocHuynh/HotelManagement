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

namespace HotelManager.Services
{
    public class CleanRoomService : ICleanRoomService
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<CleanRoomService> _logger;

        public CleanRoomService(HotelDbContext context, ILogger<CleanRoomService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Room>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all rooms for cleaning workflow");
                var rooms = await _context.Rooms
                    .Include(r => r.Bookings)
                    .ToListAsync();

                var filteredRooms = rooms
                    .Where(r =>
                    {
                        var latestBooking = r.Bookings
                            .OrderByDescending(b => b.CheckOutDate)
                            .FirstOrDefault();

                        return latestBooking != null && latestBooking.Status == BookingStatus.CheckedOut;
                    })
                    .ToList();

                return filteredRooms;
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
                room.RoomStatus = RoomStatus.Available;
                var latestBooking = await _context.Bookings
                    .Where(b => b.RoomNumber == room.RoomNumber)
                    .OrderByDescending(b => b.CheckOutDate)
                    .FirstOrDefaultAsync();

                if (latestBooking != null)
                {
                    latestBooking.Status = BookingStatus.Pending;
                }

                _context.Rooms.Update(room);
                _context.Bookings.Update(latestBooking);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Room marked as cleaned successfully. RoomNumber: {RoomNumber}", room.RoomNumber);
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

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == report.RoomNumber);
                if (room == null)
                    throw new ArgumentException($"Room {report.RoomNumber} does not exist");

                report.ReportedDate = DateTime.Now;
                report.IsResolved = false;
                report.Room = null;

                _context.MaintenanceReports.Add(report);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Damage report sent successfully for RoomNumber: {RoomNumber}", report.RoomNumber);
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
                return await _context.MaintenanceReports
                    .OrderByDescending(r => r.ReportedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting damage reports: {Message}", ex.Message);
                throw;
            }
        }
    }
}
