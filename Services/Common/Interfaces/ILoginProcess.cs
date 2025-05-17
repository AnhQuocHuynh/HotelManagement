using HotelManager.Models;
using HotelManager.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Services.Common.Interfaces
{
    public interface ILoginProcess
    {
        Task<UserAccount> Login(string username, string password, UserRole role);
    }
}
