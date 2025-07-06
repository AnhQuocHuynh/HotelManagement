using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HotelManager.Services
{
    public class CustomerService
    {
        private readonly HotelDbContext _dbContext;

        public CustomerService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(Customer customer)
        {
            try
            {
                Debug.WriteLine($"CustomerService: CreateAsync started for CCCD: {customer.CCCD}");
                var existingCustomer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.CCCD == customer.CCCD);
                if (existingCustomer != null)
                {
                    Debug.WriteLine($"Customer with CCCD {customer.CCCD} already exists");
                    throw new Exception("Customer with CCCD already exists.");
                }

                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("CustomerService: CreateAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomerService: CreateAsync error - {ex.Message}");
                throw new Exception($"Lỗi khi tạo khách hàng: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Customer customer)
        {
            try
            {
                Debug.WriteLine($"CustomerService: UpdateAsync started for Customer ID: {customer.Id}, CCCD: {customer.CCCD}");
                var existingCustomer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customer.Id);
                if (existingCustomer == null)
                {
                    Debug.WriteLine("Customer not found");
                    throw new Exception("Khách hàng không tồn tại.");
                }

                existingCustomer.FullName = customer.FullName;
                existingCustomer.CCCD = customer.CCCD;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Type = customer.Type;

                await _dbContext.SaveChangesAsync();
                Debug.WriteLine("CustomerService: UpdateAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomerService: UpdateAsync error - {ex.Message}");
                throw new Exception($"Lỗi khi cập nhật khách hàng: {ex.Message}", ex);
            }
        }
    }
}