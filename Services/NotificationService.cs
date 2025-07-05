using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using HotelManager.Interfaces;

namespace HotelManager.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ISnackbarMessageQueue _messageQueue;

        public NotificationService(ISnackbarMessageQueue messageQueue)
        {
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        }

        public void ShowInfo(string message, int durationSeconds = 3)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(durationSeconds));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ShowInfo(message, durationSeconds));
            }
        }

        public void ShowSuccess(string message, int durationSeconds = 3)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _messageQueue.Enqueue(
                    message,
                    "OK",
                    _ => { },
                    PackIconKind.CheckCircle,
                    false,
                    true,
                    TimeSpan.FromSeconds(durationSeconds));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ShowSuccess(message, durationSeconds));
            }
        }

        public void ShowWarning(string message, int durationSeconds = 4)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _messageQueue.Enqueue(
                    message,
                    "OK",
                    _ => { },
                    PackIconKind.AlertCircle,
                    false,
                    true,
                    TimeSpan.FromSeconds(durationSeconds));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ShowWarning(message, durationSeconds));
            }
        }

        public void ShowError(string message, int durationSeconds = 5)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _messageQueue.Enqueue(
                    message,
                    "OK",
                    _ => { },
                    PackIconKind.ErrorOutline,
                    false,
                    true,
                    TimeSpan.FromSeconds(durationSeconds));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ShowError(message, durationSeconds));
            }
        }

        public void ShowActionSnackbar(string message, string actionText, Action actionCallback, int durationSeconds = 5)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _messageQueue.Enqueue(
                    message,
                    actionText,
                    (PackIconKind? _) => actionCallback(),
                    PackIconKind.Information,
                    false,
                    true,
                    TimeSpan.FromSeconds(durationSeconds));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ShowActionSnackbar(message, actionText, actionCallback, durationSeconds));
            }
        }

        public async Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Có", string noText = "Không")
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                return await Application.Current.Dispatcher.InvokeAsync(() => ShowConfirmAsync(title, message, yesText, noText)).Result;
            }

            try
            {
                var dialog = new ConfirmationDialog
                {
                    Title = title,
                    Message = message,
                    YesText = yesText,
                    NoText = noText
                };

                var result = await DialogHost.Show(dialog, "RootDialog");
                return result is bool boolResult && boolResult;
            }
            catch (Exception)
            {
                // Fallback to MessageBox if DialogHost fails
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }
        }

        public async Task ShowMessageAsync(string title, string message, string okText = "OK")
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                await Application.Current.Dispatcher.InvokeAsync(() => ShowMessageAsync(title, message, okText));
                return;
            }

            try
            {
                var dialog = new MessageDialog
                {
                    Title = title,
                    Message = message,
                    OkText = okText
                };

                await DialogHost.Show(dialog, "RootDialog");
            }
            catch (Exception)
            {
                // Fallback to MessageBox if DialogHost fails
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task<string> ShowInputAsync(string title, string message, string defaultValue = "", string placeholder = "")
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                return await Application.Current.Dispatcher.InvokeAsync(() => ShowInputAsync(title, message, defaultValue, placeholder)).Result;
            }

            try
            {
                var dialog = new InputDialog
                {
                    Title = title,
                    Message = message,
                    InputValue = defaultValue,
                    Placeholder = placeholder
                };

                var result = await DialogHost.Show(dialog, "RootDialog");
                return result as string ?? string.Empty;
            }
            catch (Exception)
            {
                // Fallback to MessageBox if DialogHost fails
                MessageBox.Show($"{message}\nVui lòng nhập giá trị mặc định: {defaultValue}", title, MessageBoxButton.OK, MessageBoxImage.Information);
                return defaultValue;
            }
        }
    }

    #region Dialog UserControls (Simple Implementation)

    // Note: These would be actual UserControls in a real implementation
    // For now, we'll create minimal implementations

    internal class ConfirmationDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string YesText { get; set; }
        public string NoText { get; set; }
    }

    internal class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string OkText { get; set; }
    }

    internal class InputDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string InputValue { get; set; }
        public string Placeholder { get; set; }
    }

    #endregion
} 