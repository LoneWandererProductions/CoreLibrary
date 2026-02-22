/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Converter
 * FILE:        ColorToNameConverter.cs
 * PURPOSE:     Convert between Color and its name as string (e.g., "Red", "Blue", etc.) for WPF bindings.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;

namespace CommonControls.Converters
{
    /// <summary>
    /// Converter that converts between a Color and its name as a string for WPF bindings. It uses reflection to find the name of the color in System.Windows.Media.Colors when converting from Color to string, and uses ColorConverter to convert from string to Color.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ColorToNameConverter : IValueConverter
    {
        /// <summary>
        /// Convert from Color (ViewModel) to string (Control)
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
            if (value is Color color)
            {
                // Find the name of the color in System.Windows.Media.Colors
                var colorProperty = typeof(Colors).GetProperties()
                    .FirstOrDefault(p => (Color)p.GetValue(null, null) == color);

                return colorProperty?.Name ?? color.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Convert from string (Control) to Color (ViewModel)
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
            if (value is string colorName && !string.IsNullOrEmpty(colorName))
            {
                try
                {
                    return ColorConverter.ConvertFromString(colorName);
                }
                catch { return Colors.Transparent; }
            }
            return Colors.Transparent;
        }
    }
}