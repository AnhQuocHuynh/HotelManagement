using HotelManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Services.Manager
{
    internal class CleanerActivityService
    {
        private readonly HotelDbContext _dbContext;
        public CleanerActivityService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>>
            getCountNumbersOfRoomEachCleanerCleaned(DateTime start, DateTime end)
        {
            return await _dbContext.Cleanings
                .Include(c => c.Room)
                .Include(c => c.Employee)
                .Where(c => c.CleaningDate >= start && c.CleaningDate <= end)
                .GroupBy(c => c.Employee.FullName)
                .Select(g => new
                {
                    Cleaner = g.Key,
                    DeluxeCount = g.Count(c => c.Room.RoomType == HotelManager.Models.Enums.RoomType.Deluxe),
                    StandardCount = g.Count(c => c.Room.RoomType == HotelManager.Models.Enums.RoomType.Standard),
                    SuiteCount = g.Count(c => c.Room.RoomType == HotelManager.Models.Enums.RoomType.Suite)
                })
                .ToDictionaryAsync(
                    x => x.Cleaner,
                    x => (x.DeluxeCount, x.StandardCount, x.SuiteCount)
                );
        }

    }
}
