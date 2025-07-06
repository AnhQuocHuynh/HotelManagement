using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Helpers;

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

            // Set up PasswordBox assistants
            PasswordBoxAssistant.SetBindPassword(CurrentPasswordBox, "CurrentPassword");
            PasswordBoxAssistant.SetBindPassword(NewPasswordBox, "NewPassword");
            PasswordBoxAssistant.SetBindPassword(ConfirmPasswordBox, "ConfirmPassword");
        }
    }
} 