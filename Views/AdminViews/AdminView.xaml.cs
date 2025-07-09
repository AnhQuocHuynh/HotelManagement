using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Views
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public partial class AdminView : UserControl
    {
        public AdminView()
        {
            InitializeComponent();
            
            // Set DataContext using DI
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<AdminViewModel>();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel viewModel)
            {
                viewModel.RefreshCommand?.Execute(null);
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel viewModel)
            {
                viewModel.LogoutCommand?.Execute(null);
            }
        }

        private void btnEmployeeManagement_Click(object sender, RoutedEventArgs e)
        {
            txtContentHeader.Text = "Employee Management";
            // TODO: Navigate to employee management view
        }

        private void btnRoomManagement_Click(object sender, RoutedEventArgs e)
        {
            txtContentHeader.Text = "Room Management";
            // TODO: Navigate to room management view
        }

        private void btnAccountManagement_Click(object sender, RoutedEventArgs e)
        {
            txtContentHeader.Text = "Account Management";
            // TODO: Navigate to account management view
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            txtContentHeader.Text = "Reports";
            // TODO: Navigate to reports view
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            txtContentHeader.Text = "Settings";
            // TODO: Navigate to settings view
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel viewModel)
            {
                viewModel.ReportsCommand?.Execute(null);
            }
        }

    }
}
