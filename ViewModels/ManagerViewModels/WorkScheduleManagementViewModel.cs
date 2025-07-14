using CommunityToolkit.Mvvm.Input;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Models;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.AspNetCore.Mvc.Filters;
using HotelManager.Core.Data;
using System.ComponentModel;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using HotelManager.Utilities;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.Windows.Controls;

namespace HotelManager.ViewModels.ManagerViewModels
{
    /// <summary>
    /// Option class cho WorkDay binding
    /// </summary>
    public class WorkDayOption
    {
        public WorkDay Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Option class cho WorkShift binding
    /// </summary>
    public class WorkShiftOption
    {
        public WorkShift Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho Manager quản lý lịch làm việc
    /// TODO (Bảo): Implement tất cả properties và commands với proper data binding
    /// </summary>
    public class WorkScheduleManagementViewModel : BaseViewModel
    {
        // TODO (Bảo): Inject các services cần thiết
        private readonly IWorkScheduleService _workScheduleService;
        private readonly IEmployeeService _employeeService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<WorkScheduleManagementViewModel> _logger;

        #region Properties cho Schedule Assignment

        private ObservableCollection<Employee> _employees = new();
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox để chọn nhân viên
        /// </summary>
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WorkSchedule> _weeklySchedules = new();
        /// <summary>
        /// TODO (Bảo): Bind vào DataGrid/Calendar control để hiển thị lịch tuần
        /// </summary>
        public ObservableCollection<WorkSchedule> WeeklySchedules
        {
            get => _weeklySchedules;
            set { _weeklySchedules = value; OnPropertyChanged(); }
        }

        private Employee? _selectedEmployee;
        /// <summary>
        /// TODO (Bảo): Two-way binding cho employee selection
        /// </summary>
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set 
            { 
                _selectedEmployee = value; 
                OnPropertyChanged();
                // TODO (Bảo): Update CanExecute cho AssignScheduleCommand
                ((AsyncRelayCommand)AssignScheduleCommand).NotifyCanExecuteChanged();
            }
        }

        private WorkDay _selectedDay = WorkDay.Monday;
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox cho WorkDay selection
        /// </summary>
        public WorkDay SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private WorkShift _selectedShift = WorkShift.Morning;
        /// <summary>
        /// TODO (Bảo): Bind vào ComboBox cho WorkShift selection
        /// </summary>
        public WorkShift SelectedShift
        {
            get => _selectedShift;
            set { _selectedShift = value; OnPropertyChanged(); }
        }

        private DateTime _selectedDate = DateTime.Today;
        /// <summary>
        /// TODO (Bảo): Bind vào DatePicker
        /// </summary>
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        private DateTime _selectedWeek = DateTime.Today;
        /// <summary>
        /// TODO (Bảo): Bind vào week navigation controls
        /// </summary>
        public DateTime SelectedWeek
        {
            get => _selectedWeek;
            set 
            { 
                _selectedWeek = value; 
                OnPropertyChanged();
                // TODO (Bảo): Auto-load weekly schedules khi week thay đổi
                _ = LoadWeeklySchedulesAsync();
            }
        }

