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

        public async Task<Invoice> CreateForBookingAsync(Booking booking, decimal totalAmount)
        {
            var invoice = new Invoice
            {
                BookingId = booking.Id,
                Booking = booking,
                IssueDate = DateTime.Now,
                TotalAmount = totalAmount
            };
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
            return invoice;
        }
        public async Task<List<Payment>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            var invoiceIds = await _dbContext.Invoices
                .Where(i => i.BookingId == bookingId)
                .Select(i => i.Id)
                .ToListAsync();

            return await _dbContext.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .ToListAsync();
        }

        public async Task<Invoice> GetByBookingIdAsync(int id)
        {
            return await _dbContext.Invoices
                .Include(i => i.Booking)
                .FirstOrDefaultAsync(i => i.BookingId == id);
        }

        public async Task UpdateAsync(Invoice currentInvoice)
        {
            if (currentInvoice == null)
                throw new ArgumentNullException(nameof(currentInvoice));

            _dbContext.Invoices.Update(currentInvoice); 
            await _dbContext.SaveChangesAsync();
        }
    }
}