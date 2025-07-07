using System.Windows;

namespace HotelManager.Config
{
    public static class DatabaseConfig
    {
        public const string ServerName = @"(localdb)\ProjectModels";
        public const string DatabaseName = "HotelManager";
        public const bool IntegratedSecurity = true;

        public static string GetConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
}