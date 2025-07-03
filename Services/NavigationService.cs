using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Interfaces;
using HotelManager.ViewModels;

namespace HotelManager.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Func<BaseViewModel>> _viewModelFactories;
        private readonly Stack<BaseViewModel> _navigationHistory;

        private BaseViewModel _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _viewModelFactories = new Dictionary<Type, Func<BaseViewModel>>();
            _navigationHistory = new Stack<BaseViewModel>();
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    CurrentViewModelChanged?.Invoke(_currentViewModel);
                }
            }
        }

        public bool CanGoBack => _navigationHistory.Any();

        public event Action<BaseViewModel> CurrentViewModelChanged;

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object parameter) where TViewModel : BaseViewModel
        {
            var viewModel = CreateViewModel<TViewModel>();
            
            // Pass parameter to ViewModel if it implements INavigationAware
            if (viewModel is INavigationAware navigationAware && parameter != null)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            NavigateToInternal(viewModel);
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            NavigateToInternal(viewModel);
        }

        public void GoBack()
        {
            if (!CanGoBack)
                return;

            // Notify current ViewModel about navigation away
            if (CurrentViewModel is INavigationAware currentNavigationAware)
            {
                currentNavigationAware.OnNavigatedFrom();
            }

            var previousViewModel = _navigationHistory.Pop();
            
            // Notify previous ViewModel about navigation back
            if (previousViewModel is INavigationAware previousNavigationAware)
            {
                previousNavigationAware.OnNavigatedTo(null);
            }

            CurrentViewModel = previousViewModel;
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        public void RegisterViewModelFactory<TViewModel>(Func<TViewModel> factory) where TViewModel : BaseViewModel
        {
            _viewModelFactories[typeof(TViewModel)] = factory;
        }

        private void NavigateToInternal(BaseViewModel viewModel)
        {
            // Store current ViewModel in history
            if (CurrentViewModel != null)
            {
                // Notify current ViewModel about navigation away
                if (CurrentViewModel is INavigationAware currentNavigationAware)
                {
                    currentNavigationAware.OnNavigatedFrom();
                }

                _navigationHistory.Push(CurrentViewModel);
            }

            CurrentViewModel = viewModel;
        }

        private TViewModel CreateViewModel<TViewModel>() where TViewModel : BaseViewModel
        {
            var viewModelType = typeof(TViewModel);

            // Check if custom factory is registered
            if (_viewModelFactories.TryGetValue(viewModelType, out var factory))
            {
                return (TViewModel)factory();
            }

            // Try to get from DI container
            try
            {
                return _serviceProvider.GetRequiredService<TViewModel>();
            }
            catch (InvalidOperationException)
            {
                // If not registered in DI, try to create with Activator
                try
                {
                    return (TViewModel)Activator.CreateInstance(viewModelType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Unable to create ViewModel of type {viewModelType.Name}. " +
                        $"Make sure it's registered in DI container or has a parameterless constructor.", ex);
                }
            }
        }
    }

    /// <summary>
    /// Interface for ViewModels that need to be notified about navigation events
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Called when navigating to this ViewModel
        /// </summary>
        void OnNavigatedTo(object parameter);

        /// <summary>
        /// Called when navigating away from this ViewModel
        /// </summary>
        void OnNavigatedFrom();
    }
} 