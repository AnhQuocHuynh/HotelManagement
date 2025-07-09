using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Helpers;
using HotelManager.ViewModels.Dialogs;

namespace HotelManager.Views.Dialogs
{
    public partial class ChangePasswordDialog : Window
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
            
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<ViewModels.Dialogs.ChangePasswordDialogViewModel>();
            }

            if (DataContext is ChangePasswordDialogViewModel vm)
            {
                vm.CloseAction = this.Close;
            }

            // Set up PasswordBox assistants
            PasswordBoxAssistant.SetBindPassword(CurrentPasswordBox, "CurrentPassword");
            PasswordBoxAssistant.SetBindPassword(NewPasswordBox, "NewPassword");
            PasswordBoxAssistant.SetBindPassword(ConfirmPasswordBox, "ConfirmPassword");
        }
    }
} 