using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HotelManager.ViewModels;
using HotelManager.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Utilities;

public class ViewModelLocator
{
    public static ViewModelLocator Instance { get; } = new();

    private static readonly Dictionary<Type, Type> _viewModelToViewMap = new();
    private static readonly Dictionary<Type, object> _viewModelInstances = new();

    public MainViewModel MainViewModel => GetViewModel<MainViewModel>();
    public PaymentViewModel PaymentViewModel => GetViewModel<PaymentViewModel>();

    public static void Register<TVm, TView>() where TVm : class where TView : FrameworkElement
    {
        _viewModelToViewMap[typeof(TVm)] = typeof(TView);
    }

    public static TView GetView<TVm, TView>() where TVm : class where TView : FrameworkElement
    {
        if (!_viewModelToViewMap.TryGetValue(typeof(TVm), out var viewType))
            throw new KeyNotFoundException($"No view registered for {typeof(TVm).Name}");
        return (TView)Activator.CreateInstance(viewType)!;
    }

    public static TVm GetViewModel<TVm>() where TVm : class
    {
        if (App.ServiceProvider is IServiceProvider sp)
        {
            var resolved = sp.GetService<TVm>();
            if (resolved != null) return resolved;
        }

        if (!_viewModelInstances.TryGetValue(typeof(TVm), out var vm))
        {
            vm = Activator.CreateInstance(typeof(TVm))!;
            _viewModelInstances[typeof(TVm)] = vm;
        }
        return (TVm)vm;
    }

    public static void Clear() => _viewModelInstances.Clear();
} 