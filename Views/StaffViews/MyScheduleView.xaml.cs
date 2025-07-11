using HotelManager.ViewModels.ManagerViewModels.Reports;
using HotelManager.ViewModels.StaffViewModels;
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

namespace HotelManager.Views.StaffViews
{
    /// <summary>
    /// Interaction logic for MyScheduleView.xaml
    /// </summary>
    public partial class MyScheduleView : UserControl
    {
        public MyScheduleView()
        {
            InitializeComponent();
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<MyScheduleViewModel>();
            }
        }
    }
}
