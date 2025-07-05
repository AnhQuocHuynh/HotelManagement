using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels.Admin;

namespace HotelManager.Views
{
    public partial class AccountCreateView : Window
    {
        public AccountCreateView()
        {
            InitializeComponent();
            Loaded += AccountCreateView_Loaded;
        }

        private void AccountCreateView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountCreateViewModel vm)
            {
                vm.CloseAction = () =>
                {
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                };
            }
        }
    }
}