using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class RoomService : IService<Room>
    {
        private readonly HotelDbContext _dbContext;

        public RoomService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Room> CreateAsync(Room entity)
        {
            _dbContext.Rooms.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.Rooms.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            _dbContext.Rooms.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _dbContext.Rooms
                .Where(r => r.IsAvailable)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<Room> GetByIdAsync(int id)
        {
            var entity = await _dbContext.Rooms.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Room with ID {id} not found.");
            }
            return entity;
        }

        public async Task<Room> UpdateAsync(Room entity)
        {
            _dbContext.Rooms.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> IsRoomAvailableAsync(string roomNumber)
        {
            var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
            return room != null && room.IsAvailable;
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsByTypeAsync(RoomType roomType)
        {
            return await _dbContext.Rooms
                .Where(r => r.IsAvailable && r.RoomType == roomType)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync()
        {
            return await _dbContext.Rooms
                .Where(r => r.IsAvailable)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }
    }
}