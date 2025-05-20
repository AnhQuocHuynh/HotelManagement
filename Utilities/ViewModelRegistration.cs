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

            //test
            ViewModelLocator.Register<ViewModels.Common.test.testManagerBaseVM, Views.Common.test.testManagerBaseView>();
            ViewModelLocator.Register<ViewModels.Common.test.testAttendentBaseVM, Views.Common.test.testAttendentBaseView>();
            ViewModelLocator.Register<ViewModels.Common.test.testReceptionistBaseVM, Views.Common.test.testReceptionistBaseView>();
        }
    }
}
