/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/CubicBezierCurve.cs
 * PURPOSE:     Curve object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using Mathematics;
using SkiaSharp;

namespace RenderEngine
{
    /// <inheritdoc cref="Geometry" />
    /// <summary>
    ///     Draw a Bezier Curve
    /// </summary>
    /// <seealso cref="Geometry" />
    /// <seealso cref="IDrawable" />
    public sealed class CubicBezierCurve : Geometry, IDrawable
    {
        /// <summary>
        ///     Gets or sets the first control point.
        /// </summary>
        /// <value>
        ///     The first.
        /// </value>
        public Coordinate2D First { get; set; }

        /// <summary>
        ///     Gets or sets the second control point.
        /// </summary>
        /// <value>
        ///     The first.
        /// </value>
        public Coordinate2D Second { get; set; }

        /// <summary>
        ///     Gets or sets the third control point or Ending point.
        /// </summary>
        /// <value>
        ///     The first.
        /// </value>
        public Coordinate2D Third { get; set; }

        /// <summary>
        ///     Gets or sets the width of the stroke.
        /// </summary>
        /// <value>
        ///     The width of the stroke.
        /// </value>
        public int StrokeWidth { get; set; } = 3;

        /// <inheritdoc />
        /// <summary>
        ///     Draws the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="paint">The paint.</param>
        /// <param name="style">The style.</param>
        public void Draw(SKCanvas canvas, SKPaint paint, GraphicStyle style)
        {
            using var path = new SKPath();
            // Move to the starting point
            path.MoveTo(Start.X, Start.Y);

            // Draw a cubic Bezier curve
            path.CubicTo(First.X, First.Y, Second.X, Second.Y, Third.X, Third.Y);

            switch (style)
            {
                // Choose drawing style
                case GraphicStyle.Mesh:
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = StrokeWidth;
                    break;
                case GraphicStyle.Fill:
                {
                    using var fillPaint = new SKPaint { Style = SKPaintStyle.Fill };
                    canvas.DrawPath(path, fillPaint);
                    return; // No need to draw the stroke in the Fill style
                }
                case GraphicStyle.Plot:
                    paint.StrokeWidth = StrokeWidth;
                    RenderHelper.DrawPoint(canvas, First, paint);
                    RenderHelper.DrawPoint(canvas, Second, paint);
                    RenderHelper.DrawPoint(canvas, Third, paint);
                    break;
            }

            if (RenderRegister.Debug)
            {
                Trace.WriteLine(ToString());
            }

            // Draw the path
            canvas.DrawPath(path, paint);
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var str = string.Concat(Start.ToString(), Environment.NewLine);
            str = string.Concat(str, First.ToString(), Environment.NewLine);
            str = string.Concat(str, Second.ToString(), Environment.NewLine);
            str = string.Concat(str, Third.ToString());

            return str;
        }
    }
}
