using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HotelManager.Exceptions;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManager.Services
{
    public class BookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CustomerService _customerService;
        private readonly HotelDbContext _dbContext;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, CustomerService customerService, HotelDbContext dbContext, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _customerService = customerService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return (await _dbContext.Bookings.Include(b => b.Customer).ToListAsync());
        }

        public async Task CreateAsync(Booking booking)
        {
            try
            {
                _logger.LogInformation("Creating booking for customer: {CustomerName}, Room: {RoomNumber}", booking.Customer?.FullName, booking.RoomNumber);
                if (booking.Customer != null)
                {
                    await _customerService.CreateAsync(booking.Customer);
                    booking.CustomerId = booking.Customer.Id;
                }
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Booking created successfully. BookingId: {BookingId}", booking.Id);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception when creating booking: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating booking: {Message}", ex.Message);
                throw new BusinessException($"Lỗi khi tạo booking: {ex.Message}", ex, "Lỗi khi tạo booking.", "BOOKING_CREATE_ERROR");
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            try
            {
                _logger.LogInformation("Updating booking. BookingId: {BookingId}", booking.Id);
                var existingBooking = await _unitOfWork.Bookings.GetByIdAsync(booking.Id);
                if (existingBooking == null)
                {
                    _logger.LogWarning("Booking not found for update. BookingId: {BookingId}", booking.Id);
                    throw new EntityNotFoundException("Booking", booking.Id);
                }
                // Update properties
                existingBooking.RoomNumber = booking.RoomNumber;
                existingBooking.RoomType = booking.RoomType;
                existingBooking.Status = booking.Status;
                existingBooking.CheckInDate = booking.CheckInDate;
                existingBooking.CheckOutDate = booking.CheckOutDate;
                // ... update các trường khác nếu cần
                await _unitOfWork.Bookings.UpdateAsync(existingBooking);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Booking updated successfully. BookingId: {BookingId}", booking.Id);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception when updating booking: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating booking: {Message}", ex.Message);
                throw new BusinessException($"Lỗi khi cập nhật booking: {ex.Message}", ex, "Lỗi khi cập nhật booking.", "BOOKING_UPDATE_ERROR");
            }
        }

        public async Task DeleteAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Deleting booking. BookingId: {BookingId}", bookingId);
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for delete. BookingId: {BookingId}", bookingId);
                    throw new EntityNotFoundException("Booking", bookingId);
                }
                await _unitOfWork.Bookings.DeleteAsync(bookingId);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Booking deleted successfully. BookingId: {BookingId}", bookingId);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception when deleting booking: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting booking: {Message}", ex.Message);
                throw new BusinessException($"Lỗi khi xóa booking: {ex.Message}", ex, "Lỗi khi xóa booking.", "BOOKING_DELETE_ERROR");
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
                .Select(g => (
                    Employee: g.Key!,
                    Revenue: g.SelectMany(b => b.Invoices).Sum(i => i.TotalAmount),
                    BookingCount: g.Count()
                ))
                .OrderByDescending(x => x.Revenue)
                .Take(topCount)
                .ToList();
        }

        /// <summary>
        /// Kiểm tra xung đột booking cho một phòng trong khoảng thời gian
        /// </summary>
        public async Task<List<Booking>> GetConflictingBookingsAsync(string roomNumber, DateTime checkInDate, DateTime checkOutDate, int? excludeBookingId = null)
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                .Where(b =>
                    b.RoomNumber == roomNumber &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    (excludeBookingId == null || b.Id != excludeBookingId) &&
                    b.CheckInDate < checkOutDate &&
                    b.CheckOutDate > checkInDate)
                .ToListAsync();
        }

        #endregion
    }
}