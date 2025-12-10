/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorPickerHelper.cs
 * PURPOSE:     Helper functions for  ColorPicker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Media;
using System.Windows.Shapes;
using Imaging;

namespace CommonControls;

/// <summary>
///     Helper Class for the ColorPicker
/// </summary>
internal static class ColorPickerHelper
{
    /// <summary>
    ///     Gets the color preview.
    /// </summary>
    /// <param name="colorHsv"></param>
    /// <returns>A Rectangle</returns>
    internal static Rectangle GetColorPreview(ColorHsv colorHsv)
    {
        var rectangle = new Rectangle { Width = 25, Height = 25 };

        var col = new Color { A = (byte)colorHsv.A, R = (byte)colorHsv.R, G = (byte)colorHsv.G, B = (byte)colorHsv.B };
        rectangle.Fill = new SolidColorBrush { Color = col };

        return rectangle;
    }
}