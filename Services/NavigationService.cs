using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Interfaces;
using HotelManager.ViewModels;
using Serilog;
using System.Diagnostics;

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

            Debug.WriteLine($"Created view model of type {typeof(TViewModel).Name}");

            // Pass parameter to ViewModel if it implements INavigationAware
            if (viewModel is INavigationAware navigationAware && parameter != null)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            NavigateToInternal(viewModel);
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            // Check if the ViewModel is already the current one
            if (_currentViewModel != null && _currentViewModel.GetType() == viewModel.GetType())
                return;
            // Ensure the ViewModel is not null
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            NavigateToInternal(viewModel);
        }

        public void GoBack()
        {
            LogStack("Before GoBack");

            if (!CanGoBack)
            {
                Log.Warning("[NavigationService] Cannot go back — navigation history is empty.");
                return;
            }

            if (CurrentViewModel is INavigationAware currentNavigationAware)
            {
                Log.Information("[NavigationService] Navigating away from {ViewModel}", CurrentViewModel.GetType().Name);
                currentNavigationAware.OnNavigatedFrom();
            }

            var previousViewModel = _navigationHistory.Pop();

            if (previousViewModel is INavigationAware previousNavigationAware)
            {
                Log.Information("[NavigationService] Navigating back to {ViewModel}", previousViewModel.GetType().Name);
                previousNavigationAware.OnNavigatedTo(null);
            }

            CurrentViewModel = previousViewModel;

            LogStack("After GoBack");
        }

        //debugging method to log the current navigation stack
        private void LogStack(string context)
        {
            var stackSnapshot = _navigationHistory
                .Select(vm => vm.GetType().Name)
                .Reverse()
                .ToList();

            var stackInfo = string.Join(" -> ", stackSnapshot);
            Log.Information("[NavigationService] {Context} | Stack: [TOP] {Stack}", context, stackInfo);
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