/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        WpfVectorRenderer.cs
 * PURPOSE:     On possible Contract to display our vector image
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LightVector.Enums;
using LightVector.Interfaces;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     Sample Implementation for an WPF Renderer
    /// </summary>
    /// <seealso cref="IVectorRenderer" />
    public class WpfVectorRenderer : IVectorRenderer
    {
        /// <summary>
        ///     The vector objects
        /// </summary>
        private readonly List<SaveObject> _vectorObjects;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WpfVectorRenderer" /> class.
        /// </summary>
        /// <param name="vectorObjects">The vector objects.</param>
        public WpfVectorRenderer(IEnumerable<SaveObject> vectorObjects)
        {
            _vectorObjects = vectorObjects.OrderBy(v => v.Layer).ToList(); // Ensure rendering order
        }

        /// <inheritdoc />
        public object RenderToContainer()
        {
            var canvas = new Canvas();

            // If no objects, return an empty 0x0 canvas
            if (!_vectorObjects.Any()) return canvas;

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            // Step 1: Create the shapes and find the global bounding box
            foreach (var obj in _vectorObjects)
            {
                var shape = ConvertToWpfObject(obj);
                if (shape == null) continue;

                // Temporarily set position based on start coordinates
                Canvas.SetLeft(shape, obj.StartCoordinates.X);
                Canvas.SetTop(shape, obj.StartCoordinates.Y);
                canvas.Children.Add(shape);

                // Get bounds relative to the object's StartCoordinates
                var bounds = GetGraphicBounds(obj);

                // Track the absolute world-space edges
                minX = Math.Min(minX, obj.StartCoordinates.X + bounds.Left);
                minY = Math.Min(minY, obj.StartCoordinates.Y + bounds.Top);
                maxX = Math.Max(maxX, obj.StartCoordinates.X + bounds.Right);
                maxY = Math.Max(maxY, obj.StartCoordinates.Y + bounds.Bottom);
            }

            // Step 2: Calculate actual total dimensions
            var totalWidth = maxX - minX;
            var totalHeight = maxY - minY;

            canvas.Width = Math.Max(0, totalWidth);
            canvas.Height = Math.Max(0, totalHeight);

            // Step 3: Normalization Shift
            // If our minX is -105, we need to add +105 to every object 
            // so the drawing starts at 0 on the canvas and doesn't get clipped.
            foreach (FrameworkElement child in canvas.Children)
            {
                var currentLeft = Canvas.GetLeft(child);
                var currentTop = Canvas.GetTop(child);

                Canvas.SetLeft(child, currentLeft - minX);
                Canvas.SetTop(child, currentTop - minY);
            }

            return canvas;
        }

        /// <inheritdoc />
        public ImageSource RenderToImage()
        {
            var canvas = (Canvas)RenderToContainer();

            // Use the width/height we calculated in RenderToContainer
            var width = (int)Math.Ceiling(canvas.Width);
            var height = (int)Math.Ceiling(canvas.Height);

            var renderBitmap = new RenderTargetBitmap(
                width > 0 ? width : 1,
                height > 0 ? height : 1,
                96, 96, PixelFormats.Pbgra32);

            canvas.Measure(new Size(width, height));
            canvas.Arrange(new Rect(0, 0, width, height));

            renderBitmap.Render(canvas);
            return renderBitmap;
        }

        /// <summary>
        ///     Converts to WPF object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        /// <exception cref="NotSupportedException">Unknown graphic type: {obj.Type}</exception>
        private static Shape ConvertToWpfObject(SaveObject obj)
        {
            // 1. GEOMETRY CREATION (Local Space 0,0)
            // We do NOT pass StartCoordinates here. 
            // The object is created at (0,0) so the Canvas can move it later.
            Shape wpfShape = obj.Type switch
            {
                GraphicTypes.Line => CreateLine((LineObject)obj.Graphic),
                GraphicTypes.Circle => CreateCircle((CircleObject)obj.Graphic),
                GraphicTypes.Oval => CreateOval((OvalObject)obj.Graphic),
                GraphicTypes.Polygon => CreatePolygon((PolygonObject)obj.Graphic),
                GraphicTypes.BezierCurve => CreateBezier((BezierCurve)obj.Graphic),
                _ => throw new NotSupportedException($"Unknown graphic type: {obj.Type}")
            };

            // 2. STYLE INTEGRATION (WPF Brushes & Strokes)
            // This turns your generic Hex strings into actual WPF resources
            var graphic = obj.Graphic;

            wpfShape.Stroke = ParseColor(graphic.Stroke);
            wpfShape.Fill = ParseColor(graphic.Fill);
            wpfShape.StrokeThickness = graphic.Thickness;

            wpfShape.StrokeLineJoin = graphic.StrokeLineJoin switch
            {
                VectorLineJoin.Bevel => PenLineJoin.Bevel,
                VectorLineJoin.Miter => PenLineJoin.Miter,
                VectorLineJoin.Round => PenLineJoin.Round,
                _ => PenLineJoin.Bevel
            };

            return wpfShape;
        }

        /// <summary>
        ///     Creates the line.
        /// </summary>
        /// <param name="lineObject">The line object.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Line CreateLine(LineObject lineObject)
        {
            // INTEGRATION NOTE: 
            // We set X1/Y1 to 0. 
            // The 'StartCoordinates' from SaveObject will be applied via Canvas.SetLeft/Top
            // outside of this method.
            return new Line { X1 = 0, Y1 = 0, X2 = lineObject.Direction.X, Y2 = lineObject.Direction.Y };
        }

        /// <summary>
        ///     Creates the polygon.
        /// </summary>
        /// <param name="polygonObject">The polygon object.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Polygon CreatePolygon(PolygonObject polygonObject)
        {
            // PURE GEOMETRY: Just the relative vertices.
            var points = new PointCollection(polygonObject.Vertices.Select(v => new Point(v.X, v.Y)));

            return new Polygon { Points = points };
        }

        /// <summary>
        ///     Creates the circle.
        /// </summary>
        /// <param name="circleObject">The circle object.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Path CreateCircle(CircleObject circleObject)
        {
            // CENTER IS (0,0). Canvas moves it to the right spot.
            var circle = new EllipseGeometry(new Point(0, 0), circleObject.Radius, circleObject.Radius);

            return new Path { Data = circle };
        }

        /// <summary>
        ///     Creates the oval.
        /// </summary>
        /// <param name="ovalObject">The oval object.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Path CreateOval(OvalObject ovalObject)
        {
            var oval = new EllipseGeometry(new Point(0, 0), ovalObject.RadiusX, ovalObject.RadiusY);

            return new Path { Data = oval };
        }

        private static Path CreateBezier(BezierCurve bezierObject)
        {
            // Assuming your Factory returns a generic Geometry or PathGeometry
            // Ensure your Factory doesn't add offsets internally!
            return new Path
            {
                Data = BezierCurveFactory.CreateBezierGeometry(bezierObject.Vectors, bezierObject.Tension)
            };
        }

        /// <summary>
        /// Parses Hex String to WPF Brush (Optimized).
        /// </summary>
        /// <param name="colorString">The color string.</param>
        /// <returns>Parsed Hex to wpf color.</returns>
        private static SolidColorBrush ParseColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString)) return Brushes.Transparent;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                var brush = new SolidColorBrush(color);

                // IMPORTANT: Freezing makes rendering significantly faster
                if (brush.CanFreeze) brush.Freeze();
                return brush;
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        /// <summary>
        /// Gets the graphic bounds.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static Rect GetGraphicBounds(SaveObject obj)
        {
            // Padding to account for half the stroke thickness so edges aren't clipped
            var padding = obj.Graphic.Thickness / 2.0;

            switch (obj.Type)
            {
                case GraphicTypes.Line:
                    var l = (LineObject)obj.Graphic;
                    // A line can go negative relative to start (e.g., direction -50, -50)
                    double x = Math.Min(0, l.Direction.X);
                    double y = Math.Min(0, l.Direction.Y);
                    double width = Math.Abs(l.Direction.X);
                    double height = Math.Abs(l.Direction.Y);
                    return new Rect(x - padding, y - padding, width + obj.Graphic.Thickness,
                        height + obj.Graphic.Thickness);

                case GraphicTypes.Circle:
                    var c = (CircleObject)obj.Graphic;
                    // EllipseGeometry in your code is centered on the start point
                    return new Rect(-c.Radius - padding, -c.Radius - padding,
                        (c.Radius * 2) + obj.Graphic.Thickness, (c.Radius * 2) + obj.Graphic.Thickness);

                case GraphicTypes.Oval:
                    var o = (OvalObject)obj.Graphic;
                    return new Rect(-o.RadiusX - padding, -o.RadiusY - padding,
                        (o.RadiusX * 2) + obj.Graphic.Thickness, (o.RadiusY * 2) + obj.Graphic.Thickness);

                case GraphicTypes.Polygon:
                    var p = (PolygonObject)obj.Graphic;
                    if (p.Vertices == null || p.Vertices.Count == 0) return new Rect(0, 0, 0, 0);

                    // Find the extremes of all vertices
                    double minX = p.Vertices.Min(v => v.X);
                    double minY = p.Vertices.Min(v => v.Y);
                    double maxX = p.Vertices.Max(v => v.X);
                    double maxY = p.Vertices.Max(v => v.Y);

                    return new Rect(minX - padding, minY - padding,
                        (maxX - minX) + obj.Graphic.Thickness, (maxY - minY) + obj.Graphic.Thickness);

                case GraphicTypes.BezierCurve:
                    var b = (BezierCurve)obj.Graphic;
                    if (b.Vectors == null || b.Vectors.Count == 0) return new Rect(0, 0, 0, 0);

                    // Note: Mathematical bounds of a Bezier are complex, but for a 
                    // "Dumb but Reliable" version, the bounding box of the control points 
                    // is guaranteed to contain the curve.
                    double bMinX = b.Vectors.Min(v => v.X);
                    double bMinY = b.Vectors.Min(v => v.Y);
                    double bMaxX = b.Vectors.Max(v => v.X);
                    double bMaxY = b.Vectors.Max(v => v.Y);

                    return new Rect(bMinX - padding, bMinY - padding,
                        (bMaxX - bMinX) + obj.Graphic.Thickness, (bMaxY - bMinY) + obj.Graphic.Thickness);

                default:
                    return new Rect(0, 0, 0, 0);
            }
        }
    }
}
