using System;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace HotelManager.Extensions
{
    public static class DialogHostExtensions
    {
        /// <summary>
        /// Hiển thị dialog xác nhận
        /// </summary>
        public static async Task<bool> ShowConfirmDialogAsync(this DialogHost dialogHost, 
            string title, 
            string message, 
            string yesText = "Có", 
            string noText = "Không")
        {
            var dialog = new ConfirmDialog
            {
                Title = title,
                Message = message,
                YesText = yesText,
                NoText = noText
            };

            var result = await DialogHost.Show(dialog, dialogHost.Identifier);
            return result is bool boolResult && boolResult;
        }

        /// <summary>
        /// Hiển thị dialog thông báo
        /// </summary>
        public static async Task ShowMessageDialogAsync(this DialogHost dialogHost, 
            string title, 
            string message, 
            string okText = "OK")
        {
            var dialog = new MessageDialog
            {
                Title = title,
                Message = message,
                OkText = okText
            };

            await DialogHost.Show(dialog, dialogHost.Identifier);
        }

        /// <summary>
        /// Hiển thị dialog nhập liệu
        /// </summary>
        public static async Task<string> ShowInputDialogAsync(this DialogHost dialogHost, 
            string title, 
            string message, 
            string defaultValue = "", 
            string placeholder = "")
        {
            var dialog = new InputDialog
            {
                Title = title,
                Message = message,
                InputValue = defaultValue,
                Placeholder = placeholder
            };

            var result = await DialogHost.Show(dialog, dialogHost.Identifier);
            return result as string ?? string.Empty;
        }

        /// <summary>
        /// Hiển thị dialog tùy chỉnh
        /// </summary>
        public static async Task<object> ShowCustomDialogAsync(this DialogHost dialogHost, 
            object content, 
            string identifier = null)
        {
            return await DialogHost.Show(content, identifier ?? dialogHost.Identifier);
        }

        /// <summary>
        /// Đóng dialog hiện tại
        /// </summary>
        public static void CloseDialog(this DialogHost dialogHost, object result = null)
        {
            DialogHost.CloseDialogCommand.Execute(result, dialogHost);
        }
    }

    #region Simple Dialog Classes

    public class ConfirmDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string YesText { get; set; } = "Có";
        public string NoText { get; set; } = "Không";
    }

    public class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string OkText { get; set; } = "OK";
    }

    public class InputDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string InputValue { get; set; }
        public string Placeholder { get; set; }
    }

    #endregion
} 