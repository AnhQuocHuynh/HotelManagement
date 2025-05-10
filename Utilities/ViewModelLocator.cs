using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels;

namespace HotelManager.Utilities
{
    public class ViewModelLocator
    {
        private static readonly Dictionary<Type, Type> _viewModelToViewMap = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, object> _viewModelInstances = new Dictionary<Type, object>();

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