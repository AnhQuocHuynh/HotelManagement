using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HotelManager.Services.Manager;
using HotelManager.ViewModels.ManagerViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace HotelManager.Views.ManagerViews
{
    /// <summary>
    /// Interaction logic for RevenueBarChartView.xaml
    /// </summary>
    public partial class RevenueReportChartView : UserControl
    {
        private RevenueReportChartViewModel ViewModel => DataContext as RevenueReportChartViewModel;

        public RevenueReportChartView()
        {
            InitializeComponent();
            
            // Set DataContext using DI
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<RevenueReportChartViewModel>();
            }
        }
    }
}
