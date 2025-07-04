using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HotelManager.Views.Common;

namespace HotelManager.Converters;

public class LoginViewVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If current view is LoginView – hide (Collapsed); otherwise Visible
        return value is LoginView ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
} 