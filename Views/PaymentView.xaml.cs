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
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Interfaces;

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
            
            // Reuse the existing PaymentViewModel created by NavigationService if available
            if (App.ServiceProvider != null)
            {
                var navService = App.ServiceProvider.GetRequiredService<INavigationService>();
                if (navService?.CurrentViewModel is PaymentViewModel vm)
                {
                    DataContext = vm; // Use the already-initialized ViewModel (with navigation parameter)
                }
                else
                {
                    // Fallback – resolve a new instance (e.g., design-time or direct view usage)
                    DataContext = App.ServiceProvider.GetRequiredService<PaymentViewModel>();
                }
            }
        }
    }
}
