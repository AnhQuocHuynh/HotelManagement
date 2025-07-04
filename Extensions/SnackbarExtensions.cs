using System;
using MaterialDesignThemes.Wpf;

namespace HotelManager.Extensions
{
    public static class SnackbarExtensions
    {
        /// <summary>
        /// Hiển thị thông báo thông tin
        /// </summary>
        public static void EnqueueInfo(this ISnackbarMessageQueue messageQueue, string message, int durationSeconds = 3)
        {
            messageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(durationSeconds));
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public static void EnqueueSuccess(this ISnackbarMessageQueue messageQueue, string message, int durationSeconds = 3)
        {
            messageQueue.Enqueue(
                message,
                "OK",
                null,
                PackIconKind.CheckCircle,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        }

        /// <summary>
        /// Hiển thị thông báo cảnh báo
        /// </summary>
        public static void EnqueueWarning(this ISnackbarMessageQueue messageQueue, string message, int durationSeconds = 4)
        {
            messageQueue.Enqueue(
                message,
                "OK",
                null,
                PackIconKind.AlertCircle,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        public static void EnqueueError(this ISnackbarMessageQueue messageQueue, string message, int durationSeconds = 5)
        {
            messageQueue.Enqueue(
                message,
                "OK",
                null,
                PackIconKind.ErrorOutline,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        }

        /// <summary>
        /// Hiển thị thông báo với action
        /// </summary>
        public static void EnqueueWithAction(this ISnackbarMessageQueue messageQueue, 
            string message, 
            string actionText, 
            Action actionCallback, 
            PackIconKind? icon = null,
            int durationSeconds = 5)
        {
            messageQueue.Enqueue(
                message,
                actionText,
                (PackIconKind? _) => actionCallback(),
                icon,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        }

        /// <summary>
        /// Hiển thị thông báo với icon tùy chỉnh
        /// </summary>
        public static void EnqueueWithIcon(this ISnackbarMessageQueue messageQueue, 
            string message, 
            PackIconKind icon, 
            int durationSeconds = 3)
        {
            messageQueue.Enqueue(
                message,
                null,
                null,
                icon,
                false,
                true,
                TimeSpan.FromSeconds(durationSeconds));
        }
    }
} 