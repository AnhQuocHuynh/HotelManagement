using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using HotelManager.Data;
using HotelManager.Config;

namespace HotelManager.Data;

/// <summary>
/// Provides design-time creation of <see cref="HotelDbContext"/> so that Entity Framework Core tools
/// (e.g. dotnet ef) can create migrations without needing to run the full WPF startup pipeline.
/// </summary>
public class DesignTimeHotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();
        optionsBuilder.UseSqlServer(Config.DatabaseConfig.GetConnectionString());
        return new HotelDbContext(optionsBuilder.Options);
    }
} 