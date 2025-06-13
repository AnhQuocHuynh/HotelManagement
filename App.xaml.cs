using System.Configuration;
using System.Data;
using System.Windows;
using HotelManager.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize database with seed data
        try
        {
            using (var context = new HotelDbContext())
            {
                HotelDbInitializer.Seed(context);
                
                // Test all accounts
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
        }
    }
}

