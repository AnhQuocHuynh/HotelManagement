<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HotelManager.ViewModels;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
=======
using System.Windows;
using HotelManager.ViewModels.Admin;

namespace HotelManager.Views.AdminViews
{
    public partial class AdminView : Window
    {
        private readonly AdminViewModel _viewModel;

        public AdminView()
        {
            InitializeComponent();
            _viewModel = new AdminViewModel();
            DataContext = _viewModel;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.RefreshCommand?.Execute(null);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // TODO: Implement logout logic
                MessageBox.Show("Đăng xuất thành công!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
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
    }
} 
>>>>>>> BuiQuocBao_Br
