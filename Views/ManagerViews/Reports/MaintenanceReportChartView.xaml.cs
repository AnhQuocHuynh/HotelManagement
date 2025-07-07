using HotelManager.ViewModels.ManagerViewModels.Reports;
using Microsoft.Extensions.DependencyInjection;
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

namespace HotelManager.Views.ManagerViews.Reports
{
    /// <summary>
    /// Interaction logic for MaintenanceReportChartView.xaml
    /// </summary>
    public partial class MaintenanceReportChartView : UserControl
    {
        public MaintenanceReportChartView()
        {
            InitializeComponent();

            // Set DataContext using DI
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<MaintenanceReportChartViewModel>();
            }
        }
    }
}
