/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Converter
 * FILE:        NullToVisibilityConverter.cs
 * PURPOSE:     Convert null to Visibility converter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonControls.Converter
{
    /// <inheritdoc />
    /// <summary>
    /// Convert null to Visibility converter.
    /// </summary>
    /// <seealso cref="T:System.Windows.Data.IValueConverter" />
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NullToVisibilityConverter"/> is collapse.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapse; otherwise, <c>false</c>.
        /// </value>
        public bool Collapse { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NullToVisibilityConverter"/> is invert.
        /// </summary>
        /// <value>
        ///   <c>true</c> if invert; otherwise, <c>false</c>.
        /// </value>
        public bool Invert { get; set; }

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
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;

            // If we want to show when NULL, and it IS NULL, we want Visible.
            if (Invert) isNull = !isNull;

            return isNull ? (Collapse ? Visibility.Collapsed : Visibility.Hidden) : Visibility.Visible;
        }

        /// <inheritdoc />
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
            => Binding.DoNothing;
    }
}
