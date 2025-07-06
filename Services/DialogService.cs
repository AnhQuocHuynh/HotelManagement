<<<<<<< Updated upstream
using HotelManager.Interfaces;
using System.Windows;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HotelManager.Interfaces;
using HotelManager.Utilities;
using HotelManager.ViewModels;
>>>>>>> Stashed changes
using HotelManager.Views;

namespace HotelManager.Services
{
<<<<<<< Updated upstream
    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            Window? window = null;

            if (viewModel is ViewModels.EmployeeEditViewModel)
            {
                window = new EmployeeEditView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };
            }
            else if (viewModel is ViewModels.Admin.AccountCreateViewModel)
            {
                window = new AccountCreateView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false
                };
            }
            else
            {
                // fallback generic window
                window = new Window
                {
                    Content = new System.Windows.Controls.ContentControl { DataContext = viewModel },
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Width = 400,
                    Height = 300,
                    Title = "Dialog"
                };
            }

            return window.ShowDialog();
        }
    }
} 
=======
    internal class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            if (!ViewModelLocatorRegistered(viewModel.GetType()))
                throw new InvalidOperationException($"ViewModel not registered: {viewModel.GetType().Name}");

            var view = ViewModelLocator.GetView<TViewModel,Window>();
            view.DataContext = viewModel;

            

            // Set CloseAction in VM if it exists
            var closeProp = typeof(TViewModel).GetProperty("CloseAction");
            if (closeProp != null && closeProp.PropertyType == typeof(Action))
            {
                closeProp.SetValue(viewModel, new Action(() =>
                {
                    view.DialogResult = (bool?)typeof(TViewModel).GetProperty("DialogResult")?.GetValue(viewModel);
                    view.Close();
                }));
            }

            return view.ShowDialog();
        }

        //Helper to check if a viewmodel is registered
        private bool ViewModelLocatorRegistered(Type vmType)
        {
            var mapField = typeof(ViewModelLocator).GetField("_viewModelToViewMap", BindingFlags.NonPublic | BindingFlags.Static);
            var map = mapField?.GetValue(null) as Dictionary<Type, Type>;
            return map?.ContainsKey(vmType) == true;
        }

    }
}
>>>>>>> Stashed changes
