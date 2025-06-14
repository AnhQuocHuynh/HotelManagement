using HotelManager.ViewModels.StaffViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace HotelManager.Views.StaffViews
{
    /// <summary>
    /// Interaction logic for ReceptionistView.xaml
    /// </summary>
    public partial class ReceptionistView : UserControl
    {
        public ReceptionistView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("ReceptionistView: UserControl_Loaded started");
                if (DataContext is ReceptionistViewModel viewModel)
                {
                    await viewModel.LoadDataAsync();
                    Debug.WriteLine("ReceptionistView: Data loaded successfully");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"ReceptionistView: Load error - {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
