using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HotelManager.Extensions;
using HotelManager.Models.Enums;
using HotelManager.Models;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Services;
<<<<<<< Updated upstream
=======
using RelayCommand = HotelManager.Utilities.RelayCommand;
>>>>>>> Stashed changes
using HotelManager.Helpers;

namespace HotelManager.ViewModels
{
    internal class EmployeeEditViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        public IEnumerable<EmployeePosition> Positions { get; } = EnumHelper.EmployeePositions;
        public Employee EditableEmployee { get; set; }
        private readonly Employee _originalEmp;

<<<<<<< Updated upstream
        private readonly Dictionary<string, string> _errors = new();

        public string FullNameError => GetError(nameof(EditableEmployee.FullName));
        public string EmailError => GetError(nameof(EditableEmployee.Email));
        public string PhoneNumberError => GetError(nameof(EditableEmployee.PhoneNumber));
        public string CCCDError => GetError(nameof(EditableEmployee.CCCD));

        private string GetError(string propertyName) =>
            _errors.TryGetValue(propertyName, out var msg) ? msg : string.Empty;

=======
>>>>>>> Stashed changes
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action CloseAction { get; set; } // Action to close the dialog if needed
        public bool? DialogResult { get; set; } // To indicate if the dialog was accepted or canceled

<<<<<<< Updated upstream
        public EmployeeEditViewModel(Employee employee, EmployeeService employeeService)
        {
            _originalEmp = employee;
=======
        public EmployeeEditViewModel(Employee employee)
        {
            _originalEmp = employee;
            // Clone or assign the original employee (optional deep copy to avoid immediate changes)
>>>>>>> Stashed changes
            EditableEmployee = new Employee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
<<<<<<< Updated upstream
                CCCD = employee.CCCD,
=======
>>>>>>> Stashed changes
                HireDate = employee.HireDate,
                Position = employee.Position,
                UserAccount = employee.UserAccount
            };

<<<<<<< Updated upstream
            _employeeService = employeeService;
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);
        }

        private async Task<bool> ValidateAllAsync()
        {
            _errors.Clear();

            var e = EditableEmployee;

            if (string.IsNullOrWhiteSpace(e.FullName))
                _errors[nameof(e.FullName)] = "Full name is required.";

            if (string.IsNullOrWhiteSpace(e.Email))
                _errors[nameof(e.Email)] = "Email is required.";
            else if (!e.Email.IsValidEmail())
                _errors[nameof(e.Email)] = "Invalid email format.";

            if (string.IsNullOrWhiteSpace(e.PhoneNumber))
                _errors[nameof(e.PhoneNumber)] = "Phone number is required.";
            else if (!e.PhoneNumber.IsValidPhoneNumber())
                _errors[nameof(e.PhoneNumber)] = "Invalid phone number format.";

            if (string.IsNullOrWhiteSpace(e.CCCD))
                _errors[nameof(e.CCCD)] = "ID number (CCCD) is required.";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(e.CCCD, @"^\d{12}$"))
                _errors[nameof(e.CCCD)] = "CCCD must be exactly 12 digits.";

            // Uniqueness check (excluding current)
            var all = await _employeeService.GetAllAsync();

            if (all.Any(emp => emp.Id != _originalEmp.Id && emp.Email.Equals(e.Email, StringComparison.OrdinalIgnoreCase)))
                _errors[nameof(e.Email)] = "This email is already in use.";

            if (all.Any(emp => emp.Id != _originalEmp.Id && emp.CCCD == e.CCCD))
                _errors[nameof(e.CCCD)] = "This ID number (CCCD) is already in use.";

            // Notify bindings
            OnPropertyChanged(nameof(FullNameError));
            OnPropertyChanged(nameof(EmailError));
            OnPropertyChanged(nameof(PhoneNumberError));
            OnPropertyChanged(nameof(CCCDError));

            return !_errors.Any();
=======
            _employeeService = new EmployeeService(new HotelDbContext());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(param => Cancel());

>>>>>>> Stashed changes
        }

        private async Task SaveAsync()
        {
<<<<<<< Updated upstream
            if(!await ValidateAllAsync())
            {
=======
            if (string.IsNullOrWhiteSpace(EditableEmployee.FullName) ||
                string.IsNullOrWhiteSpace(EditableEmployee.Email) ||
                string.IsNullOrWhiteSpace(EditableEmployee.PhoneNumber))
            {
                MessageBox.Show("Fields cannot be empty.");
                return;
            }

            if (!EditableEmployee.Email.IsValidEmail())
            {
                MessageBox.Show("Invalid email format.");
>>>>>>> Stashed changes
                return;
            }

            // Apply changes to the original object
            _originalEmp.FullName = EditableEmployee.FullName;
            _originalEmp.Email = EditableEmployee.Email;
            _originalEmp.PhoneNumber = EditableEmployee.PhoneNumber;
<<<<<<< Updated upstream
            _originalEmp.CCCD = EditableEmployee.CCCD;
            _originalEmp.HireDate = EditableEmployee.HireDate;
            _originalEmp.Position = EditableEmployee.Position;

            await _employeeService.UpdateAsync(_originalEmp);
=======
            _originalEmp.HireDate = EditableEmployee.HireDate;
            _originalEmp.Position = EditableEmployee.Position;

            await _employeeService.UpdateAsync(EditableEmployee);
>>>>>>> Stashed changes
            MessageBox.Show("Employee updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true; // Indicate success
            CloseAction?.Invoke(); // Close the dialog if applicable
        }

        private void Cancel()
        {
            DialogResult = false; // Indicate cancellation
            CloseAction?.Invoke(); // Close the dialog if applicable
        }
    }
}
