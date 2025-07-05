using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HotelManager.ViewModels.ManagerViewModels;

namespace HotelManager.ViewModels.ManagerViewModels
{
    public class ReportsBaseViewModel : BaseViewModel
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

        public ReportsBaseViewModel()
        {
            Options = new ObservableCollection<string>
            {
                "View revenue report",
                "View receptionist activity report",
                "View cleaner activity report",
            };
            // Set a default selected option
            SelectedOption = Options.FirstOrDefault();
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
                    CurrentContent = new Views.ManagerViews.Reports.RevenueReportChartView();
                    break;
                case "View receptionist activity report":
                    CurrentContent = new Views.ManagerViews.Reports.ReceptionistActivityReportChartView();
                    break;
                case "View cleaner activity report":
                    CurrentContent = new Views.ManagerViews.Reports.CleanerActivivtyReportChartView();
                    break;
                default:
                    CurrentContent = new Views.ManagerViews.Reports.RevenueReportChartView();
                    break;
            }
        }
    }
}
