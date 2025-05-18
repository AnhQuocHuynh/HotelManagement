using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace HotelManager.Helpers
{
    // Bui Quoc Bao get from ChatGPT
    // Hỗ trợ binding cho PasswordBox
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPassword = DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

        // getter & setter
        // cho phép dùng trong xaml
        public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPassword);
        public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPassword, value);

        //khi BoundPassword trong VM đổi thì đổi PasswordNox.Password
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = d as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                // check update để tránh vòng lặp
                if (!GetIsUpdating(passwordBox))
                {
                    passwordBox.Password = (string)e.NewValue;
                }
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }
        // PasswordBox.Password thay đổi thì gọi sự kiện, ghi lại BounPassword

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetBoundPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }

        // flag để tránh vòng lặp
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

        private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);
        private static void SetIsUpdating(DependencyObject obj, bool value) => obj.SetValue(IsUpdatingProperty, value);
    }
}
