using HotelManager.Interfaces;
using System.Windows;
using HotelManager.Views;

namespace HotelManager.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            Window? window = null;

            if (viewModel is ViewModels.EmployeeEditViewModel)
            {
                window = new EmployeeEditView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };
            }
            else if (viewModel is ViewModels.Admin.AccountCreateViewModel)
            {
                window = new AccountCreateView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };
            }
            else
            {
                // fallback generic window
                window = new Window
                {
                    Content = new System.Windows.Controls.ContentControl { DataContext = viewModel },
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Width = 400,
                    Height = 300,
                    Title = "Dialog"
                };
            }

            return window.ShowDialog();
        }
    }
} 