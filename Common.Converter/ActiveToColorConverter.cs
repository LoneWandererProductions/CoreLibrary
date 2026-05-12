/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Converter
 * FILE:        ActiveToColorConverter.cs
 * PURPOSE:     Convert a boolean value to a color for WPF bindings.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Converter
{
    /// <summary>
    /// Converter that converts a boolean value to a color for WPF bindings. It returns Gold for true (active) and Dark Gray for false (inactive).
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ActiveToColorConverter : IValueConverter
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
            if (value is bool isActive && isActive)
            {
                return new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Gold for Active
            }
            return new SolidColorBrush(Color.FromRgb(60, 60, 60)); // Dark Gray for Inactive
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
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                if (color == Color.FromRgb(255, 215, 0)) // Gold
                {
                    return true;
                }
                if (color == Color.FromRgb(60, 60, 60)) // Dark Gray
                {
                    return false;
                }
            }
            return false;   
        }
    }
}
