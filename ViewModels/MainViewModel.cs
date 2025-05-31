using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Utilities;

namespace HotelManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowLoginCommand { get; }
        public ICommand ShowHomeCommand { get; }
        public ICommand ShowCleanerCommand { get; }
        public ICommand ShowTechnicianCommand { get; }

        public MainViewModel()
        {
            ShowLoginCommand = new RelayCommand(_ => CurrentView = new Views.LoginView());
            ShowHomeCommand = new RelayCommand(_ => CurrentView = new Views.HomeView());
            ShowCleanerCommand = new RelayCommand(_ =>
            {
                var cleanerWindow = new Views.StaffViews.CleanerView();
                cleanerWindow.Show();
            });
            ShowTechnicianCommand = new RelayCommand(_ =>
            {
                var technicianWindow = new Views.StaffViews.TechnicianView();
                technicianWindow.Show();
            });


            CurrentView = new Views.LoginView();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}