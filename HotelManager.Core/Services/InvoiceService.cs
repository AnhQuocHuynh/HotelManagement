using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class InvoiceService
    {
        private readonly HotelDbContext _dbContext;

        public InvoiceService(HotelDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _dbContext.Invoices
                .Include(i => i.Booking)
                .ToListAsync();
        }

        public async Task<Invoice> CreateForBookingAsync(int bookingId, decimal totalAmount)
        {
            var invoice = new Invoice
            {
                BookingId = bookingId,
                IssueDate = DateTime.Now,
                TotalAmount = totalAmount
            };
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
            return invoice;
        }
    }
}