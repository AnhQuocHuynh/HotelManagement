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

namespace HotelManager.Services
{
    public class CleanRoomService : ICleanRoomService
    {
        private readonly HotelDbContext _context;

        public CleanRoomService(HotelDbContext context)
        {
            _context = context;
        }
        public async Task<List<Room>> GetAllAsync()
        {
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
        public async Task MarkRoomAsCleanedAsync(Room room)
        {
            room.RoomStatus = RoomStatus.Available;
            var latestBooking = await _context.Bookings
            .Where(b => b.RoomNumber == room.RoomNumber)
            .OrderByDescending(b => b.CheckOutDate)
            .FirstOrDefaultAsync();

            if (latestBooking != null)
            {
                latestBooking.Status = BookingStatus.Pending; // Set lại thành Pending như bạn yêu cầu
            }

            _context.Rooms.Update(room);
            _context.Bookings.Update(latestBooking);
            await _context.SaveChangesAsync();
        }
        public async Task SendDamageReportAsync(MaintenanceReport report)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(report.RoomNumber))
                throw new ArgumentException("Room number is required");
            
            if (string.IsNullOrEmpty(report.Description))
                throw new ArgumentException("Description is required");

            // Check if room exists
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == report.RoomNumber);
            if (room == null)
                throw new ArgumentException($"Room {report.RoomNumber} does not exist");

            // Set properties
            report.ReportedDate = DateTime.Now;
            report.IsResolved = false;
            
            // Don't set Room navigation property, just RoomNumber foreign key
            report.Room = null;

            // Add to specific DbSet
            _context.MaintenanceReports.Add(report);
            await _context.SaveChangesAsync();
        }

    }
}
