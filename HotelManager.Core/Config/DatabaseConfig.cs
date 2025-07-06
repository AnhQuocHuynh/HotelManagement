using System.Windows;

namespace HotelManager.Config
{
    public static class DatabaseConfig
    {
<<<<<<< HEAD
        public const string ServerName = @"(local)\SQLEXPRESS";
=======
        // Support multiple server configurations - team members can change as needed
        public const string ServerName = @"(local)\SQLEXPRESS"; // Default for BuiQuocBao, change to "(local)\SQLEXPRESS" for Quốc setup
>>>>>>> BuiQuocBao_Br
        public const string DatabaseName = "HotelManager";
        public const bool IntegratedSecurity = true;

        public static string GetConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};Trusted_Connection={IntegratedSecurity};TrustServerCertificate=True;";
        }
    }
}