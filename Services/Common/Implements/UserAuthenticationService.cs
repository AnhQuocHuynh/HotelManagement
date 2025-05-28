using HotelManager.Data.Common;
using HotelManager.Models;
using HotelManager.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.Helpers;

namespace HotelManager.Services.Common.Implements
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        UserRepository userRepository;

        public UserAuthenticationService(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<UserAccount> Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var hashedPassword = HashHelper.HashPassword(password);
            return await userRepository.FindAccountAsync(username, hashedPassword);
        }
    }
}
