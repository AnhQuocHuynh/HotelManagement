namespace HotelManager.Config
{
    public static class DatabaseConfig
    {
        public const string ServerName = "localhost"; 
        public const string DatabaseName = "HotelManager";
        public const string UserId = "sa"; 
        public const string Password = "123456"; 
        public const bool IntegratedSecurity = false; 

        public static string GetConnectionString()
        {
            if (IntegratedSecurity)
            {
                return $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            }
            return $"Server={ServerName};Database={DatabaseName};User Id={UserId};Password={Password};TrustServerCertificate=True;";
        }
    }
} 