namespace HotelManager.Config
{
    public static class DatabaseConfig
    {
<<<<<<< Updated upstream:HotelManager.Core/Config/DatabaseConfig.example.cs
        // TODO: Thay đổi tên server cho phù hợp với máy của bạn
        public const string ServerName = "YOUR_SERVER_NAME\\SQLEXPRESS";
=======
        public const string ServerName = "LAPTOP-CUA-QUOC\\SQLEXPRESS01";
>>>>>>> Stashed changes:Config/DatabaseConfig.cs
        public const string DatabaseName = "HotelManager";
        public const bool IntegratedSecurity = true;

        public static string GetConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};Trusted_Connection={IntegratedSecurity};TrustServerCertificate=True;";
        }
    }
}