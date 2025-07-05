namespace HotelManager.Config
{
    public static class AppSettings
    {
        // Database settings
        public static class Database
        {
            public const string ConnectionString = "Server=LAPTOP-HR42JU06\\SQLEXPRESS;Database=HotelManager;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            public const int CommandTimeout = 30;
        }

        // API settings
        public static class Api
        {
            public const string BaseUrl = "https://api.example.com";
            public const int Timeout = 30;
        }

        // Application settings
        public static class Application
        {
            public const string Name = "Hotel Manager";
            public const string Version = "1.0.0";
            public const int DefaultPageSize = 10;
            public const string DefaultDateFormat = "dd/MM/yyyy";
            public const string DefaultTimeFormat = "HH:mm:ss";
        }

        // File paths
        public static class Paths
        {
            public const string Images = "Resources/Images";
            public const string Fonts = "Resources/Fonts";
            public const string Icons = "Resources/Icons";
            public const string Logs = "Logs";
        }
    }
} 