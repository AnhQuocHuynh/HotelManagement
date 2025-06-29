using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels;
using HotelManager.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Utilities
{
    public class ViewModelLocator
    {
        // ✅ Singleton instance for XAML binding
        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        private static readonly Dictionary<Type, Type> _viewModelToViewMap = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, object> _viewModelInstances = new Dictionary<Type, object>();

        // ✅ Properties for XAML binding - using public types only
        public MainViewModel MainViewModel => GetViewModel<MainViewModel>();
        public PaymentViewModel PaymentViewModel => GetViewModel<PaymentViewModel>();
        // Note: LoginViewModel, RoomViewModel, AdminViewModel, BookingViewModel are internal - can't expose directly
        // Will handle these through GetViewModel<T>() method instead

        public static void Register<TViewModel, TView>() 
            where TViewModel : class
            where TView : FrameworkElement
        {
            _viewModelToViewMap[typeof(TViewModel)] = typeof(TView);
        }

        public static TView GetView<TViewModel, TView>() 
            where TViewModel : class
            where TView : FrameworkElement
        {
            if (!_viewModelToViewMap.ContainsKey(typeof(TViewModel)))
            {
                throw new KeyNotFoundException($"No view registered for ViewModel type {typeof(TViewModel).Name}");
            }

            var viewType = _viewModelToViewMap[typeof(TViewModel)];
            return (TView)Activator.CreateInstance(viewType);
        }

        public static TViewModel GetViewModel<TViewModel>() where TViewModel : class
        {
            if (App.ServiceProvider != null)
            {
                try
                {
                    return App.ServiceProvider.GetService<TViewModel>() ?? CreateManualViewModel<TViewModel>();
                }
                catch
                {
                    return CreateManualViewModel<TViewModel>();
                }
            }
            
            return CreateManualViewModel<TViewModel>();
        }

        public static TViewModel CreateViewModel<TViewModel>() where TViewModel : class
        {
            if (App.ServiceProvider != null)
            {
                try
                {
                    return App.ServiceProvider.GetService<TViewModel>() ?? 
                           (TViewModel)Activator.CreateInstance<TViewModel>();
                }
                catch
                {
                    return (TViewModel)Activator.CreateInstance<TViewModel>();
                }
            }
            
            return (TViewModel)Activator.CreateInstance<TViewModel>();
        }

        private static TViewModel CreateManualViewModel<TViewModel>() where TViewModel : class
        {
            if (!_viewModelInstances.ContainsKey(typeof(TViewModel)))
            {
                _viewModelInstances[typeof(TViewModel)] = Activator.CreateInstance<TViewModel>();
            }

            return (TViewModel)_viewModelInstances[typeof(TViewModel)];
        }

        public static void ClearViewModelInstances()
        {
            _viewModelInstances.Clear();
        }
    }
} 