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
    public class WpfVectorRenderer : IVectorRenderer
    {
        private readonly List<SaveObject> _vectorObjects;

        public WpfVectorRenderer(IEnumerable<SaveObject> vectorObjects)
        {
            _vectorObjects = vectorObjects.OrderBy(v => v.Layer).ToList(); // Ensure rendering order
        }

        public object RenderToContainer()
        {
            var canvas = new Canvas();

            foreach (var obj in _vectorObjects)
            {
                var shape = ConvertToWpfObject(obj);

                // Apply position offset based on StartCoordinates
                if (shape != null)
                {
                    Canvas.SetLeft(shape, obj.StartCoordinates.X);
                    Canvas.SetTop(shape, obj.StartCoordinates.Y);
                    canvas.Children.Add(shape);
                }
            }

            return canvas;
        }

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

        private Shape ConvertToWpfObject(SaveObject obj)
        {
            switch (obj.Type)
            {
                case GraphicTypes.Line:
                    var lineObject = (LineObject)obj.Graphic;
                    return CreateLine(lineObject, obj.StartCoordinates);

                case GraphicTypes.Point:
                    break;
                case GraphicTypes.Curve:
                    break;
                case GraphicTypes.Polygon:
                    break;
                case GraphicTypes.Circle:
                    break;
                case GraphicTypes.Oval:
                    break;
                default:
                    throw new NotSupportedException($"Unknown graphic type: {obj.Type}");
            }

            return null;
        }

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
    }
}
