using HotelManager.Config;
using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Data.Common
{
    public class EmployeeAccountRepository
    {

        private readonly HotelDbContext _hotelDbContext;

        public EmployeeAccountRepository()
        {
            _hotelDbContext = new HotelDbContext();
        }

        public async Task<UserAccount> FindAccountAsync(string username, string passwordHash, EmployeePosition position)
        {
            UserAccount account = await _hotelDbContext.UserAccounts
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => 
                u.Username == username && 
                u.PasswordHash == passwordHash && 
                u.Employee.Position == position
                );
            return account;
        }
    }
}
