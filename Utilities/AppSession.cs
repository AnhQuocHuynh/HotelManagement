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
        private static UserAccount CurrentUserAccount { get; set; }
        private static List<Room> RoomList { get; set; } = new List<Room>();
        private static List<Service> ServiceList { get; set; } = new List<Service>();

        public static void Clear()
        {
            CurrentUserAccount = null;
            RoomList.Clear();
            ServiceList.Clear();
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
