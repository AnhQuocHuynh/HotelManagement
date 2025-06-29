using System.Configuration;
using System.Data;
using System.Windows;
using HotelManager.Data;
using HotelManager.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;

namespace HotelManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Khởi tạo Serilog logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Khởi tạo DI container với Serilog
        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Tích hợp Serilog vào HostBuilder
            .ConfigureServices((context, services) =>
            {
                // Đăng ký DbContext
                services.AddDbContext<HotelDbContext>(options =>
                    options.UseSqlServer(Config.DatabaseConfig.GetConnectionString()));

                // Đăng ký logger factory
                services.AddSingleton<ILoggerFactory>(sp => new Serilog.Extensions.Logging.SerilogLoggerFactory());
                services.AddLogging();

                // Đăng ký các service, repository, unit of work, viewmodel ...
                services.AddScoped<HotelManager.Interfaces.IUnitOfWork, HotelManager.Repositories.UnitOfWork>();
                services.AddScoped(typeof(HotelManager.Interfaces.IRepository<>), typeof(HotelManager.Repositories.Repository<>));
                services.AddScoped<HotelManager.Services.BookingService>();
                services.AddScoped<HotelManager.Services.RoomService>();
                services.AddScoped<HotelManager.Services.CustomerService>();
                services.AddScoped<HotelManager.Services.PaymentService>();
                services.AddScoped<HotelManager.Services.InvoiceService>();
                services.AddScoped<HotelManager.Services.CleanRoomService>();
                services.AddScoped<HotelManager.Services.MaintenanceService>();
                // ... các service khác nếu cần
                // ViewModel
                services.AddTransient<HotelManager.ViewModels.BookingViewModel>();
                services.AddTransient<HotelManager.ViewModels.MainViewModel>();
                services.AddTransient<HotelManager.ViewModels.PaymentViewModel>();
                services.AddTransient<HotelManager.ViewModels.RoomViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.CleanerViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.TechnicianViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.ManagerViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.ReceptionistViewModel>();
                services.AddTransient<HotelManager.ViewModels.AdminViewModel>();
                services.AddTransient<HotelManager.ViewModels.EmployeeEditViewModel>();
                services.AddTransient<HotelManager.ViewModels.RoomInfoEditViewModel>();
                // ... các ViewModel khác nếu cần
            })
            .Build();

        ServiceProvider = _host.Services;

        //register all ViewModel
        ViewModelRegistration.RegisterAll();
        // Initialize database with seed data
        try
        {
            using (var context = new HotelDbContext())
            {
                HotelDbInitializer.Seed(context);
                var accounts = context.UserAccounts.Include(u => u.Employee).ToList();
                string accountInfo = $"Database initialized with {accounts.Count} accounts:\n";
                foreach (var account in accounts)
                {
                    accountInfo += $"- {account.Username} ({account.Role}, {account.Employee?.Position})\n";
                }
                MessageBox.Show(accountInfo, "Database Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Error(ex, "Database initialization error");
        }
    }
}

