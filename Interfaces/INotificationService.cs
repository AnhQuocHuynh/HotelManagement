using System;
using System.Threading.Tasks;

namespace HotelManager.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Hiển thị thông báo thông tin (Snackbar)
        /// </summary>
        void ShowInfo(string message, int durationSeconds = 3);

        /// <summary>
        /// Hiển thị thông báo thành công (Snackbar)
        /// </summary>
        void ShowSuccess(string message, int durationSeconds = 3);

        /// <summary>
        /// Hiển thị thông báo cảnh báo (Snackbar)
        /// </summary>
        void ShowWarning(string message, int durationSeconds = 4);

        /// <summary>
        /// Hiển thị thông báo lỗi (Snackbar)
        /// </summary>
        void ShowError(string message, int durationSeconds = 5);

        /// <summary>
        /// Hiển thị dialog xác nhận với Yes/No
        /// </summary>
        Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Có", string noText = "Không");

        /// <summary>
        /// Hiển thị dialog thông báo với OK
        /// </summary>
        Task ShowMessageAsync(string title, string message, string okText = "OK");

        /// <summary>
        /// Hiển thị dialog nhập liệu
        /// </summary>
        Task<string> ShowInputAsync(string title, string message, string defaultValue = "", string placeholder = "");

        /// <summary>
        /// Hiển thị Snackbar với action button
        /// </summary>
        void ShowActionSnackbar(string message, string actionText, Action actionCallback, int durationSeconds = 5);
    }
} 