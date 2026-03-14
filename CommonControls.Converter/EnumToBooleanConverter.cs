/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Converter
 * FILE:        EnumToBooleanConverter.cs
 * PURPOSE:     Enum to Boolean converter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonControls.Converter
{
    /// <summary>
    /// Converts between an <see cref="Enum"/> value and a <see cref="bool"/> for use in XAML bindings.
    /// Typically used to bind enum values to radio buttons or toggle controls:
    /// - <see cref="Convert"/> returns true if the enum value matches the converter parameter.
    /// - <see cref="ConvertBack"/> returns the enum value represented by the parameter when the boolean is true.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class EnumToBooleanConverter : IValueConverter
    {
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
            return value?.ToString() == parameter?.ToString();
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
            if ((bool)value)
                return Enum.Parse(targetType, parameter.ToString()!);
            return Binding.DoNothing;
        }
    }
}
