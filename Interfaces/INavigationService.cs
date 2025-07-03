using System;
using HotelManager.ViewModels;

namespace HotelManager.Interfaces
{
    public interface INavigationService
    {
        /// <summary>
        /// ViewModel hiện tại
        /// </summary>
        BaseViewModel CurrentViewModel { get; }

        /// <summary>
        /// Sự kiện khi ViewModel thay đổi
        /// </summary>
        event Action<BaseViewModel> CurrentViewModelChanged;

        /// <summary>
        /// Điều hướng đến ViewModel theo type
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

        /// <summary>
        /// Điều hướng đến ViewModel theo type với tham số
        /// </summary>
        void NavigateTo<TViewModel>(object parameter) where TViewModel : BaseViewModel;

        /// <summary>
        /// Điều hướng đến ViewModel instance
        /// </summary>
        void NavigateTo(BaseViewModel viewModel);

        /// <summary>
        /// Quay lại ViewModel trước đó
        /// </summary>
        void GoBack();

        /// <summary>
        /// Kiểm tra có thể quay lại không
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Xóa lịch sử điều hướng
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Đăng ký ViewModel factory
        /// </summary>
        void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory) where TViewModel : BaseViewModel;
    }
} 