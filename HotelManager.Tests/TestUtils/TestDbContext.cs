using HotelManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HotelManager.Tests.TestUtils
{
    public static class TestDbContext
    {
        public static HotelDbContext CreateInMemoryContext(string databaseName = null)
        {
            var dbName = databaseName ?? Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;
            return new HotelDbContext(options);
        }

        public static async Task<HotelDbContext> CreateInMemoryContextWithDataAsync(string databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            await context.Database.EnsureCreatedAsync();
            return context;
        }
    }
} 