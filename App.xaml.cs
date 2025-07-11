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
using System.IO;
using HotelManager.Interfaces;
using HotelManager.Services;
using MaterialDesignThemes.Wpf;
using HotelManager.Repositories;
using HotelManager.Core.Interfaces;

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

        // 📋 Enhanced Serilog configuration với multiple sinks và structured logging
        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDirectory); // Ensure Logs directory exists

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information) // Reduce Microsoft noise
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning) // Reduce EF noise
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "HotelManager")
            .Enrich.WithProperty("Version", "1.0.0")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logDirectory, "hotel-manager-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30, // Keep 30 days of logs
                fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB per file
                rollOnFileSizeLimit: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logDirectory, "hotel-manager-errors-.log"),
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90, // Keep errors for 3 months
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        // 🚨 Global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        Log.Information("🏨 Hotel Manager Application Starting...");
        Log.Information("📂 Log Directory: {LogDirectory}", logDirectory);

        // Khởi tạo DI container với enhanced Serilog
        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Tích hợp Serilog vào HostBuilder
            .ConfigureServices((context, services) =>
            {
                Log.Information("🔧 Configuring Dependency Injection Services...");

                // Đăng ký DbContext
                services.AddDbContext<HotelDbContext>(options =>
                    options.UseSqlServer(
                        Config.DatabaseConfig.GetConnectionString(),
                        sql => sql.MigrationsAssembly("HotelManager.Core")));

                // 📊 Enhanced logging services
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(dispose: true);
                });

                // 🏗️ Repository và Unit of Work
                services.AddScoped<IUnitOfWork, HotelManager.Repositories.UnitOfWork>();
                services.AddScoped(typeof(IRepository<>), typeof(HotelManager.Repositories.Repository<>));
                
                // 🔍 Monitoring & Audit Services
                services.AddScoped<HotelManager.Interfaces.IAuditService, HotelManager.Services.AuditService>();

                // 🛎️ Business Services with enhanced audit support
                services.AddScoped<BookingService>();
                services.AddScoped<CustomerService>();
                services.AddScoped<EmployeeService>();
                services.AddScoped<RoomService>();
                services.AddScoped<PaymentService>();
                services.AddScoped<InvoiceService>();
                services.AddScoped<UserAccountService>();
                services.AddScoped<HotelManager.Services.CleanRoomService>();
                services.AddScoped<HotelManager.Services.MaintenanceService>();
                services.AddScoped<HotelManager.Services.DialogService>();
                services.AddScoped<HotelManager.Interfaces.IWorkAssignmentService, HotelManager.Services.WorkAssignmentService>();
                
                // 📈 Manager Services
                services.AddScoped<HotelManager.Services.Manager.ReceptionistService>();
                services.AddScoped<HotelManager.Services.Manager.InVoiceService>();

                // 🖥️ ViewModels
                services.AddTransient<HotelManager.ViewModels.BookingViewModel>();
                services.AddTransient<HotelManager.ViewModels.MainViewModel>();
                services.AddTransient<HotelManager.ViewModels.PaymentViewModel>();
                services.AddTransient<HotelManager.ViewModels.RoomViewModel>(provider =>
                    new HotelManager.ViewModels.RoomViewModel(
                        provider.GetRequiredService<RoomService>(),
                        provider.GetRequiredService<DialogService>(),
                        provider.GetRequiredService<INavigationService>()
                    )
                );
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.CleanerViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.TechnicianViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.ManagerViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.ReceptionistViewModel>();
                services.AddTransient<HotelManager.ViewModels.StaffViewModels.ReceptionistRoomViewModel>();
                services.AddTransient<HotelManager.ViewModels.Admin.AdminViewModel>(provider =>
                    new HotelManager.ViewModels.Admin.AdminViewModel(
                        provider.GetRequiredService<EmployeeService>(),
                        provider.GetRequiredService<IDialogService>(),
                        provider.GetRequiredService<INotificationService>(),
                        provider.GetRequiredService<ILogger<HotelManager.ViewModels.Admin.AdminViewModel>>(),
                        provider,
                        provider.GetRequiredService<IUnitOfWork>(),
                        provider.GetRequiredService<INavigationService>()
                    )
                );
                services.AddTransient<HotelManager.ViewModels.EmployeeEditViewModel>();
                services.AddTransient<HotelManager.ViewModels.RoomInfoEditViewModel>();
                services.AddTransient<HotelManager.ViewModels.Common.LoginViewModel>();
                services.AddTransient<HotelManager.ViewModels.ManagerViewModels.Reports.RevenueReportChartViewModel>();
                services.AddTransient<HotelManager.ViewModels.ManagerViewModels.Reports.ReceptionistActivityReportChartViewModel>();
                services.AddTransient<HotelManager.ViewModels.ManagerViewModels.EmployeeListViewModel>();
                services.AddTransient<HotelManager.ViewModels.ProfileViewModel>();
                services.AddTransient<HotelManager.ViewModels.Dialogs.ChangePasswordDialogViewModel>();

                // TODO (Tuấn): Uncomment sau khi implement WorkScheduleService
                // services.AddScoped<HotelManager.Core.Interfaces.IWorkScheduleService, HotelManager.Core.Services.WorkScheduleService>();
                // services.AddScoped<HotelManager.Core.Interfaces.IWorkScheduleRepository, HotelManager.Core.Repositories.WorkScheduleRepository>();

                // TODO (Bảo): Uncomment sau khi implement ViewModels
                services.AddTransient<HotelManager.ViewModels.ManagerViewModels.WorkScheduleManagementViewModel>(provider =>
                    new ViewModels.ManagerViewModels.WorkScheduleManagementViewModel(
                        null,
                        provider.GetRequiredService<EmployeeService>(),
                        provider.GetRequiredService<INotificationService>(),
                        provider.GetRequiredService<ILogger<HotelManager.ViewModels.ManagerViewModels.WorkScheduleManagementViewModel>>()
                    )
                );

                services.AddTransient<HotelManager.ViewModels.StaffViewModels.MyScheduleViewModel>();

                services.AddSingleton<ICurrentUserProvider, WpfCurrentUserProvider>();

                // Interface mappings for staff services
                services.AddScoped<HotelManager.Interfaces.ICleanRoomService, HotelManager.Services.CleanRoomService>();
                services.AddScoped<HotelManager.Interfaces.IMaintenanceService, HotelManager.Services.MaintenanceService>();
                
                // Register IService<Room> interface
                services.AddScoped<HotelManager.Interfaces.IService<HotelManager.Models.Room>, HotelManager.Services.RoomService>();

                // Register new services for Phase 4
                services.AddSingleton<ISnackbarMessageQueue>(provider => new SnackbarMessageQueue(TimeSpan.FromSeconds(3)));
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<INavigationService, NavigationService>();

                services.AddScoped<IDialogService, DialogService>();

                Log.Information("✅ Dependency Injection Services Configured Successfully");
            })
            .Build();

        ServiceProvider = _host.Services;

        //register all ViewModel
        ViewModelRegistration.RegisterAll();
        
        // Initialize database with seed data
        InitializeDatabaseAsync();

        Log.Information("🚀 Hotel Manager Application Started Successfully");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("🛑 Hotel Manager Application Shutting Down...");
        
        try
        {
            _host?.Dispose();
            Log.Information("✅ Application Shutdown Completed Successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error during application shutdown");
        }
        finally
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

    private void InitializeDatabaseAsync()
    {
        try
        {
            Log.Information("🗄️ Initializing Database...");
            
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                HotelDbInitializer.Seed(context);
                var accounts = context.UserAccounts.Include(u => u.Employee).ToList();
                
                Log.Information("✅ Database initialized with {AccountCount} accounts", accounts.Count);
                
                string accountInfo = $"Database initialized with {accounts.Count} accounts:\n";
                foreach (var account in accounts)
                {
                    accountInfo += $"- {account.Username} ({account.Role}, {account.Employee?.Position})\n";
                    Log.Debug("👤 Account: {Username} ({Role}, {Position})", 
                        account.Username, account.Role, account.Employee?.Position);
                }
                
                MessageBox.Show(accountInfo, "Database Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Database initialization error");
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "🚨 FATAL: Unhandled Exception in AppDomain. IsTerminating: {IsTerminating}", e.IsTerminating);
        
        if (!e.IsTerminating)
        {
            MessageBox.Show(
                $"Unexpected error occurred: {exception?.Message}\n\nThe error has been logged.",
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "🚨 Unhandled Dispatcher Exception");
        
        MessageBox.Show(
            $"UI Error occurred: {e.Exception.Message}\n\nThe error has been logged. You can continue using the application.",
            "UI Error",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        
        e.Handled = true; // Allow application to continue
    }

    public class WpfCurrentUserProvider : ICurrentUserProvider
    {
        public string? GetCurrentUsername()
        {
            var user = AppSession.GetCurrentUserAccount();
            return user?.Username;
        }
    }
}

