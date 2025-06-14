using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ManagerViewModel()
        {
            Options = new ObservableCollection<string>
            {
                "View report",
            };
            // Set a default selected option
            SelectedOption = Options.FirstOrDefault();
        }

        void changeContentControl(string option)
        {
            switch (option)
            {
                case "View report":
                    CurrentContent = new Views.ManagerViews.RevenueReportChartView();
                    break;
                default:
                    CurrentContent = new Views.ManagerViews.RevenueReportChartView();
                    break;
            }
        }
    }
}
