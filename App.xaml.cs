using HotelManager.Config;
using HotelManager.Data;
using HotelManager.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace HotelManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();
            optionsBuilder.UseSqlServer(DatabaseConfig.GetConnectionString());

            using (var context = new HotelDbContext(optionsBuilder.Options))
            {
                HotelDbInitializer.Seed(context);
            }
        }
    }
}
