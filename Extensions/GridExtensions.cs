using System;
using System.Windows;
using System.Windows.Controls;

namespace HotelManager.Extensions
{
    public static class GridExtensions
    {
        #region Spacing Attached Property

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(GridExtensions),
                new PropertyMetadata(0.0, OnSpacingChanged));

        public static double GetSpacing(DependencyObject obj)
        {
            return (double)obj.GetValue(SpacingProperty);
        }

        public static void SetSpacing(DependencyObject obj, double value)
        {
            obj.SetValue(SpacingProperty, value);
        }

        private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid grid)
            {
                var spacing = (double)e.NewValue;
                ApplySpacing(grid, spacing);

                // Monitor children changes
                grid.LayoutUpdated += (s, args) => ApplySpacing(grid, spacing);
            }
        }

        private static void ApplySpacing(Grid grid, double spacing)
        {
            if (spacing <= 0) return;

            foreach (UIElement child in grid.Children)
            {
                if (child is FrameworkElement element)
                {
                    var margin = element.Margin;
                    element.Margin = new Thickness(
                        margin.Left + spacing,
                        margin.Top + spacing,
                        margin.Right + spacing,
                        margin.Bottom + spacing);
                }
            }
        }

        #endregion

        #region AutoSize Attached Property

        public static readonly DependencyProperty AutoSizeColumnsProperty =
            DependencyProperty.RegisterAttached(
                "AutoSizeColumns",
                typeof(bool),
                typeof(GridExtensions),
                new PropertyMetadata(false, OnAutoSizeColumnsChanged));

        public static bool GetAutoSizeColumns(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSizeColumnsProperty);
        }

        public static void SetAutoSizeColumns(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSizeColumnsProperty, value);
        }

        private static void OnAutoSizeColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid grid && (bool)e.NewValue)
            {
                grid.SizeChanged += (s, args) => AutoSizeColumns(grid);
                AutoSizeColumns(grid);
            }
        }

        private static void AutoSizeColumns(Grid grid)
        {
            if (grid.ColumnDefinitions.Count == 0) return;

            var availableWidth = grid.ActualWidth;
            var columnWidth = availableWidth / grid.ColumnDefinitions.Count;

            foreach (var column in grid.ColumnDefinitions)
            {
                column.Width = new GridLength(columnWidth, GridUnitType.Pixel);
            }
        }

        #endregion

        #region AutoSize Rows Attached Property

        public static readonly DependencyProperty AutoSizeRowsProperty =
            DependencyProperty.RegisterAttached(
                "AutoSizeRows",
                typeof(bool),
                typeof(GridExtensions),
                new PropertyMetadata(false, OnAutoSizeRowsChanged));

        public static bool GetAutoSizeRows(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSizeRowsProperty);
        }

        public static void SetAutoSizeRows(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSizeRowsProperty, value);
        }

        private static void OnAutoSizeRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid grid && (bool)e.NewValue)
            {
                grid.SizeChanged += (s, args) => AutoSizeRows(grid);
                AutoSizeRows(grid);
            }
        }

        private static void AutoSizeRows(Grid grid)
        {
            if (grid.RowDefinitions.Count == 0) return;

            var availableHeight = grid.ActualHeight;
            var rowHeight = availableHeight / grid.RowDefinitions.Count;

            foreach (var row in grid.RowDefinitions)
            {
                row.Height = new GridLength(rowHeight, GridUnitType.Pixel);
            }
        }

        #endregion
    }
} 