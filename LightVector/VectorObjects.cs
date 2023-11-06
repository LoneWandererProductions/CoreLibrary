/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/TranslatedLine.cs
 * PURPOSE:     Hold the Graphic Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LightVector
{
    /// <summary>
    ///     The graphic object class.
    ///     Contains all the basic Options
    /// </summary>
    public class GraphicObject
    {
        /// <summary>
        ///     Optional
        /// </summary>
        public int Thickness { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the stroke.
        ///     Optional
        /// </summary>
        public SolidColorBrush Stroke { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        ///     If filled we will get filled curves
        /// </summary>
        public SolidColorBrush Fill { get; set; }

        /// <summary>
        ///     Optional
        /// </summary>
        public PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;
    }

    /// <inheritdoc />
    /// <summary>
    ///     The line object class.
    /// </summary>
    public sealed class LineObject : GraphicObject
    {
        /// <summary>
        ///     Gets or sets the start point.
        /// </summary>
        public Point StartPoint { get; init; }

        /// <summary>
        ///     Gets or sets the end point.
        /// </summary>
        public Point EndPoint { get; init; }
    }

    /// <inheritdoc />
    /// <summary>
    ///     The curve object class.
    /// </summary>
    public sealed class CurveObject : GraphicObject
    {
        /// <summary>
        ///     Gets or sets the points.
        /// </summary>
        public List<Point> Points { get; set; }

        /// <summary>
        ///     Gets or sets the tension.
        /// </summary>
        public double Tension { get; init; } = 1.0;

        /// <summary>
        ///     Get the path.
        /// </summary>
        /// <returns>The <see cref="Path" /> Bezier Path.</returns>
        internal Path GetPath()
        {
            var path = CustomObjects.ReturnBezierCurve(Points, Tension);
            path.Fill = Fill;
            path.Stroke = Stroke;
            path.StrokeThickness = Thickness;
            path.StrokeLineJoin = StrokeLineJoin;
            return path;
        }

        /// <summary>
        ///     Get the points.
        /// </summary>
        /// <returns>The <see cref="List{Rectangle}" />.</returns>
        public IEnumerable<Rectangle> GetPoints()
        {
            var lst = new List<Rectangle>();

            foreach (var point in Points)
            {
                var rect = new Rectangle {Width = 3, Height = 3};
                Canvas.SetLeft(rect, point.X - 3);
                Canvas.SetTop(rect, point.Y - 3);
                rect.Fill = Brushes.White;
                rect.Stroke = Brushes.Gray;
                rect.StrokeThickness = 3;

                lst.Add(rect);
            }

            return lst;
        }
    }

    /// <summary>
    ///     TODO overthink
    ///     The save object class.
    ///     Save in a Dictionary, Id will be the Key and forwarder for the ParentId
    /// </summary>
    public sealed class SaveObject
    {
        /// <summary>
        ///     Gets or sets the point id.
        /// </summary>
        public List<int> PointIds { get; set; }

        /// <summary>
        ///     Gets or sets the parent id. Key of the Dictionary
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        ///     Gets or sets the Type.
        ///     0 is Point
        ///     1 is Line
        ///     2 is Curve
        /// </summary>
        public int Type { get; set; }
    }
}
