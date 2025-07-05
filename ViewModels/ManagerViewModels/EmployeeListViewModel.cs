using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services.Manager;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class EmployeeListViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;

        // Dữ liệu gốc
        private List<Employee> _allEmployees = new();

        // Dữ liệu hiển thị trên UI
        public ObservableCollection<Employee> Employees { get; set; } = new();

        // Các filter property
        private string _filterName;
        public string FilterName
        {
            get => _filterName;
            set => SetProperty(ref _filterName, value);
        }

        private string _filterMail;
        public string FilterMail
        {
            get => _filterMail;
            set => SetProperty(ref _filterMail, value);
        }

        private string _filterPhone;
        public string FilterPhone
        {
            get => _filterPhone;
            set => SetProperty(ref _filterPhone, value);
        }

        private string _filterCCCD;
        public string FilterCCCD
        {
            get => _filterCCCD;
            set => SetProperty(ref _filterCCCD, value);
        }

        private DateTime? _filterHireDateFrom;
        public DateTime? FilterHireDateFrom
        {
            get => _filterHireDateFrom;
            set => SetProperty(ref _filterHireDateFrom, value);
        }

        private DateTime? _filterHireDateTo;
        public DateTime? FilterHireDateTo
        {
            get => _filterHireDateTo;
            set => SetProperty(ref _filterHireDateTo, value);
        }

        private String _selectedPosition;
        public String SelectedPosition
        {
            get => _selectedPosition;
            set => SetProperty(ref _selectedPosition, value);
        }

        // List cho ComboBox Positions
        public List<String> Positions { get; set; }
        private void positionInit()
        {
            Positions = new List<string> { "All" };
            Positions.AddRange(Enum.GetValues(typeof(EmployeePosition))
                         .Cast<EmployeePosition>()
                         .Select(p => p.ToString()));
        }


        // Commands
        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetCommand { get; }

        // Scope để quản lý vòng đời của các dịch vụ
        private IServiceScope _scope;
        
        // Constructor
        public EmployeeListViewModel()
        {

            _scope = App.ServiceProvider.CreateScope();
            var dbContext = _scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            _employeeService = new EmployeeService(dbContext);

            positionInit();
            ResetFilters();

            ApplyFilterCommand = new RelayCommand(ApplyFilters);
            ResetCommand = new RelayCommand(ResetFilters);

            _ = FetchEmployeesAsync();
        }

        // lấy employee từ db và gán vào list
        private async Task FetchEmployeesAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            _allEmployees = employees.ToList();
            UpdateEmployeesList(_allEmployees);
        }


        private void ApplyFilters()
        {
            var filtered = _allEmployees.Where(e =>
                (string.IsNullOrWhiteSpace(FilterName) || e.FullName.Contains(FilterName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(FilterMail) || e.Email.Contains(FilterMail, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(FilterPhone) || e.PhoneNumber.Contains(FilterPhone)) &&
                (string.IsNullOrWhiteSpace(FilterCCCD) || e.CCCD.Contains(FilterCCCD)) &&
                (!FilterHireDateFrom.HasValue || e.HireDate.Date >= FilterHireDateFrom.Value.Date) &&
                (!FilterHireDateTo.HasValue || e.HireDate.Date <= FilterHireDateTo.Value.Date) &&
                (string.IsNullOrEmpty(SelectedPosition) || SelectedPosition == "All" ||
                    (Enum.TryParse(SelectedPosition, out EmployeePosition pos) && e.Position == pos))
            ).ToList();

            UpdateEmployeesList(filtered);
        }


        private void ResetFilters()
        {
            FilterName = string.Empty;
            FilterMail = string.Empty;
            FilterPhone = string.Empty;
            FilterCCCD = string.Empty;
            FilterHireDateFrom = null;
            FilterHireDateTo = null;
            SelectedPosition = "All";

            UpdateEmployeesList(_allEmployees);
        }



        private void UpdateEmployeesList(List<Employee> employees)
        {
            Employees.Clear();
            foreach (var emp in employees)
            {
                Employees.Add(emp);
            }
        }
    }
}
