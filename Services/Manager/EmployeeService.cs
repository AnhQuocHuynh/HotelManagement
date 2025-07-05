using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Services.Manager
{
    public class EmployeeService
    {
        private readonly HotelDbContext _dbContext;

        public EmployeeService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// Lấy danh sách tất cả nhân viên
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _dbContext.Employees
                                   .Include(e => e.UserAccount) // nếu muốn lấy kèm UserAccount
                                   .ToListAsync();
        }
    }
}
