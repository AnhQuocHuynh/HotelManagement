using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Utilities;

namespace HotelManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
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
            ShowLoginCommand = new RelayCommand(_ => CurrentView = new Views.Common.LoginView());
            ShowHomeCommand = new RelayCommand(_ => CurrentView = new Views.HomeView());
            ShowAdminCommand = new RelayCommand(_ => CurrentView = new Views.AdminView());
            ShowCleanerCommand = new RelayCommand(_ => CurrentView = new Views.StaffViews.CleanerView());
            ShowTechnicianCommand = new RelayCommand(_ => CurrentView = new Views.StaffViews.TechnicianView());
            ShowReceptionistCommand = new RelayCommand(_ => CurrentView = new Views.StaffViews.ReceptionistView());
            ShowManagerCommand = new RelayCommand(_ => CurrentView = new Views.StaffViews.ManagerView());
            LogoutCommand = new RelayCommand(_ => Logout());

            // Subscribe to login success event
            ViewModels.Common.LoginViewModel.OnLoginSuccess += NavigateBasedOnUserRole;

            CurrentView = new Views.Common.LoginView();
        }

        // Navigation method based on user role
        public void NavigateBasedOnUserRole()
            {
            var currentUser = Utilities.AppSession.GetCurrentUserAccount();
            if (currentUser == null) return;

            switch (currentUser.Role)
            {
                case Models.Enums.UserRole.Admin:
                    CurrentView = new Views.AdminView();
                    break;
                case Models.Enums.UserRole.Manager:
                    CurrentView = new Views.StaffViews.ManagerView();
                    break;
                case Models.Enums.UserRole.Staff:
                    // For staff, check their position
                    if (currentUser.Employee?.Position == Models.Enums.EmployeePosition.Cleaner)
                        CurrentView = new Views.StaffViews.CleanerView();
                    else if (currentUser.Employee?.Position == Models.Enums.EmployeePosition.Technician)
                        CurrentView = new Views.StaffViews.TechnicianView();
                    else if (currentUser.Employee?.Position == Models.Enums.EmployeePosition.Receptionist)
                        CurrentView = new Views.StaffViews.ReceptionistView();
                    else
                        CurrentView = new Views.HomeView();
                    break;
                case Models.Enums.UserRole.Customer:
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