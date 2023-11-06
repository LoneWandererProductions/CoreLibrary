/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/GraphicHelper.cs
 * PURPOSE:     Internal helper
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LightVector
{
    /// <summary>
    ///     The graphic helper class.
    /// </summary>
    internal static class GraphicHelper
    {
        /// <summary>
        ///     Get the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The <see cref="Rectangle" />.</returns>
        internal static Rectangle GetPoint(Point point)
        {
            var rect = new Rectangle {Width = 3, Height = 3};
            Canvas.SetLeft(rect, point.X - 3);
            Canvas.SetTop(rect, point.Y - 3);
            rect.Fill = Brushes.White;
            rect.Stroke = Brushes.Gray;
            rect.StrokeThickness = 3;

            return rect;
        }
    }
}