        private string _assignmentNotes = string.Empty;
        /// <summary>
        /// TODO (Bảo): Bind vào TextBox cho ghi chú
        /// </summary>
        public string AssignmentNotes
        {
            get => _assignmentNotes;
            set { _assignmentNotes = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Danh sách các ngày trong tuần cho ComboBox
        /// </summary>
        public ObservableCollection<WorkDayOption> WorkDays { get; } = new()
        {
            new WorkDayOption { Value = WorkDay.Monday, DisplayName = "Thứ 2" },
            new WorkDayOption { Value = WorkDay.Tuesday, DisplayName = "Thứ 3" },
            new WorkDayOption { Value = WorkDay.Wednesday, DisplayName = "Thứ 4" },
            new WorkDayOption { Value = WorkDay.Thursday, DisplayName = "Thứ 5" },
            new WorkDayOption { Value = WorkDay.Friday, DisplayName = "Thứ 6" },
            new WorkDayOption { Value = WorkDay.Saturday, DisplayName = "Thứ 7" },
            new WorkDayOption { Value = WorkDay.Sunday, DisplayName = "Chủ Nhật" }
        };

        /// <summary>
        /// Danh sách các ca làm việc cho ComboBox
        /// </summary>
        public ObservableCollection<WorkShiftOption> WorkShifts { get; } = new()
        {
            new WorkShiftOption { Value = WorkShift.Morning, DisplayName = "Ca Sáng" },
            new WorkShiftOption { Value = WorkShift.Afternoon, DisplayName = "Ca Chiều" },
            new WorkShiftOption { Value = WorkShift.Evening, DisplayName = "Ca Tối" },
            new WorkShiftOption { Value = WorkShift.Night, DisplayName = "Ca Đêm" }
        };

        #endregion


        #region Commands

        /// <summary>
        /// TODO (Bảo): Implement command để phân công lịch làm việc
        /// </summary>
        public ICommand AssignScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để load lịch tuần
        /// </summary>
        public ICommand LoadWeeklyScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để update schedule
        /// </summary>
        public ICommand UpdateScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để delete schedule
        /// </summary>
        public ICommand DeleteScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement navigation commands cho tuần trước/sau
        /// </summary>
        public ICommand NavigatePreviousWeekCommand { get; private set; }
        public ICommand NavigateNextWeekCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Implement command để refresh data
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        #endregion

        #region Constructor

        public WorkScheduleManagementViewModel(
            IWorkScheduleService workScheduleService,
            IEmployeeService employeeService,
            INotificationService notificationService,
            ILogger<WorkScheduleManagementViewModel> logger)
        {
            _workScheduleService = workScheduleService;
            _employeeService = employeeService;
            _notificationService = notificationService;
            _logger = logger;

            // TODO (Bảo): Initialize commands
            InitializeCommands();
        }

        // TODO (Bảo): Parameterless constructor for design-time
        public WorkScheduleManagementViewModel() : this(null!, null!, null!, null!)
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// TODO (Bảo): Initialize tất cả commands
        /// </summary>
        private async Task InitializeCommands()
        {
            AssignScheduleCommand = new AsyncRelayCommand(AssignScheduleAsync, CanAssignSchedule);
            LoadWeeklyScheduleCommand = new AsyncRelayCommand(LoadWeeklySchedulesAsync);
            UpdateScheduleCommand = new AsyncRelayCommand<WorkSchedule>(UpdateScheduleAsync);
            DeleteScheduleCommand = new AsyncRelayCommand<WorkSchedule>(DeleteScheduleAsync);
            NavigatePreviousWeekCommand = new AsyncRelayCommand(NavigateToPreviousWeek);
            NavigateNextWeekCommand = new AsyncRelayCommand(NavigateToNextWeek);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

            // TODO (Bảo): Load initial data
            await LoadInitialDataAsync();
        }

        /// <summary>
        /// TODO (Bảo): Implement validation cho assignment
        /// </summary>
        private bool CanAssignSchedule()
        {
            // TODO: Check if employee selected và basic validation
            return SelectedEmployee != null;
        }

        /// <summary>
        /// TODO (Bảo): Implement schedule assignment với error handling
        /// </summary>
        private async Task AssignScheduleAsync()
        {
            try
            {
                // Validate services
                if (_workScheduleService == null)
                {
                    _notificationService?.ShowError("WorkScheduleService chưa được khởi tạo");
                    return;
                }

                if (SelectedEmployee == null)
                {
                    _notificationService?.ShowError("Vui lòng chọn nhân viên");
                    return;
                }

                // TODO (Bảo): Implement assignment logic
                // 1. Validate input
                bool isConfilct = await _workScheduleService.ValidateScheduleConflictAsync(
                    SelectedEmployee.Id,
                    SelectedDay,
                    SelectedShift,
                    SelectedDate
                );

                if (isConfilct)
                {
                    _notificationService?.ShowError("Lịch làm việc đã có xung đột. Vui lòng chọn lại.");
                    return;
                }

                // 2. Call _workScheduleService.AssignScheduleAsync
                await _workScheduleService.AssignScheduleAsync(
                    SelectedEmployee.Id,
                    SelectedDay,
                    SelectedShift,
                    SelectedDate,
                    null, // End date can be null for single day assignments
                    AppSession.GetCurrentUserAccount().EmployeeId, // Assigned by employee ID (can be set to current user)
                    AssignmentNotes
                );

                // 3. Refresh UI
                if (IsSameWeek(SelectedDate, SelectedWeek))
                    await LoadWeeklySchedulesAsync();

                // 4. Show success notification
                _notificationService?.ShowSuccess("Thêm lịch làm việc thành công");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning schedule");
                _notificationService?.ShowError("Lỗi phân công lịch làm việc");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement load weekly schedules
        /// </summary>
        private async Task LoadWeeklySchedulesAsync()
        {
            try
            {
                // TODO (Bảo): Implement loading logic

                WeeklySchedules = new ObservableCollection<WorkSchedule>(await _workScheduleService.GetWeeklyScheduleAsync(GetStartOfWeek(SelectedWeek)));

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading weekly schedules");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement update schedule
        /// </summary>
        private Boolean isUpdating = false;
        private async Task UpdateScheduleAsync(WorkSchedule? schedule)
        {
            // TODO (Bảo): Implement update logic
            bool isConfilct = await _workScheduleService.ValidateScheduleConflictAsync(
                SelectedEmployee.Id,
                SelectedDay,
                SelectedShift,
                SelectedDate
            );

            if (isConfilct)
            {
                _notificationService?.ShowError("Lịch làm việc đã có xung đột. Vui lòng chọn lại.");
                return;
            }

            schedule = await _workScheduleService.UpdateAsync(
                new WorkSchedule
                {
                    EmployeeId = SelectedEmployee.Id,
                    WorkDay = SelectedDay,
                    Shift = SelectedShift,
                    StartDate = SelectedDate,
                    Notes = AssignmentNotes
                }
            );

            await LoadWeeklySchedulesAsync();

            _notificationService.ShowSuccess("Thay đổi lịch làm việc thành công");
        }

        /// <summary>
        /// TODO (Bảo): Implement delete schedule với confirmation
        /// </summary>
        private async Task DeleteScheduleAsync(WorkSchedule? schedule)
        {
            Debug.WriteLine($"[DEBUG] DeleteScheduleAsync called with id={schedule?.Id}");

            // TODO (Bảo): Implement delete với user confirmation
            if (schedule != null)
            {
                var result = MessageBox.Show(
                    "Do you really want to delete this schedule",
                    "Yes, delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _workScheduleService.DeleteAsync(schedule.Id);
                        _notificationService.ShowSuccess("delete succesfully", 3);

                        // Refresh data after deletion
                        if (IsSameWeek(schedule.StartDate, SelectedWeek))
                            await LoadWeeklySchedulesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error deleting schedule");
                        _notificationService.ShowError("Can't delete schedule", 3);
                    }
                }
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement week navigation
        /// </summary>
        private async Task NavigateToPreviousWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(-7);
        }

        private async Task NavigateToNextWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(7);
        }

        /// <summary>
        /// TODO (Bảo): Implement data refresh
        /// </summary>
        private bool isRefreshing = false;
        private async Task RefreshDataAsync()
        {
            if (isRefreshing == true)
                return;
            isRefreshing = true;
            // TODO (Bảo): Reload employees và schedules

            var employees = (await _employeeService.GetAllAsync()).ToList();
            Employees = new ObservableCollection<Employee>(employees);
            SelectedEmployee = Employees.FirstOrDefault();

            SelectedWeek = DateTime.Today.Date;
            DateTime startDate = SelectedWeek.AddDays(SelectedWeek.DayOfWeek - DayOfWeek.Monday).Date;

            //Employees = new ObservableCollection<Employee>(SampleWorkScheduleData.GetSampleEmployees());
            //WeeklySchedules = new ObservableCollection<WorkSchedule>(SampleWorkScheduleData.GetSampleSchedules());

            _notificationService.ShowSuccess("làm mới thành công");
            isRefreshing = false;
        }

        /// <summary>
        /// TODO (Bảo): Implement initial data loading
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            // TODO (Bảo): Load employees và current week schedules
            await RefreshDataAsync();

        }

        #endregion

        #region Helpers

        private DateTime GetStartOfWeek(DateTime date)
        {
            // giả sử tuần bắt đầu từ thứ 2 (Monday)
            int diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff).Date;
        }


        private bool IsSameWeek(DateTime date1, DateTime date2)
        {
            DateTime startOfWeek1 = GetStartOfWeek(date1);
            DateTime startOfWeek2 = GetStartOfWeek(date2);
            return startOfWeek1 == startOfWeek2;
        }

        #endregion
    }
} 