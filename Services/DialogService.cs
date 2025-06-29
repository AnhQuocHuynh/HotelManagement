using HotelManager.Interfaces;
using System.Windows;

namespace HotelManager.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            // Logic mở dialog cho ViewModel (giả lập, có thể mở Window tương ứng)
            // Ở đây chỉ return true để không lỗi, bạn có thể mở Window thực tế nếu cần
            return true;
        }
    }
} 