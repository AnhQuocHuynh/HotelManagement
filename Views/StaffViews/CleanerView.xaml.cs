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
    /// Interaction logic for CleanerView.xaml
    /// </summary>
    public partial class CleanerView : UserControl
    {
        private readonly IServiceScope _scope;

        public CleanerView()
        {
            InitializeComponent();
            if (App.ServiceProvider != null)
            {
                _scope = App.ServiceProvider.CreateScope();
                DataContext = _scope.ServiceProvider.GetRequiredService<ViewModels.StaffViewModels.CleanerViewModel>();
            }

            // Dispose scope when the view is unloaded
            this.Unloaded += OnViewUnloaded;
        }

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            _scope?.Dispose();
        }
    }
}
