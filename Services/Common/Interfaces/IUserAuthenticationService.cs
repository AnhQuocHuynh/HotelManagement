using HotelManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Services.Common.Interfaces
{
    public interface IUserAuthenticationService
    {
        // tìm name và password torgn db
        // trả về UserAccount nếu tìm thấy, trả về null nếu ko thấy
        Task<UserAccount> Authenticate(string username, string password);
    }
}
