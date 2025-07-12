namespace HotelManager.Core.Config
{
    public static class DatabaseConfig
    {
        public const string ServerName = @"(localdb)\MSSQLLocalDB";
        public const string DatabaseName = "HotelManager";
        public const bool IntegratedSecurity = true;

        public static string GetConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};Trusted_Connection={IntegratedSecurity};TrustServerCertificate=True;";
        }
    }
}