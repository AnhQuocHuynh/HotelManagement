using HotelManager.Models;

namespace HotelManager.Utilities
{
    public static class AppSession
    {
        private static UserAccount? _currentUser;

        public static void SetCurrentUserAccount(UserAccount user)
        {
            _currentUser = user;
        }

        public static UserAccount? GetCurrentUserAccount()
        {
            return _currentUser;
        }

        public static void Clear()
        {
            _currentUser = null;
        }
    }
} 