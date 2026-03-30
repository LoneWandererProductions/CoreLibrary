/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Converter
 * FILE:        BooleanToVisibilityConverter.cs
 * PURPOSE:     Boolean to Visibility converter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.Converter
{
    /// <inheritdoc />
    /// <summary>
    /// Boolean to Visibility converter.
    /// </summary>
    /// <seealso cref="T:System.Windows.Data.IValueConverter" />
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BooleanToVisibilityConverter"/> is collapse.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapse; otherwise, <c>false</c>.
        /// </value>
        public bool Collapse { get; set; } = false; // true → Collapsed, false → Hidden

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
            var b = value is true;

            if (b)
            {
                return Visibility.Visible;
            }

            return Collapse ? Visibility.Collapsed : Visibility.Hidden;
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
            => value is Visibility.Visible;
    }
}
