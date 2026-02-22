/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Converter
 * FILE:        BooleanToVisibilityConverter.cs
 * PURPOSE:     Boolean to Visibility converter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonControls.Converter
{
    /// <summary>
    /// Boolean to Visibility converter.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BooleanToVisibilityConverter"/> is collapse.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapse; otherwise, <c>false</c>.
        /// </value>
        public bool Collapse { get; set; } = false; // true → Collapsed, false → Hidden

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
            if (value is bool b)
                return b ? Visibility.Visible : (Collapse ? Visibility.Collapsed : Visibility.Hidden);
            return Visibility.Hidden;
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
            => value is Visibility.Visible;
    }
}