using HotelManager.Interfaces;
using System.Windows;
using HotelManager.Views;
using System;

namespace HotelManager.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            Window? window = null;

            if (viewModel is ViewModels.EmployeeEditViewModel employeeEditViewModel)
            {
                window = new EmployeeEditView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };

                // Set up the close action for the view model
                employeeEditViewModel.CloseAction = () =>
                {
                    Console.WriteLine($"DialogService: CloseAction called for EmployeeEditViewModel. DialogResult: {employeeEditViewModel.DialogResult}");
                    if (employeeEditViewModel.DialogResult.HasValue)
                    {
                        window.DialogResult = employeeEditViewModel.DialogResult.Value;
                        Console.WriteLine($"DialogService: Set window.DialogResult to {employeeEditViewModel.DialogResult.Value}");
                    }
                    window.Close();
                    Console.WriteLine("DialogService: Called window.Close()");
                };
                
                Console.WriteLine("DialogService: Set up CloseAction for EmployeeEditViewModel");
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
            else if (viewModel is ViewModels.RoomInfoEditViewModel roomEditViewModel)
            {
                window = new RoomInfoEditView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };

                // Set up the close action for the view model
                roomEditViewModel.CloseAction = (result) =>
                {
                    window.DialogResult = result;
                    window.Close();
                };
            }
            else if (viewModel is ViewModels.Admin.AccountCreateViewModel accountCreateViewModel)
            {
                window = new AccountCreateView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };

                // Set up the close action for the view model
                accountCreateViewModel.CloseAction = () =>
                {
                    Console.WriteLine($"DialogService: CloseAction called for AccountCreateViewModel. DialogResult: {accountCreateViewModel.DialogResult}");
                    if (accountCreateViewModel.DialogResult.HasValue)
                    {
                        window.DialogResult = accountCreateViewModel.DialogResult.Value;
                        Console.WriteLine($"DialogService: Set window.DialogResult to {accountCreateViewModel.DialogResult.Value}");
                    }
                    window.Close();
                    Console.WriteLine("DialogService: Called window.Close()");
                };
                
                Console.WriteLine("DialogService: Set up CloseAction for AccountCreateViewModel");
            }
            else
            {
                throw new ArgumentException($"Unsupported view model type: {typeof(TViewModel).Name}");
            }

            if (window != null)
            {
                return window.ShowDialog();
            }

            return false;
        }
    }
} 