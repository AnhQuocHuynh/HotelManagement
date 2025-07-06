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

namespace HotelManager.Views
{
    /// <summary>
    /// Interaction logic for EmployeeEditView.xaml
    /// </summary>
    public partial class EmployeeEditView : Window
    {
        public EmployeeEditView()
        {
            InitializeComponent();
<<<<<<< Updated upstream
            Loaded += EmployeeEditView_Loaded;
        }

        private void EmployeeEditView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.EmployeeEditViewModel vm)
            {
                vm.CloseAction = () =>
                {
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                };
            }
=======
>>>>>>> Stashed changes
        }
    }
}
