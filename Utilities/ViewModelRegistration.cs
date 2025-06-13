using HotelManager.ViewModels;
using HotelManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Utilities
{
    class ViewModelRegistration
    {
        public static void RegisterAll()
        {
            ViewModelLocator.Register<MainViewModel, MainWindow>();
            ViewModelLocator.Register<ViewModels.Common.LoginViewModel, Views.Common.LoginView>();
            ViewModelLocator.Register<AdminViewModel, AdminView>();
            ViewModelLocator.Register<EmployeeEditViewModel, EmployeeEditView>();
        }
    }
}
