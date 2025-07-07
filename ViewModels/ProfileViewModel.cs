using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using HotelManager.Interfaces;
using HotelManager.Exceptions;
using Microsoft.Extensions.Logging;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Helpers;
using HotelManager.Utilities;

namespace HotelManager.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly UserAccountService _userAccountService;
        private readonly EmployeeService _employeeService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ProfileViewModel> _logger;

        // Profile Properties
        private string _fullName;
        private string _username;
        private string _email;
        private string _phoneNumber;
        private string _cccd;
        private string _role;
        private string _position;
        private DateTime? _hireDate;
        private string _department;
        private DateTime? _lastLoginDate;
        private bool _isEditMode;
        private bool _isEmployee;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        public string CCCD
        {
            get => _cccd;
            set { _cccd = value; OnPropertyChanged(); }
        }

        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public string Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        public DateTime? HireDate
        {
            get => _hireDate;
            set { _hireDate = value; OnPropertyChanged(); }
        }

        public string Department
        {
            get => _department;
            set { _department = value; OnPropertyChanged(); }
        }

        public DateTime? LastLoginDate
        {
            get => _lastLoginDate;
            set { _lastLoginDate = value; OnPropertyChanged(); }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set 
            { 
                _isEditMode = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }

        public bool IsReadOnly => !IsEditMode;

        public bool IsEmployee
        {
            get => _isEmployee;
            set { _isEmployee = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand BackCommand { get; }
        public ICommand EditModeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public ProfileViewModel(UserAccountService userAccountService, EmployeeService employeeService,
            INavigationService navigationService, INotificationService notificationService, 
            ILogger<ProfileViewModel> logger)
        {
            _userAccountService = userAccountService;
            _employeeService = employeeService;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _logger = logger;

            BackCommand = new RelayCommand(Back);
            EditModeCommand = new RelayCommand(EditMode);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);
            ChangePasswordCommand = new RelayCommand(ChangePassword);

            LoadProfile();
        }

        public ProfileViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time data
                FullName = "John Doe";
                Username = "johndoe";
                Email = "john.doe@hotel.com";
                PhoneNumber = "0123456789";
                Role = "Staff";
                Position = "Receptionist";
                HireDate = DateTime.Now.AddYears(-2);
                Department = "Front Office";
                LastLoginDate = DateTime.Now.AddHours(-2);
                IsEmployee = true;
            }
            else
            {
                // Runtime initialization
                _userAccountService = App.ServiceProvider?.GetRequiredService<UserAccountService>();
                _employeeService = App.ServiceProvider?.GetRequiredService<EmployeeService>();
                _navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>();
                _notificationService = App.ServiceProvider?.GetRequiredService<INotificationService>();
                _logger = App.ServiceProvider?.GetRequiredService<ILogger<ProfileViewModel>>();

                BackCommand = new RelayCommand(Back);
                EditModeCommand = new RelayCommand(EditMode);
                SaveCommand = new AsyncRelayCommand(SaveAsync);
                CancelCommand = new RelayCommand(Cancel);
                ChangePasswordCommand = new RelayCommand(ChangePassword);

                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            try
            {
                LogInformation("Loading user profile");
                
                var currentUser = AppSession.GetCurrentUserAccount();
                if (currentUser == null)
                {
                    LogWarning("No current user found");
                    _notificationService?.ShowError("No user session found");
                    return;
                }

                // Load basic user info
                Username = currentUser.Username;
                Role = currentUser.Role.ToString();

                // Load employee info if applicable
                if (currentUser.Employee != null)
                {
                    IsEmployee = true;
                    FullName = currentUser.Employee.FullName;
                    Email = currentUser.Employee.Email;
                    PhoneNumber = currentUser.Employee.PhoneNumber;
                    CCCD = currentUser.Employee.CCCD;
                    Position = currentUser.Employee.Position.ToString();
                    HireDate = currentUser.Employee.HireDate;
                    Department = GetDepartmentByPosition(currentUser.Employee.Position);
                }
                else
                {
                    IsEmployee = false;
                    // For non-employee users, load basic info
                    FullName = currentUser.Username; // Fallback
                    Email = "";
                    PhoneNumber = "";
                }

                // Set last login (mock data for now)
                LastLoginDate = DateTime.Now.AddHours(-2);

                LogInformation("Profile loaded successfully for user: {Username}", Username);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading profile");
                _notificationService?.ShowError($"Error loading profile: {ex.Message}");
            }
        }

        private string GetDepartmentByPosition(EmployeePosition position)
        {
            return position switch
            {
                EmployeePosition.Receptionist => "Front Office",
                EmployeePosition.Cleaner => "Housekeeping",
                EmployeePosition.Technician => "Maintenance",
                EmployeePosition.Manager => "Management",
                _ => "General"
            };
        }

        private void Back()
        {
            try
            {
                LogInformation("Navigating back from profile");
                _navigationService?.GoBack();
                //LogInformation("Navigation back successful");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error navigating back");
                _notificationService?.ShowError($"Navigation error: {ex.Message}");
            }
        }

        private void EditMode()
        {
            try
            {
                LogInformation("Entering edit mode");
                IsEditMode = true;
                _notificationService?.ShowInfo("Edit mode enabled. Make your changes and click Save.");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error entering edit mode");
                _notificationService?.ShowError($"Error entering edit mode: {ex.Message}");
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                LogInformation("Saving profile changes");
                
                var currentUser = AppSession.GetCurrentUserAccount();
                if (currentUser == null)
                {
                    _notificationService?.ShowError("No user session found");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    _notificationService?.ShowError("Full name is required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    _notificationService?.ShowError("Email is required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    _notificationService?.ShowError("Phone number is required");
                    return;
                }

                // Update employee info if applicable
                if (IsEmployee && currentUser.Employee != null)
                {
                    currentUser.Employee.FullName = FullName;
                    currentUser.Employee.Email = Email;
                    currentUser.Employee.PhoneNumber = PhoneNumber;
                    currentUser.Employee.CCCD = CCCD;

                    await _employeeService.UpdateAsync(currentUser.Employee);
                    LogInformation("Employee profile updated successfully");
                }

                // Update user account info
                await _userAccountService.UpdateAsync(currentUser);

                IsEditMode = false;
                _notificationService?.ShowSuccess("Profile updated successfully!");
                
                LogInformation("Profile saved successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error saving profile");
                _notificationService?.ShowError($"Error saving profile: {ex.Message}");
            }
        }

        private void Cancel()
        {
            try
            {
                LogInformation("Canceling profile changes");
                
                // Reload original data
                LoadProfile();
                IsEditMode = false;
                
                _notificationService?.ShowInfo("Changes cancelled");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error canceling changes");
                _notificationService?.ShowError($"Error canceling changes: {ex.Message}");
            }
        }

        private void ChangePassword()
        {
            try
            {
                LogInformation("Opening password change dialog");
                
                var dialog = new Views.Dialogs.ChangePasswordDialog();
                dialog.Owner = Application.Current.MainWindow;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error opening password change dialog");
                _notificationService?.ShowError($"Error opening password change dialog: {ex.Message}");
            }
        }
    }
} 