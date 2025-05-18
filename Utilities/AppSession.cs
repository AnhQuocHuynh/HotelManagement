using HotelManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Utilities
{
    public static class AppSession
    {
        public static UserAccount CurrentUserAccount { get; set; }
        public static List<Room> RoomList { get; set; } = new List<Room>();
        public static List<Service> ServiceList { get; set; } = new List<Service>();

        public static void Clear()
        {
            CurrentUserAccount = null;
            RoomList.Clear();
            ServiceList.Clear();
        }
    }



}
