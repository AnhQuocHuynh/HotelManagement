using HotelManager.Data;
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
        public ReceptionistService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, (int DeluxeCount, int StandardCount, int SuiteCount)>> getCountNumbersOfRoomEachCleanerCleaned(DateTime start, DateTime end)
        {

        }
    }
}
