using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class BookingService
    {
        private readonly HotelDbContext _dbContext;
        private readonly CustomerService _customerService;

        public BookingService(HotelDbContext dbContext, CustomerService customerService)
        {
            _dbContext = dbContext;
            _customerService = customerService;
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingEmployee)
                .Include(b => b.CheckInEmployee)
                .Include(b => b.CheckOutEmployee)
                .ToListAsync();
        }

        public async Task CreateAsync(Booking booking)
        {
            try
            {
                Debug.WriteLine("BookingService: CreateAsync started");
                if (booking.Customer != null)
                {
                    Debug.WriteLine($"Creating customer with CCCD: {booking.Customer.CCCD}");
                    await _customerService.CreateAsync(booking.Customer);
                    booking.CustomerId = booking.Customer.Id;
                }

                _dbContext.Bookings.Add(booking);

                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == booking.RoomNumber && r.RoomType == booking.RoomType);
                if (room != null)
                {
                    if (booking.Status != BookingStatus.CheckedOut && booking.Status != BookingStatus.Cancelled)
                    {
                        room.RoomStatus = RoomStatus.Occupied;
                        Debug.WriteLine($"Room {room.RoomNumber} set IsAvailable = false");
                    }
                    else
                    {
                        room.RoomStatus = RoomStatus.Available;
                        Debug.WriteLine($"Room {room.RoomNumber} set IsAvailable = true");
                    }
                }
                else
                {
                    throw new Exception($"Phòng {booking.RoomNumber} không tồn tại.");
                }

                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("BookingService: CreateAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BookingService: CreateAsync error - {ex.Message}");
                throw new Exception($"Lỗi khi tạo booking: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            try
            {
                Debug.WriteLine($"BookingService: UpdateAsync started for Booking ID: {booking.Id}");
                var existingBooking = await _dbContext.Bookings
                    .Include(b => b.Customer)
                    .FirstOrDefaultAsync(b => b.Id == booking.Id);
                if (existingBooking == null)
                {
                    Debug.WriteLine("Booking not found");
                    throw new Exception("Booking không tồn tại.");
                }

                // Cập nhật thông tin booking
                Debug.WriteLine($"Updating booking: RoomType={booking.RoomType}, RoomNumber={booking.RoomNumber}, CheckIn={booking.CheckInDate}, CheckOut={booking.CheckOutDate}, Status={booking.Status}");
                existingBooking.RoomType = booking.RoomType;
                existingBooking.RoomNumber = booking.RoomNumber;
                existingBooking.CheckInDate = booking.CheckInDate;
                existingBooking.CheckOutDate = booking.CheckOutDate;
                existingBooking.Status = booking.Status;

                // Cập nhật khách hàng
                if (booking.Customer != null && existingBooking.Customer != null)
                {
                    Debug.WriteLine($"Updating customer: ID={existingBooking.Customer.Id}, CCCD={booking.Customer.CCCD}, FullName={booking.Customer.FullName}, Phone={booking.Customer.PhoneNumber}, Type={booking.Customer.Type}");
                    if (existingBooking.Customer.Id > 0)
                    {
                        existingBooking.Customer.FullName = booking.Customer.FullName;
                        existingBooking.Customer.CCCD = booking.Customer.CCCD;
                        existingBooking.Customer.PhoneNumber = booking.Customer.PhoneNumber;
                        existingBooking.Customer.Type = booking.Customer.Type;
                        await _customerService.UpdateAsync(existingBooking.Customer);
                    }
                    else
                    {
                        Debug.WriteLine("Creating new customer for existing booking");
                        await _customerService.CreateAsync(booking.Customer);
                        existingBooking.CustomerId = booking.Customer.Id;
                    }
                }
                else
                {
                    Debug.WriteLine("No customer update required");
                }

                // Cập nhật RoomStatus của phòng
                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == booking.RoomNumber && r.RoomType == booking.RoomType);
                if (room != null)
                {
                    if (booking.Status != BookingStatus.CheckedOut && booking.Status != BookingStatus.Cancelled)
                    {
                        room.RoomStatus = RoomStatus.Occupied;
                        Debug.WriteLine($"Room {room.RoomNumber} set IsAvailable = false");
                    }
                    else
                    {
                        room.RoomStatus = RoomStatus.Available;
                        Debug.WriteLine($"Room {room.RoomNumber} set IsAvailable = true");
                    }
                }
                else
                {
                    Debug.WriteLine($"Room {booking.RoomNumber} not found");
                    throw new Exception($"Phòng {booking.RoomNumber} không tồn tại.");
                }

                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("BookingService: UpdateAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BookingService: UpdateAsync error - {ex.Message}");
                throw new Exception($"Lỗi khi cập nhật booking: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int bookingId)
        {
            try
            {
                Debug.WriteLine($"BookingService: DeleteAsync started for Booking ID: {bookingId}");
                var booking = await _dbContext.Bookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId);
                if (booking == null)
                {
                    Debug.WriteLine("Booking not found");
                    throw new Exception("Booking không tồn tại.");
                }

                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == booking.RoomNumber && r.RoomType == booking.RoomType);
                if (room != null)
                {
                    room.RoomStatus = RoomStatus.Available;
                    Debug.WriteLine($"Room {room.RoomNumber} set IsAvailable = true");
                }

                _dbContext.Bookings.Remove(booking);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("BookingService: DeleteAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BookingService: DeleteAsync error - {ex.Message}");
                throw new Exception($"Lỗi khi xóa booking: {ex.Message}", ex);
            }
        }

        #region Manager Reporting Methods

        /// <summary>
        /// Lấy các booking đã checkout để tính doanh thu
        /// </summary>
        public async Task<List<Booking>> GetCheckedOutBookingsAsync()
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingEmployee)
                .Include(b => b.CheckInEmployee)
                .Include(b => b.CheckOutEmployee)
                .Include(b => b.Invoices)
                .Where(b => b.Status == BookingStatus.CheckedOut)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy booking đã checkout theo khoảng thời gian
        /// </summary>
        public async Task<List<Booking>> GetCheckedOutBookingsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingEmployee)
                .Include(b => b.CheckInEmployee)
                .Include(b => b.CheckOutEmployee)
                .Include(b => b.Invoices)
                .Where(b => b.Status == BookingStatus.CheckedOut && 
                           b.CheckOutDate >= fromDate && 
                           b.CheckOutDate <= toDate)
                .ToListAsync();
        }

        /// <summary>
        /// Thống kê doanh số theo nhân viên (Booking Employee)
        /// </summary>
        public async Task<Dictionary<Employee, decimal>> GetRevenueByBookingEmployeeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.Bookings
                .Include(b => b.BookingEmployee)
                .Include(b => b.Invoices)
                .Where(b => b.Status == BookingStatus.CheckedOut && 
                           b.BookingEmployeeId != null);

            if (fromDate.HasValue)
                query = query.Where(b => b.CheckOutDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= toDate.Value);

            var bookings = await query.ToListAsync();

            return bookings
                .GroupBy(b => b.BookingEmployee)
                .ToDictionary(
                    g => g.Key!,
                    g => g.SelectMany(b => b.Invoices).Sum(i => i.TotalAmount)
                );
        }

        /// <summary>
        /// Thống kê doanh số theo nhân viên CheckOut
        /// </summary>
        public async Task<Dictionary<Employee, decimal>> GetRevenueByCheckOutEmployeeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.Bookings
                .Include(b => b.CheckOutEmployee)
                .Include(b => b.Invoices)
                .Where(b => b.Status == BookingStatus.CheckedOut && 
                           b.CheckOutEmployeeID != null);

            if (fromDate.HasValue)
                query = query.Where(b => b.CheckOutDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= toDate.Value);

            var bookings = await query.ToListAsync();

            return bookings
                .GroupBy(b => b.CheckOutEmployee)
                .ToDictionary(
                    g => g.Key!,
                    g => g.SelectMany(b => b.Invoices).Sum(i => i.TotalAmount)
                );
        }

        /// <summary>
        /// Thống kê số lượt đặt phòng theo thời gian
        /// </summary>
        public async Task<Dictionary<DateTime, int>> GetBookingCountByDateAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _dbContext.Bookings
                .Where(b => b.BookingDate >= fromDate && b.BookingDate <= toDate)
                .ToListAsync();

            return bookings
                .GroupBy(b => b.BookingDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Thống kê doanh thu theo thời gian (daily/monthly)
        /// </summary>
        public async Task<Dictionary<DateTime, decimal>> GetRevenueByDateAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _dbContext.Bookings
                .Include(b => b.Invoices)
                .Where(b => b.Status == BookingStatus.CheckedOut &&
                           b.CheckOutDate >= fromDate && 
                           b.CheckOutDate <= toDate)
                .ToListAsync();

            return bookings
                .GroupBy(b => b.CheckOutDate.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(b => b.Invoices).Sum(i => i.TotalAmount)
                );
        }

        /// <summary>
        /// Lấy top employees theo doanh số
        /// </summary>
        public async Task<List<(Employee Employee, decimal Revenue, int BookingCount)>> GetTopEmployeesByRevenueAsync(int topCount = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var revenueByEmployee = await GetRevenueByBookingEmployeeAsync(fromDate, toDate);
            
            var bookingCounts = await _dbContext.Bookings
                .Where(b => b.BookingEmployeeId != null && 
                           b.Status == BookingStatus.CheckedOut &&
                           (!fromDate.HasValue || b.CheckOutDate >= fromDate.Value) &&
                           (!toDate.HasValue || b.CheckOutDate <= toDate.Value))
                .GroupBy(b => b.BookingEmployeeId)
                .ToDictionaryAsync(g => g.Key!.Value, g => g.Count());

            return revenueByEmployee
                .OrderByDescending(kvp => kvp.Value)
                .Take(topCount)
                .Select(kvp => (
                    Employee: kvp.Key,
                    Revenue: kvp.Value,
                    BookingCount: bookingCounts.TryGetValue(kvp.Key.Id, out var count) ? count : 0
                ))
                .ToList();
        }

        #endregion
    }
}