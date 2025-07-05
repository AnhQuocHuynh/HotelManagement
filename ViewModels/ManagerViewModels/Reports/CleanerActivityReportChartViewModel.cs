using HotelManager.Services.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.ViewModels.ManagerViewModels.Reports
{
    public class CleanerActivityReportChartViewModel : BaseViewModel
    {
        ExportService _exportService;

        public ObservableCollection<string> ViewTypeOptions { get; set; }

        private string _selectedViewType;
        public string SelectedViewType
        {
            get => _selectedViewType;
            set
            {
                if (_selectedViewType != value)
                {
                    _selectedViewType = value;
                    OnPropertyChanged(nameof(SelectedViewType));
                }
            }
        }

        private void ViewTypeOptionsInit()
        {
            ViewTypeOptions = new ObservableCollection<string>
            {
                "Total by time",
                "Each cleaner on time",
            };
            SelectedViewType = ViewTypeOptions.FirstOrDefault();
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        // end date
        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }


    }
}
