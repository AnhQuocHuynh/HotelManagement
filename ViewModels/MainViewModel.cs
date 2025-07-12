using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Utilities;
using HotelManager.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.ViewModels.Admin;
using HotelManager.ViewModels.StaffViewModels;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.ViewModels.Common;

namespace HotelManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private readonly INavigationService _navigationService;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowLoginCommand { get; }
        public ICommand ShowHomeCommand { get; }
        public ICommand ShowAdminCommand { get; }
        public ICommand ShowCleanerCommand { get; }
        public ICommand ShowTechnicianCommand { get; }
        public ICommand ShowReceptionistCommand { get; }
        public ICommand ShowManagerCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>();
            
            // Subscribe to navigation service events
            if (_navigationService != null)
            {
                _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
            }

            //ShowLoginCommand = new RelayCommand(() => CurrentView = new Views.Common.LoginView());
            //ShowHomeCommand = new RelayCommand(() => CurrentView = new Views.HomeView());
            //ShowAdminCommand = new RelayCommand(() => CurrentView = new Views.AdminView());
            //ShowCleanerCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.CleanerView());
            //ShowTechnicianCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.TechnicianView());
            //ShowReceptionistCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.ReceptionistView());
            //ShowManagerCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.ManagerView());

            // Refactored navigation to use NavigationService
            ShowLoginCommand = new RelayCommand(() => _navigationService?.NavigateTo<LoginViewModel>());
            ShowHomeCommand = new RelayCommand(() => _navigationService?.NavigateTo<HomeViewModel>());
            ShowAdminCommand = new RelayCommand(() => _navigationService?.NavigateTo<AdminViewModel>());
            ShowCleanerCommand = new RelayCommand(() => _navigationService?.NavigateTo<CleanerViewModel>());
            ShowTechnicianCommand = new RelayCommand(() => _navigationService?.NavigateTo<TechnicianViewModel>());
            ShowReceptionistCommand = new RelayCommand(() => _navigationService?.NavigateTo<ReceptionistViewModel>());
            ShowManagerCommand = new RelayCommand(() => _navigationService?.NavigateTo<ManagerViewModel>());

            LogoutCommand = new RelayCommand(Logout);

            // Subscribe to login success event
            ViewModels.Common.LoginViewModel.OnLoginSuccess += NavigateBasedOnUserRole;

            CurrentView = new Views.Common.LoginView();
        }

        private void OnCurrentViewModelChanged(BaseViewModel viewModel)
        {
            // Map ViewModel to View
            if (viewModel is AdminViewModel)
                CurrentView = new Views.AdminView();
            else if (viewModel is RoomViewModel)
                CurrentView = new Views.RoomView();
            else if (viewModel is BookingViewModel)
                CurrentView = new Views.BookingView();
            else if (viewModel is PaymentViewModel)
                CurrentView = new Views.PaymentView();
            else if (viewModel is StaffViewModels.CleanerViewModel)
                CurrentView = new Views.StaffViews.CleanerView();
            else if (viewModel is StaffViewModels.TechnicianViewModel)
                CurrentView = new Views.StaffViews.TechnicianView();
            else if (viewModel is StaffViewModels.ReceptionistViewModel)
                CurrentView = new Views.StaffViews.ReceptionistView();
            else if (viewModel is StaffViewModels.ReceptionistRoomViewModel)
                CurrentView = new Views.StaffViews.ReceptionistRoomView();
            else if (viewModel is StaffViewModels.ManagerViewModel)
                CurrentView = new Views.StaffViews.ManagerView();
            else if (viewModel is ProfileViewModel)
                CurrentView = new Views.ProfileView();
            else if (viewModel is LoginViewModel)
                CurrentView = new Views.Common.LoginView();
            else if (viewModel is StaffViewModels.MyScheduleViewModel)
                CurrentView = new Views.StaffViews.MyScheduleView();
            else
                CurrentView = new Views.HomeView();
        }

        // Navigation method based on user role
        public void NavigateBasedOnUserRole()
        {
            var currentUser = Utilities.AppSession.GetCurrentUserAccount();
            if (currentUser == null)
            {
                _navigationService?.NavigateTo<LoginViewModel>();
                return;
            }

            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    _navigationService?.NavigateTo<AdminViewModel>();
                    break;
                case UserRole.Manager:
                    _navigationService?.NavigateTo<StaffViewModels.ManagerViewModel>();
                    break;
                case UserRole.Staff:
                    if (currentUser.Employee != null)
                    {
                        switch (currentUser.Employee.Position)
                        {
                            case EmployeePosition.Cleaner:
                                _navigationService?.NavigateTo<StaffViewModels.CleanerViewModel>();
                                break;
                            case EmployeePosition.Technician:
                                _navigationService?.NavigateTo<StaffViewModels.TechnicianViewModel>();
                                break;
                            case EmployeePosition.Receptionist:
                                _navigationService?.NavigateTo<StaffViewModels.ReceptionistViewModel>();
                                break;
                            default:
                                _navigationService?.NavigateTo<HomeViewModel>(); // Optional fallback
                                break;
                        }
                    }
                    else
                    {
                        _navigationService?.NavigateTo<HomeViewModel>();
                    }
                    break;
                case UserRole.Customer:
                    _navigationService?.NavigateTo<HomeViewModel>();
                    break;
                default:
                    _navigationService?.NavigateTo<HomeViewModel>();
                    break;
            }
     
        }

        // Logout method
        public void Logout()
        {
            Utilities.AppSession.Clear();
            _navigationService?.ClearHistory();
            _navigationService?.NavigateTo<LoginViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}