using HotelManager.Config;
using HotelManager.Data;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Data.Common
{
    public class UserRepository
    {
        
        private readonly HotelDbContext _hotelDbContext;

        public UserRepository(HotelDbContext context)
        {
            _hotelDbContext = context;
        }

        public async Task<UserAccount> FindAccountAsync(string  username, string passwordHash)
        {
            return await _hotelDbContext.UserAccounts
                .FirstOrDefaultAsync(u=>u.Username == username && u.PasswordHash == passwordHash);
        }
    }
}
