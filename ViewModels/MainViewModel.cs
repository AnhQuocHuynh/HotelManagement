using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Models.Enums;
using HotelManager.Utilities;
using HotelManager.ViewModels.Common;
using HotelManager.ViewModels.Common.test;
using HotelManager.Views.Common;
using HotelManager.Views.Common.test;

namespace HotelManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            // đăng ký tất cả các ViewModel và View tương ứng
            ViewModelRegistration.RegisterAll();

            // gán datacontext cho view
            var loginView = ViewDataContextService.CreateViewWithViewModel<LoginViewModel, LoginView>();
            CurrentView = loginView;

            if (loginView.DataContext is LoginViewModel loginVM)
            {
                loginVM.LoginSucceeded += position =>
                {
                    BaseViewLocator(position);
                };
            }
        }

        public void BaseViewLocator(EmployeePosition position)
        {
            switch (position)
            {
                case EmployeePosition.Manager:
                    CurrentView = ViewDataContextService.CreateViewWithViewModel<testManagerBaseVM, testManagerBaseView>();
                    break;
                case EmployeePosition.Receptionist:
                    CurrentView = ViewDataContextService.CreateViewWithViewModel<testReceptionistBaseVM, testReceptionistBaseView>();
                    break;
                case EmployeePosition.Cleaner:
                    CurrentView = ViewDataContextService.CreateViewWithViewModel<testAttendentBaseVM, testAttendentBaseView>(); 
                    break;
                default:
                    break;
            }
        }
    }
}