using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels.Admin;
using MaterialDesignThemes.Wpf;
using System;

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
                // Subscribe to success event
                vm.AccountCreatedSuccessfully += OnAccountCreatedSuccessfully;
                
                // Set up close action for dialog closing
                vm.CloseAction = () =>
                {
                    Console.WriteLine("AccountCreateView: CloseAction called from view");
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                    Console.WriteLine("AccountCreateView: Dialog closed");
                };
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountCreateViewModel vm)
            {
                // Force update password from PasswordBox before executing command
                vm.ForcePasswordUpdate(PasswordBox.Password, ConfirmPasswordBox.Password);
            }
        }

        private async void OnAccountCreatedSuccessfully()
        {
            // Create success content
            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(16)
            };

            // Add success icon
            var icon = new PackIcon
            {
                Kind = PackIconKind.CheckCircle,
                Foreground = System.Windows.Media.Brushes.Green,
                Width = 24,
                Height = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };

            // Add success text
            var text = new TextBlock
            {
                Text = "Account created successfully!",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };

            content.Children.Add(icon);
            content.Children.Add(text);

            // Show the dialog
            await MainDialogHost.ShowDialog(content);
        }
    }
}