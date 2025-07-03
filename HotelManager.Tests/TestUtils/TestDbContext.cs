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
            var dbName = databaseName ?? $"TestDB_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;
            
            var context = new HotelDbContext(options);
            
            // Ensure the database is created fresh
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            return context;
        }

        public static async Task<HotelDbContext> CreateInMemoryContextWithDataAsync(string databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            await context.Database.EnsureCreatedAsync();
            return context;
        }
    }
} 