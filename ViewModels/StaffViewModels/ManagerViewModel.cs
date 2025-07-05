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
        public IRelayCommand ShowReportsCommand { get; }


        public ManagerViewModel()
        {
            // init command
            ShowEmployeeListCommand = new RelayCommand(ShowEmployeeList);
            ShowReportsCommand = new RelayCommand(ShowReports);

            ShowReports();
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

        private void ShowReports()
        {
            // Dispose old content nếu có
            if (CurrentContent is FrameworkElement oldView && oldView.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentContent = new Views.ManagerViews.ReportsBaseView();
        }
    }
}
