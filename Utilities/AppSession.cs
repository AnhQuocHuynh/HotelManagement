using HotelManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace HotelManager.Utilities
{
    public static class AppSession
    {
        // lưu trữ thông tin người dùng hiện tại
        private static UserAccount CurrentUserAccount { get; set; }

        public static void Clear()
        {
            CurrentUserAccount = null;
        }

        public static void SetCurrentUserAccount(UserAccount userAccount)
        {
            CurrentUserAccount = userAccount;
        }

        public static UserAccount GetCurrentUserAccount()
        {
            return CurrentUserAccount;
        }
    }



}
