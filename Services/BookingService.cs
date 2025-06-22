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
    }
}