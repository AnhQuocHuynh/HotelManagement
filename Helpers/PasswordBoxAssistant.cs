using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HotelManager.Helpers;

/// <summary>
/// Helper to allow binding the Password property of a <see cref="PasswordBox"/> in MVVM scenarios.
/// </summary>
public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword",
        typeof(string),
        typeof(PasswordBoxAssistant),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPasswordProperty, value);

    private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
        "IsUpdating",
        typeof(bool),
        typeof(PasswordBoxAssistant));

    private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);

    private static void SetIsUpdating(DependencyObject obj, bool value) => obj.SetValue(IsUpdatingProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
            return;

        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

        if (!GetIsUpdating(passwordBox))
        {
            passwordBox.Password = e.NewValue as string ?? string.Empty;
        }

        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        SetIsUpdating(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);

        // Force binding update to ensure ViewModel is updated immediately
        var bindingExpression = BindingOperations.GetBindingExpression(passwordBox, BoundPasswordProperty);
        bindingExpression?.UpdateSource();

        SetIsUpdating(passwordBox, false);
    }

    /// <summary>
    /// Sets up password binding for a PasswordBox
    /// </summary>
    /// <param name="passwordBox">The PasswordBox to bind</param>
    /// <param name="password">The password to bind</param>
    public static void SetBindPassword(PasswordBox passwordBox, string password)
    {
        if (passwordBox != null)
        {
            SetBoundPassword(passwordBox, password);
        }
    }
} 