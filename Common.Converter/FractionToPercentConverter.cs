/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Converter
 * FILE:        FractionToPercentConverter.cs
 * PURPOSE:     Lets a TextBox edit a 0..1 fraction (e.g. DrawingState.BrushOpacity) as a whole-number
 *              percentage, matching how it is already displayed elsewhere (StringFormat={}{0:P0}). Typing
 *              something that doesn't parse as a number leaves the bound value untouched rather than
 *              throwing - the user just keeps editing.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Globalization;
using System.Windows.Data;

namespace Common.Converter
{
    /// <inheritdoc />
    public sealed class FractionToPercentConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double fraction ? Math.Round(fraction * 100).ToString(culture) : "0";
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && double.TryParse(text, NumberStyles.Any, culture, out var percent))
            {
                return percent / 100.0;
            }

            // Not a number (yet, or user cleared the box mid-edit): leave the source alone instead of
            // throwing or forcing a value - the TextBox keeps whatever was typed until it parses.
            return Binding.DoNothing;
        }
    }
}