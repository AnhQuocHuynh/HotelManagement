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
using System.Windows.Shapes;
using HotelManager.Data;
using HotelManager.Services;
using HotelManager.ViewModels.StaffViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Views.StaffViews
{
    /// <summary>
    /// Interaction logic for TechnicianView.xaml
    /// </summary>
    public partial class TechnicianView : UserControl
    {
        public TechnicianView()
        {
            InitializeComponent();
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<ViewModels.StaffViewModels.TechnicianViewModel>();
            }
        }
    }
}
