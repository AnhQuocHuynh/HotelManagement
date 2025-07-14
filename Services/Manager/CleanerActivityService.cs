using HotelManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Services.Manager
{
    public interface ICleanerActivityService
    {
        Task<Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>> getCountNumbersOfRoomEachCleanerCleaned(DateTime start, DateTime end);
    }

    public class CleanerActivityService : ICleanerActivityService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public CleanerActivityService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>>
            getCountNumbersOfRoomEachCleanerCleaned(DateTime start, DateTime end)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                return await dbContext.Cleanings
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
}
