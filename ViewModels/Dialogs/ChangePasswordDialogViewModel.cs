using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Helpers;
using HotelManager.Utilities;

namespace HotelManager.ViewModels.Dialogs
{
    public class ChangePasswordDialogViewModel : BaseViewModel
    {
        private readonly UserAccountService _userAccountService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ChangePasswordDialogViewModel> _logger;

        // Password Properties
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _hasError;
        private bool _canChangePassword;
        private bool _isPasswordValid;
        private double _passwordStrength;
        private string _passwordStrengthText;
        private Brush _passwordStrengthColor;
        private bool _isCurrentPasswordVisible;
        public bool IsCurrentPasswordVisible { get => _isCurrentPasswordVisible; set { _isCurrentPasswordVisible = value; OnPropertyChanged(); } }
        private bool _isNewPasswordVisible;
        public bool IsNewPasswordVisible { get => _isNewPasswordVisible; set { _isNewPasswordVisible = value; OnPropertyChanged(); } }
        private bool _isConfirmPasswordVisible;
        public bool IsConfirmPasswordVisible { get => _isConfirmPasswordVisible; set { _isConfirmPasswordVisible = value; OnPropertyChanged(); } }
        private string _firstErrorMessage;
        public string FirstErrorMessage
        {
            get => _firstErrorMessage;
            set { _firstErrorMessage = value; OnPropertyChanged(); }
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set { _currentPassword = value; OnPropertyChanged(); ValidatePasswords(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); ValidatePasswords(); UpdatePasswordStrength(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); ValidatePasswords(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        public bool CanChangePassword
        {
            get => _canChangePassword;
            set { _canChangePassword = value; OnPropertyChanged(); }
        }

        public bool IsPasswordValid
        {
            get => _isPasswordValid;
            set { _isPasswordValid = value; OnPropertyChanged(); }
        }

        public double PasswordStrength
        {
            get => _passwordStrength;
            set { _passwordStrength = value; OnPropertyChanged(); }
        }

        public string PasswordStrengthText
        {
            get => _passwordStrengthText;
            set { _passwordStrengthText = value; OnPropertyChanged(); }
        }

        public Brush PasswordStrengthColor
        {
            get => _passwordStrengthColor;
            set { _passwordStrengthColor = value; OnPropertyChanged(); }
        }

        public Action CloseAction { get; set; }

        // Commands
        public ICommand ChangePasswordCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ToggleCurrentPasswordVisibilityCommand { get; }
        public ICommand ToggleNewPasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

        public ChangePasswordDialogViewModel(UserAccountService userAccountService, 
            INotificationService notificationService, ILogger<ChangePasswordDialogViewModel> logger)
        {
            _userAccountService = userAccountService;
            _notificationService = notificationService;
            _logger = logger;

            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
            CancelCommand = new RelayCommand(Cancel);
            ToggleCurrentPasswordVisibilityCommand = new RelayCommand(() => IsCurrentPasswordVisible = !IsCurrentPasswordVisible);
            ToggleNewPasswordVisibilityCommand = new RelayCommand(() => IsNewPasswordVisible = !IsNewPasswordVisible);
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
        }

        public ChangePasswordDialogViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time data
                PasswordStrength = 75;
                PasswordStrengthText = "Strong";
                PasswordStrengthColor = Brushes.Green;
                IsPasswordValid = true;
            }
            else
            {
                _userAccountService = App.ServiceProvider?.GetRequiredService<UserAccountService>();
                _notificationService = App.ServiceProvider?.GetRequiredService<INotificationService>();
                _logger = App.ServiceProvider?.GetRequiredService<ILogger<ChangePasswordDialogViewModel>>();

                ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
                CancelCommand = new RelayCommand(Cancel);
                ToggleCurrentPasswordVisibilityCommand = new RelayCommand(() => IsCurrentPasswordVisible = !IsCurrentPasswordVisible);
                ToggleNewPasswordVisibilityCommand = new RelayCommand(() => IsNewPasswordVisible = !IsNewPasswordVisible);
                ToggleConfirmPasswordVisibilityCommand = new RelayCommand(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
            }
        }

        private void ValidatePasswords()
        {
            var errors = new System.Collections.Generic.List<string>();

            // Validate current password
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                errors.Add("Current password is required");
            }

            // Validate new password
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                errors.Add("New password is required");
            }
            else
            {
                if (NewPassword.Length < 8)
                {
                    errors.Add("New password must be at least 8 characters long");
                }

                if (!Regex.IsMatch(NewPassword, @"\d"))
                {
                    errors.Add("New password must contain at least one number");
                }

                if (NewPassword == CurrentPassword)
                {
                    errors.Add("New password must be different from current password");
                }
            }

            // Validate confirm password
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                errors.Add("Please confirm your new password");
            }
            else if (NewPassword != ConfirmPassword)
            {
                errors.Add("Passwords do not match");
            }

            // Update error state
            if (errors.Count > 0)
            {
                ErrorMessage = string.Join("\n", errors);
                FirstErrorMessage = errors[0];
                HasError = true;
                IsPasswordValid = false;
            }
            else
            {
                ErrorMessage = "";
                FirstErrorMessage = "";
                HasError = false;
                IsPasswordValid = true;
            }
        }

        private void UpdatePasswordStrength()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                PasswordStrength = 0;
                PasswordStrengthText = "";
                PasswordStrengthColor = Brushes.Gray;
                IsPasswordValid = false;
                return;
            }

            var strength = 0.0;

            // Length check
            if (NewPassword.Length >= 8) strength += 50;
            if (NewPassword.Length >= 12) strength += 25;

            // Number check
            if (Regex.IsMatch(NewPassword, @"\d")) strength += 25;

            PasswordStrength = Math.Min(strength, 100);

            // Set strength text and color
            if (PasswordStrength < 50)
            {
                PasswordStrengthText = "Weak";
                PasswordStrengthColor = Brushes.Red;
            }
            else if (PasswordStrength < 75)
            {
                PasswordStrengthText = "Fair";
                PasswordStrengthColor = Brushes.Orange;
            }
            else
            {
                PasswordStrengthText = "Good";
                PasswordStrengthColor = Brushes.Green;
            }

            IsPasswordValid = PasswordStrength >= 50;
        }

        private async Task ChangePasswordAsync()
        {
            if(CurrentPassword.Equals(NewPassword, StringComparison.OrdinalIgnoreCase))
            {
                _notificationService?.ShowError("New password must be different from current password");
                return;
            }
            try
            {
                LogInformation("Attempting to change password");

                var currentUser = AppSession.GetCurrentUserAccount();
                if (currentUser == null)
                {
                    _notificationService?.ShowError("No user session found");
                    return;
                }

                // Validate current password
                if (!await _userAccountService.ValidatePasswordAsync(currentUser.Username, CurrentPassword))
                {
                    //MessageBox.Show("Current password is incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _notificationService?.ShowError("Current password is incorrect");
                    return; 
                }
                // Change password
                await _userAccountService.ChangePasswordAsync(currentUser.Username, NewPassword);               
                _notificationService?.ShowSuccess("Password changed successfully!");
                //MessageBox.Show("Password changed successfully!");
                LogInformation("Password changed successfully for user: {Username}", currentUser.Username);

                // Close dialog
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error changing password");
                _notificationService?.ShowError($"Error changing password: {ex.Message}");
            }
        }

        private void Cancel()
        {
            try
            {
                LogInformation("Canceling password change");
                
                // Close dialog
                if (Application.Current.Windows.Count > 0)
                {
                    Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error canceling password change");
                _notificationService?.ShowError($"Error canceling: {ex.Message}");
            }
        }
    }
} 