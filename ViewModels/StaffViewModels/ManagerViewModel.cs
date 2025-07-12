using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class ManagerViewModel : BaseViewModel
    {
        private String CurrentView { get; set; }

        private Object _currentContent;
        public Object CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }

        // commands
        public IRelayCommand ShowEmployeeListCommand { get; }
        public IRelayCommand ShowReportsCommand { get; }
        public ICommand NavigateProfileCommand { get; set; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateWorkScheduleCommand { get; private set; }

        public ManagerViewModel()
        {
            // init command
            ShowEmployeeListCommand = new RelayCommand(ShowEmployeeList);
            ShowReportsCommand = new RelayCommand(ShowReports);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);
            LogoutCommand = new RelayCommand(Logout);
            NavigateWorkScheduleCommand = new RelayCommand(NavigateWorkSchedule);

            ShowReports();
        }

        private void NavigateWorkSchedule()
        {
            if (CurrentView == "WorkSchedule")
                return;

            // Dispose old content nếu có
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentContent = new Views.ManagerViews.WorkScheduleManagementView();
            CurrentView = "WorkSchedule";
        }

        private void ShowEmployeeList()
        {
            if (CurrentView == "EmployeeList")
                return;

            // Dispose old content nếu có
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentContent = new Views.ManagerViews.EmployeesListView();
            CurrentView = "EmployeeList";
        }

        private void ShowReports()
        {
            if (CurrentView == "Reports")
                return;

            // Dispose old content nếu có
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentContent = new Views.ManagerViews.ReportsBaseView();
            CurrentView = "Reports";
        }

        private void NavigateProfile()
        {
            _navigationService.NavigateTo<ProfileViewModel>();
        }

        private void Logout()
        {
            var mainVM = System.Windows.Application.Current.MainWindow?.DataContext as HotelManager.ViewModels.MainViewModel;
            mainVM?.Logout();
        }
    }
}
