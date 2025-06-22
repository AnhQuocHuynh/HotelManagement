using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class PaymentService
    {
        private readonly HotelDbContext _dbContext;

        public PaymentService(HotelDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task CreateAsync(Payment payment)
        {
            if (payment.InvoiceId > 0)
            {
                var invoice = await _dbContext.Invoices.FindAsync(payment.InvoiceId);
                if (invoice != null)
                {
                    var totalPaid = await _dbContext.Payments
                        .Where(p => p.InvoiceId == payment.InvoiceId)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;
                    payment.RemainingAmount = invoice.TotalAmount - totalPaid - payment.Amount;
                }
            }
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _dbContext.Payments
                .Include(p => p.Invoice)
                .ToListAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            var existingPayment = await _dbContext.Payments.FindAsync(payment.Id);
            if (existingPayment != null)
            {
                existingPayment.PaymentDate = payment.PaymentDate;
                existingPayment.Amount = payment.Amount;
                existingPayment.PaymentMethod = payment.PaymentMethod;
                _dbContext.Entry(existingPayment).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Payment with ID {payment.Id} not found.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await _dbContext.Payments.FindAsync(id);
            if (payment != null)
            {
                _dbContext.Payments.Remove(payment);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateRemainingAmountAsync(int invoiceId)
        {
            var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
            if (invoice != null)
            {
                var totalPaid = await _dbContext.Payments
                    .Where(p => p.InvoiceId == invoiceId)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                var remaining = invoice.TotalAmount - totalPaid;

                var payments = await _dbContext.Payments
                    .Where(p => p.InvoiceId == invoiceId)
                    .ToListAsync();
                foreach (var payment in payments)
                {
                    payment.RemainingAmount = remaining;
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}