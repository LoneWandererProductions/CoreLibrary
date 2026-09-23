/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Converter
 * FILE:        EnumToBooleanConverter.cs
 * PURPOSE:     Enum to Boolean converter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Globalization;
using System.Windows.Data;

namespace Common.Converter
{
    /// <inheritdoc />
    /// <summary>
    /// Converts between an <see cref="T:System.Enum" /> value and a <see cref="T:System.Boolean" /> for use in XAML bindings.
    /// Typically used to bind enum values to radio buttons or toggle controls:
    /// - <see cref="M:Common.Converter.EnumToBooleanConverter.Convert(System.Object,System.Type,System.Object,System.Globalization.CultureInfo)" /> returns true if the enum value matches the converter parameter.
    /// - <see cref="M:Common.Converter.EnumToBooleanConverter.ConvertBack(System.Object,System.Type,System.Object,System.Globalization.CultureInfo)" /> returns the enum value represented by the parameter when the boolean is true.
    /// </summary>
    /// <seealso cref="T:System.Windows.Data.IValueConverter" />
    public class EnumToBooleanConverter : IValueConverter
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
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
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
        {
            if ((bool)value)
                return Enum.Parse(targetType, parameter.ToString()!);

            return Binding.DoNothing;
        }
    }
}
