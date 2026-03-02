/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreViewer
 * FILE:        CoreViewer/BoolToVisibilityConverter.cs
 * PURPOSE:     Needed to convert bools to Visibility for WPF bindings.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoreViewer
{
    /// <inheritdoc />
    /// <summary>
    /// Converts a bool to Visibility (true → Visible, false → Collapsed)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible;
        }
    }
}