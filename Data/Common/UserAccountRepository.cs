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
    public class UserAccountRepository
    {

        private readonly HotelDbContext _hotelDbContext;

        public UserAccountRepository()
        {
            _hotelDbContext = new HotelDbContext();
        }

        public async Task<UserAccount> FindAccountAsync(string username, string passwordHash, UserRole role)
        {
            UserAccount account = await _hotelDbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash && u.Role == role);
            return account;
        }
    }
}
