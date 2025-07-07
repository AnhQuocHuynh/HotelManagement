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
using HotelManager.ViewModels.ManagerViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Views.ManagerViews
{
    /// <summary>
    /// Interaction logic for EmployeesListView.xaml
    /// </summary>
    public partial class EmployeesListView : UserControl
    {
        public EmployeesListView()
        {
            InitializeComponent();
            
            // Set DataContext using DI
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<EmployeeListViewModel>();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
