using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManager.Repositories
{
    public class BookingRepository : Repository<Booking>
    {
        public BookingRepository(HotelDbContext context, ILogger<BookingRepository> logger) : base(context, logger) { }

        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .Include(b => b.BookingEmployee)
                .Include(b => b.CheckInEmployee)
                .Include(b => b.CheckOutEmployee)
                .ToListAsync();
        }

        public override async Task<Booking> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .Include(b => b.BookingEmployee)
                .Include(b => b.CheckInEmployee)
                .Include(b => b.CheckOutEmployee)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
} 