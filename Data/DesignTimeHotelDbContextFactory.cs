using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using HotelManager.Core.Config;

namespace HotelManager.Data
{
    public class DesignTimeHotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
    {
        public HotelDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();
            optionsBuilder.UseSqlServer(DatabaseConfig.GetConnectionString());

            return new HotelDbContext(optionsBuilder.Options);
        }
    }
} 