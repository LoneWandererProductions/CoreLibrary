/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/WpfVectorRenderer.cs
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

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

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
            double totalWidth = maxX - minX;
            double totalHeight = maxY - minY;

            canvas.Width = Math.Max(0, totalWidth);
            canvas.Height = Math.Max(0, totalHeight);

            // Step 3: Normalization Shift
            // If our minX is -105, we need to add +105 to every object 
            // so the drawing starts at 0 on the canvas and doesn't get clipped.
            foreach (FrameworkElement child in canvas.Children)
            {
                double currentLeft = Canvas.GetLeft(child);
                double currentTop = Canvas.GetTop(child);

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
            int width = (int)Math.Ceiling(canvas.Width);
            int height = (int)Math.Ceiling(canvas.Height);

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
            switch (obj.Type)
            {
                case GraphicTypes.Line:
                    var lineObject = (LineObject)obj.Graphic;
                    return CreateLine(lineObject, obj.StartCoordinates);

                case GraphicTypes.Point:
                    break;

                case GraphicTypes.BezierCurve:
                    var bezierObject = (BezierCurve)obj.Graphic;
                    var tension = bezierObject.Tension; // Default tension if not provided
                    return BezierCurveFactory.CreateBezierCurve(bezierObject.Vectors,
                        tension); // Now returns the generated path from the factory

                case GraphicTypes.Polygon:
                    var polygonObject = (PolygonObject)obj.Graphic;
                    return CreatePolygon(polygonObject, obj.StartCoordinates);

                case GraphicTypes.Circle:
                    var circleObject = (CircleObject)obj.Graphic;
                    return CreateCircle(circleObject, obj.StartCoordinates);

                case GraphicTypes.Oval:
                    var ovalObject = (OvalObject)obj.Graphic;
                    return CreateOval(ovalObject, obj.StartCoordinates);

                default:
                    throw new NotSupportedException($"Unknown graphic type: {obj.Type}");
            }

            return null;
        }

        /// <summary>
        ///     Creates the line.
        /// </summary>
        /// <param name="lineObject">The line object.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Line CreateLine(LineObject lineObject, Point startCoordinates)
        {
            return new Line
            {
                X1 = 0, // Relative to the Canvas.SetLeft
                Y1 = 0,
                X2 = lineObject.Direction.X,
                Y2 = lineObject.Direction.Y,
                Stroke = lineObject.Stroke ?? Brushes.Black,
                StrokeThickness = lineObject.Thickness
            };
        }

        /// <summary>
        ///     Creates the polygon.
        /// </summary>
        /// <param name="polygonObject">The polygon object.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Shape CreatePolygon(PolygonObject polygonObject, Point startCoordinates)
        {
            var points = new PointCollection(polygonObject.Vertices.Select(v => new Point(v.X, v.Y)));

            return new Polygon
            {
                Points = points,
                Stroke = polygonObject.Stroke ?? Brushes.Black,
                Fill = polygonObject.Fill ?? Brushes.Transparent,
                StrokeThickness = polygonObject.Thickness
            };
        }

        /// <summary>
        ///     Creates the circle.
        /// </summary>
        /// <param name="circleObject">The circle object.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Shape CreateCircle(CircleObject circleObject, Point startCoordinates)
        {
            // Use (0,0) as the center point. Canvas.SetLeft/Top will move it to the right place.
            var circle = new EllipseGeometry(new Point(0, 0), circleObject.Radius, circleObject.Radius);

            return new Path
            {
                Data = circle,
                Stroke = circleObject.Stroke ?? Brushes.Black,
                Fill = circleObject.Fill ?? Brushes.Transparent,
                StrokeThickness = circleObject.Thickness
            };
        }

        /// <summary>
        ///     Creates the oval.
        /// </summary>
        /// <param name="ovalObject">The oval object.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <returns>Image Object to Wpf Shape</returns>
        private static Shape CreateOval(OvalObject ovalObject, Point startCoordinates)
        {
            // Same as circle: center at 0,0
            var oval = new EllipseGeometry(new Point(0, 0), ovalObject.RadiusX, ovalObject.RadiusY);

            return new Path
            {
                Data = oval,
                Stroke = ovalObject.Stroke ?? Brushes.Black,
                Fill = ovalObject.Fill ?? Brushes.Transparent,
                StrokeThickness = ovalObject.Thickness
            };
        }

        /// <summary>
        /// Gets the graphic bounds.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static Rect GetGraphicBounds(SaveObject obj)
        {
            // Padding to account for half the stroke thickness so edges aren't clipped
            double padding = obj.Graphic.Thickness / 2.0;

            switch (obj.Type)
            {
                case GraphicTypes.Line:
                    var l = (LineObject)obj.Graphic;
                    // A line can go negative relative to start (e.g., direction -50, -50)
                    double x = Math.Min(0, l.Direction.X);
                    double y = Math.Min(0, l.Direction.Y);
                    double width = Math.Abs(l.Direction.X);
                    double height = Math.Abs(l.Direction.Y);
                    return new Rect(x - padding, y - padding, width + obj.Graphic.Thickness, height + obj.Graphic.Thickness);

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
