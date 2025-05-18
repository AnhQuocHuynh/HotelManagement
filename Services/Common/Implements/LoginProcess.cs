using HotelManager.Models;
using HotelManager.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Services.Common.Implements
{
    public class LoginProcess : ILoginProcess
    {
        private readonly IUserAuthenticationService _authenticationService; // service xử lý user (đọc DB,...)

        public LoginProcess(IUserAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // Hàm đăng nhập chính
        public async Task<UserAccount> Login(string username, string password)
        {
            // Ở đây bạn gọi service kiểm tra user/pass
            UserAccount user = await _authenticationService.Authenticate(username, password);
            if (user == null)
                throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu");

            Utilities.AppSession.CurrentUserAccount = user;

            // Trả về user nếu đăng nhập thành công
            return user;
        }
    }
}
