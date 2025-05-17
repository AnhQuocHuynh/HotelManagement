using HotelManager.Data.Common;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Services.Common.Implements
{
    public class LoginProcess : Services.Common.Interfaces.ILoginProcess
    {
        public async Task<UserAccount> Login(string username, string password, UserRole role)
        {
            UserAccountRepository userAccountRepository = new UserAccountRepository();

            UserAccount account = await userAccountRepository.FindAccountAsync(username, password, role);

            return account;
        }
    }

}
