namespace HotelManager.Config
{
    public static class DatabaseConfig
    {
        // TODO: Thay đổi tên server cho phù hợp với máy của bạn
        public const string ServerName = "YOUR_SERVER_NAME\\SQLEXPRESS";
        public const string DatabaseName = "HotelManager";
        public const bool IntegratedSecurity = true;

        public static string GetConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
} 