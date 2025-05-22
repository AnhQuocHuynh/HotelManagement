using HotelManager.Data.Common;
using HotelManager.Helpers;
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
        public async Task<UserAccount> Login(string username, string password, EmployeePosition position)
        {
            EmployeeAccountRepository userAccountRepository = new EmployeeAccountRepository();

            UserAccount account = await userAccountRepository.FindAccountAsync(username, HashHelper.HashPassword(password),position);

            return account;
        }
    }

}
