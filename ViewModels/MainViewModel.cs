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

            ShowLoginCommand = new RelayCommand(() => CurrentView = new Views.Common.LoginView());
            ShowHomeCommand = new RelayCommand(() => CurrentView = new Views.HomeView());
            ShowAdminCommand = new RelayCommand(() => CurrentView = new Views.AdminView());
            ShowCleanerCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.CleanerView());
            ShowTechnicianCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.TechnicianView());
            ShowReceptionistCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.ReceptionistView());
            ShowManagerCommand = new RelayCommand(() => CurrentView = new Views.StaffViews.ManagerView());
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
            else if (viewModel is StaffViewModels.ManagerViewModel)
                CurrentView = new Views.StaffViews.ManagerView();
            else
                CurrentView = new Views.HomeView();
        }

        // Navigation method based on user role
        public void NavigateBasedOnUserRole()
        {
            var currentUser = Utilities.AppSession.GetCurrentUserAccount();
            if (currentUser == null)
            {
                CurrentView = new Views.Common.LoginView();
                return;
            }

            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    CurrentView = new Views.AdminView();
                    break;
                case UserRole.Manager:
                    CurrentView = new Views.StaffViews.ManagerView();
                    break;
                case UserRole.Staff:
                    // For staff, check their position
                    if (currentUser.Employee != null)
                    {
                        switch (currentUser.Employee.Position)
                        {
                            case EmployeePosition.Cleaner:
                                CurrentView = new Views.StaffViews.CleanerView();
                                break;
                            case EmployeePosition.Technician:
                                CurrentView = new Views.StaffViews.TechnicianView();
                                break;
                            case EmployeePosition.Receptionist:
                                CurrentView = new Views.StaffViews.ReceptionistView();
                                break;
                            default:
                                CurrentView = new Views.HomeView();
                                break;
                        }
                    }
                    else
                    {
                        CurrentView = new Views.HomeView();
                    }
                    break;
                case UserRole.Customer:
                    CurrentView = new Views.HomeView();
                    break;
                default:
                    CurrentView = new Views.HomeView();
                    break;
            }
        }

        // Logout method
        public void Logout()
        {
            Utilities.AppSession.Clear();
            CurrentView = new Views.Common.LoginView();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}