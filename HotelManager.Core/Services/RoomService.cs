using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManager.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Services
{
    public class RoomService : IService<Room>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoomService> _logger;

        public RoomService(IUnitOfWork unitOfWork, IServiceScopeFactory scopeFactory, ILogger<RoomService> logger)
        {
            _unitOfWork = unitOfWork;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<Room> CreateAsync(Room entity)
        {
            try
            {
                _logger.LogInformation("Creating room: {RoomNumber}, Type: {RoomType}", entity.RoomNumber, entity.RoomType);
                await _unitOfWork.Rooms.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Room created successfully. RoomNumber: {RoomNumber}", entity.RoomNumber);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating room: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogWarning("Delete by Id is not supported for Room. Id: {Id}", id);
            throw new NotSupportedException("Room entity does not support deletion by Id. Use DeleteAsync(string roomNumber) instead.");
        }

        public async Task<bool> DeleteAsync(string roomNumber)
        {
            try
            {
                _logger.LogInformation("Deleting room. RoomNumber: {RoomNumber}", roomNumber);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    var room = await dbContext.Rooms.FindAsync(roomNumber);
                    if (room == null)
                    {
                        _logger.LogWarning("Room not found for delete. RoomNumber: {RoomNumber}", roomNumber);
                        return false;
                    }
                    dbContext.Rooms.Remove(room);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Room deleted successfully. RoomNumber: {RoomNumber}", roomNumber);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting room: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _unitOfWork.Rooms.GetAllAsync();
        }

        public async Task<Room> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.Rooms.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Room not found. Id: {Id}", id);
                    throw new EntityNotFoundException("Room", id);
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting room by Id: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Room> UpdateAsync(Room entity)
        {
            try
            {
                _logger.LogInformation("Updating room. RoomNumber: {RoomNumber}", entity.RoomNumber);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                    // Get the existing room from database
                    var existingRoom = await dbContext.Rooms.FindAsync(entity.RoomNumber);
                    if (existingRoom == null)
                    {
                        _logger.LogWarning("Room not found for update. RoomNumber: {RoomNumber}", entity.RoomNumber);
                        throw new EntityNotFoundException("Room", entity.RoomNumber);
                    }

                    // Update the properties
                    existingRoom.RoomType = entity.RoomType;
                    existingRoom.RoomStatus = entity.RoomStatus;
                    existingRoom.PricePerNight = entity.PricePerNight;

                    // Update the entity in the context
                    dbContext.Rooms.Update(existingRoom);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("Room updated successfully. RoomNumber: {RoomNumber}", entity.RoomNumber);
                    return existingRoom;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating room: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> IsRoomAvailableAsync(string roomNumber)
        {
            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == roomNumber);
            var room = rooms.FirstOrDefault();
            return room != null && room.RoomStatus == RoomStatus.Available;
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsByTypeAsync(RoomType roomType)
        {
            return await _unitOfWork.Rooms.FindAsync(r => (r.RoomStatus == RoomStatus.Available) && (r.RoomType == roomType));
        }

        public async Task<List<Room>> GetAvailableRoomsAsync()
        {
            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.RoomStatus == RoomStatus.Available);
            return rooms.ToList();
        }

        public async Task<Room?> GetByRoomNumberAsync(string roomNumber)
        {
            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.RoomNumber == roomNumber);
            return rooms.FirstOrDefault();
        }
    }
}