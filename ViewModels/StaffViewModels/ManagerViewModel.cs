using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace HotelManager.ViewModels.StaffViewModels
{
    public class ManagerViewModel : BaseViewModel
    {
        public ObservableCollection<string> Options { get; set; }

        private string _selectedOption;
        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged(nameof(SelectedOption));
                    changeContentControl(_selectedOption);
                }
            }
        }

        private Object _currentContent;
        public Object CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }

        // commands
        public IRelayCommand ShowEmployeeListCommand { get; }

        public ManagerViewModel()
        {
            Options = new ObservableCollection<string>
            {
                "View revenue report",
                "View receptionist activity report"
            };
            // Set a default selected option
            SelectedOption = Options.FirstOrDefault();

            // init command
            ShowEmployeeListCommand = new RelayCommand(ShowEmployeeList);
        }

        private void ShowEmployeeList()
        {
            // Dispose old content nếu có
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentContent = new Views.ManagerViews.EmployeesListView();
        }


        void changeContentControl(string option)
        {
            // Dispose of the old view's DataContext if it implements IDisposable
            // prevent memory leaks
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            switch (option)
            {
                case "View revenue report":
                    CurrentContent = new Views.ManagerViews.RevenueReportChartView();
                    break;
                case "View receptionist activity report":
                    CurrentContent = new Views.ManagerViews.ReceptionistActivityReportChartView();
                    break;
                default:
                    CurrentContent = new Views.ManagerViews.RevenueReportChartView();
                    break;
            }
        }
    }
}
