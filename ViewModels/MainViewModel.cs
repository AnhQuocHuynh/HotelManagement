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
            ViewModelRegistration.RegisterAll();
            CurrentView = ViewModelLocator.GetView<LoginViewModel, LoginView>();
        }

        public void BaseViewLocator(UserRole role)
        {
            switch (role)
            {
                case UserRole.Manager:
                    CurrentView = ViewModelLocator.GetView<testManagerBaseVM, Views.Common.test.testManagerBaseView>();
                    break;
                case UserRole.Staff:
                    CurrentView = ViewModelLocator.GetView<testReceptionistBaseVM, Views.Common.test.testReceptionistBaseView>();
                    break;
                default:
                    break;
            }
        }
    }
}