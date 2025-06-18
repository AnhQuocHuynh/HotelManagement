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
using RelayCommand = HotelManager.Utilities.RelayCommand;
using HotelManager.Helpers;

namespace HotelManager.ViewModels
{
    internal class EmployeeEditViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        public IEnumerable<EmployeePosition> Positions { get; } = EnumHelper.EmployeePositions;
        public Employee EditableEmployee { get; set; }
        private readonly Employee _originalEmp;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action CloseAction { get; set; } // Action to close the dialog if needed
        public bool? DialogResult { get; set; } // To indicate if the dialog was accepted or canceled

        public EmployeeEditViewModel(Employee employee)
        {
            _originalEmp = employee;
            // Clone or assign the original employee (optional deep copy to avoid immediate changes)
            EditableEmployee = new Employee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                HireDate = employee.HireDate,
                Position = employee.Position,
                UserAccount = employee.UserAccount
            };

            _employeeService = new EmployeeService(new HotelDbContext());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(param => Cancel());

        }

        private async Task SaveAsync()
        {
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
                return;
            }

            // Apply changes to the original object
            _originalEmp.FullName = EditableEmployee.FullName;
            _originalEmp.Email = EditableEmployee.Email;
            _originalEmp.PhoneNumber = EditableEmployee.PhoneNumber;
            _originalEmp.HireDate = EditableEmployee.HireDate;
            _originalEmp.Position = EditableEmployee.Position;

            await _employeeService.UpdateAsync(EditableEmployee);
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
