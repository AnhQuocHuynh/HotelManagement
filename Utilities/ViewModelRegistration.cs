using HotelManager.ViewModels;
using HotelManager.ViewModels.Common;
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
            // Register all ViewModels and their corresponding Views here
            ViewModelLocator.Register<MainViewModel, MainWindow>();
            ViewModelLocator.Register<ViewModels.Common.LoginViewModel, Views.Common.LoginView>();

        }


    }
}
