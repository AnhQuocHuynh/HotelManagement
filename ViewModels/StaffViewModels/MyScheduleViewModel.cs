using CommunityToolkit.Mvvm.Input;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Models;
using HotelManager.Models.Enums;
using HotelManager.Models;
using HotelManager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManager.Utilities;

namespace HotelManager.ViewModels.StaffViewModels
{
    /// <summary>
    /// ViewModel cho nhân viên xem lịch làm việc cá nhân
    /// TODO (Bảo): Implement tất cả properties và commands cho staff schedule view
    /// </summary>
    public class MyScheduleViewModel : BaseViewModel
    {
        // TODO (Bảo): Inject services
        private readonly IWorkScheduleService _workScheduleService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MyScheduleViewModel> _logger;

        #region Properties

        private ObservableCollection<WorkSchedule> _mySchedules = new();
        /// <summary>
        /// TODO (Bảo): Bind vào ListView/Calendar để hiển thị lịch cá nhân
        /// </summary>
        public ObservableCollection<WorkSchedule> MySchedules
        {
            get => _mySchedules;
            set { _mySchedules = value; OnPropertyChanged(); }
        }

        private DateTime _selectedWeek = DateTime.Today;
        /// <summary>
        /// TODO (Bảo): Bind vào week navigation
        /// </summary>
        public DateTime SelectedWeek
        {
            get => _selectedWeek;
            set 
            { 
                _selectedWeek = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeekDisplayText));
                // TODO (Bảo): Auto-load schedules khi week thay đổi
                _ = LoadMyScheduleAsync();
            }
        }

        private WorkSchedule? _selectedSchedule;
        /// <summary>
        /// TODO (Bảo): Bind cho schedule details view
        /// </summary>
        public WorkSchedule? SelectedSchedule
        {
            get => _selectedSchedule;
            set 
            { 
                _selectedSchedule = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedSchedule));
            }
        }

        private bool _isLoading;
        /// <summary>
        /// TODO (Bảo): Bind vào loading indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// TODO (Bảo): Computed property cho UI display
        /// </summary>
        public string WeekDisplayText => $"Tuần {SelectedWeek:dd/MM} - {SelectedWeek.AddDays(6):dd/MM/yyyy}";

        /// <summary>
        /// TODO (Bảo): Helper property cho UI visibility
        /// </summary>
        public bool HasSelectedSchedule => SelectedSchedule != null;

        /// <summary>
        /// TODO (Bảo): Current user employee ID (lấy từ AppSession)
        /// </summary>
        public int CurrentEmployeeId => AppSession.GetCurrentUserAccount()?.EmployeeId ?? 0;

        #endregion

        #region Commands

        /// <summary>
        /// TODO (Bảo): Command load lịch cá nhân
        /// </summary>
        public ICommand LoadMyScheduleCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Commands navigation tuần
        /// </summary>
        public ICommand NavigatePreviousWeekCommand { get; private set; }
        public ICommand NavigateNextWeekCommand { get; private set; }
        public ICommand NavigateCurrentWeekCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Command xem chi tiết schedule
        /// </summary>
        public ICommand ViewScheduleDetailsCommand { get; private set; }

        /// <summary>
        /// TODO (Bảo): Command refresh data
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        #endregion

        #region Constructor

        public MyScheduleViewModel(
            IWorkScheduleService workScheduleService,
            INotificationService notificationService,
            ILogger<MyScheduleViewModel> logger)
        {
            _workScheduleService = workScheduleService;
            _notificationService = notificationService;
            _logger = logger;

            // TODO (Bảo): Initialize commands
            InitializeCommands();
        }

        // TODO (Bảo): Parameterless constructor for design-time
        public MyScheduleViewModel() : this(null!, null!, null!)
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// TODO (Bảo): Initialize commands
        /// </summary>
        private void InitializeCommands()
        {
            LoadMyScheduleCommand = new AsyncRelayCommand(LoadMyScheduleAsync);
            NavigatePreviousWeekCommand = new RelayCommand(NavigateToPreviousWeek);
            NavigateNextWeekCommand = new RelayCommand(NavigateToNextWeek);
            NavigateCurrentWeekCommand = new RelayCommand(NavigateToCurrentWeek);
            ViewScheduleDetailsCommand = new AsyncRelayCommand<WorkSchedule>(ViewScheduleDetailsAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

            // TODO (Bảo): Load initial data
            _ = LoadMyScheduleAsync();
        }

        /// <summary>
        /// TODO (Bảo): Implement load lịch cá nhân với error handling
        /// </summary>
        private async Task LoadMyScheduleAsync()
        {
            try
            {
                if (CurrentEmployeeId == 0)
                {
                    _notificationService?.ShowWarning("Không thể xác định thông tin nhân viên");
                    return;
                }

                IsLoading = true;

                // TODO (Bảo): Implement loading logic
                // 1. Calculate week start/end dates
                // 2. Call _workScheduleService.GetEmployeeScheduleAsync
                // 3. Update MySchedules collection
                // 4. Handle empty results

                throw new NotImplementedException("TODO (Bảo): Implement LoadMyScheduleAsync");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading my schedule");
                _notificationService?.ShowError("Lỗi tải lịch làm việc");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement week navigation
        /// </summary>
        private void NavigateToPreviousWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(-7);
        }

        private void NavigateToNextWeek()
        {
            SelectedWeek = SelectedWeek.AddDays(7);
        }

        private void NavigateToCurrentWeek()
        {
            SelectedWeek = DateTime.Today;
        }

        /// <summary>
        /// TODO (Bảo): Implement schedule details view
        /// </summary>
        private async Task ViewScheduleDetailsAsync(WorkSchedule? schedule)
        {
            try
            {
                if (schedule == null) return;

                SelectedSchedule = schedule;

                // TODO (Bảo): Show details in popup/side panel
                // Có thể navigate đến detail view hoặc show dialog

                throw new NotImplementedException("TODO (Bảo): Implement ViewScheduleDetailsAsync");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error viewing schedule details");
                _notificationService?.ShowError("Lỗi xem chi tiết lịch làm việc");
            }
        }

        /// <summary>
        /// TODO (Bảo): Implement data refresh
        /// </summary>
        private async Task RefreshDataAsync()
        {
            try
            {
                await LoadMyScheduleAsync();
                _notificationService?.ShowSuccess("Đã cập nhật lịch làm việc");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing data");
                _notificationService?.ShowError("Lỗi làm mới dữ liệu");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// TODO (Bảo): Helper method để group schedules by day
        /// </summary>
        public IEnumerable<IGrouping<WorkDay, WorkSchedule>> GetSchedulesByDay()
        {
            return MySchedules.GroupBy(s => s.WorkDay).OrderBy(g => (int)g.Key);
        }

        /// <summary>
        /// TODO (Bảo): Helper method để check có lịch trong ngày không
        /// </summary>
        public bool HasScheduleOnDay(WorkDay day)
        {
            return MySchedules.Any(s => s.WorkDay == day);
        }

        /// <summary>
        /// TODO (Bảo): Helper method để get total working hours trong tuần
        /// </summary>
        public int GetTotalWorkingHours()
        {
            // TODO (Bảo): Calculate based on shift hours
            return MySchedules.Count * 8; // Giả sử mỗi ca 8 tiếng
        }

        #endregion
    }
} 