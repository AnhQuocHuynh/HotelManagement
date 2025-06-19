using HotelManager.ViewModels;
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

namespace HotelManager.Views
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("PaymentView: UserControl_Loaded started");
                if (DataContext is PaymentViewModel viewModel)
                {
                    await viewModel.LoadDataAsync();
                    Debug.WriteLine("PaymentView: Data loaded successfully");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"PaymentView: Load error - {ex.Message}");
            }
        }
    }
}
