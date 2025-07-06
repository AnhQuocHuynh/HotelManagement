<<<<<<< Updated upstream
namespace HotelManager.Utilities;
=======
﻿using HotelManager.ViewModels;
using HotelManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> Stashed changes

public static class ViewModelRegistration
{
    public static void RegisterAll()
    {
<<<<<<< Updated upstream
        // Legacy placeholder – no-op. Views can rely on ViewModelLocator now.
=======
        public static void RegisterAll()
        {
            ViewModelLocator.Register<MainViewModel, MainWindow>();
            ViewModelLocator.Register<ViewModels.Common.LoginViewModel, Views.Common.LoginView>();
            ViewModelLocator.Register<AdminViewModel, AdminView>();
            ViewModelLocator.Register<EmployeeEditViewModel, EmployeeEditView>();
            ViewModelLocator.Register<RoomViewModel, RoomView>();
            ViewModelLocator.Register<RoomInfoEditViewModel, RoomInfoEditView>();
        }
>>>>>>> Stashed changes
    }
} 