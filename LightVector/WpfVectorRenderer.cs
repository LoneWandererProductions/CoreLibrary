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

namespace LightVector;

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
    /// <summary>
    ///     Renders to container.
    /// </summary>
    /// <returns>
    ///     Returns a framework-specific container
    /// </returns>
    public object RenderToContainer()
    {
        var canvas = new Canvas();

        // Initialize variables to track the boundaries of the canvas
        double canvasWidth = 0;
        double canvasHeight = 0;

        foreach (var obj in _vectorObjects)
        {
            var shape = ConvertToWpfObject(obj);

            // Apply position offset based on StartCoordinates
            if (shape == null)
            {
                continue;
            }

            Canvas.SetLeft(shape, obj.StartCoordinates.X);
            Canvas.SetTop(shape, obj.StartCoordinates.Y);
            _ = canvas.Children.Add(shape);

            // Update canvas size based on the current object
            canvasWidth = Math.Max(canvasWidth, obj.StartCoordinates.X + shape.RenderSize.Width);
            canvasHeight = Math.Max(canvasHeight, obj.StartCoordinates.Y + shape.RenderSize.Height);
        }

        // Set the canvas size to fit all objects
        canvas.Width = canvasWidth;
        canvas.Height = canvasHeight;

        return canvas;
    }


    /// <inheritdoc />
    /// <summary>
    ///     Renders to image.
    /// </summary>
    /// <returns>
    ///     Converts to a WPF Image
    /// </returns>
    public ImageSource RenderToImage()
    {
        var canvas = (Canvas)RenderToContainer();
        var renderBitmap = new RenderTargetBitmap(
            500, 500, 96, 96, PixelFormats.Pbgra32);

        canvas.Measure(new Size(500, 500));
        canvas.Arrange(new Rect(0, 0, 500, 500));

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
        // Calculate the end point based on the start coordinates and direction
        var endPoint = new Point(
            startCoordinates.X + lineObject.Direction.X,
            startCoordinates.Y + lineObject.Direction.Y);

        // Create the Line and set its properties
        return new Line
        {
            X1 = startCoordinates.X, // Start position
            Y1 = startCoordinates.Y,
            X2 = endPoint.X, // End position calculated from the start coordinates + direction
            Y2 = endPoint.Y,
            Stroke = lineObject.Stroke ?? Brushes.Black, // Use stroke from LineObject
            StrokeThickness = lineObject.Thickness, // Use thickness from LineObject
            StrokeLineJoin = lineObject.StrokeLineJoin // Stroke line join (optional, for appearance)
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
        // Convert the list of vertices into a collection of Points, offset by the startCoordinates
        var points = new PointCollection(polygonObject.Vertices.Select(v =>
            new Point(v.X + startCoordinates.X, v.Y + startCoordinates.Y)));

        return new Polygon
        {
            Points = points,
            Stroke = polygonObject.Stroke ?? Brushes.Black, // Default stroke if not provided
            Fill = polygonObject.Fill ?? Brushes.Transparent, // Default fill if not provided
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
        var circle = new EllipseGeometry(new Point(startCoordinates.X, startCoordinates.Y), circleObject.Radius,
            circleObject.Radius);

        return new Path
        {
            Data = circle,
            Stroke = circleObject.Stroke ?? Brushes.Black, // Default stroke if not provided
            Fill = circleObject.Fill ?? Brushes.Transparent, // Default fill if not provided
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
        var oval = new EllipseGeometry(new Point(startCoordinates.X, startCoordinates.Y), ovalObject.RadiusX,
            ovalObject.RadiusY);

        return new Path
        {
            Data = oval,
            Stroke = ovalObject.Stroke ?? Brushes.Black, // Default stroke if not provided
            Fill = ovalObject.Fill ?? Brushes.Transparent, // Default fill if not provided
            StrokeThickness = ovalObject.Thickness
        };
    }
}
