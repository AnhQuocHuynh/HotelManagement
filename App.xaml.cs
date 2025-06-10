using System.Configuration;
using System.Data;
using System.Windows;
using HotelManager.Data;

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
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

