/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/SelectionAdorner.cs
 * PURPOSE:     Extensions for ImageZoom, handle all the selection Tools.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    /// Adorner for ImageZoom
    /// </summary>
    /// <seealso cref="T:System.Windows.Documents.Adorner" />
    internal sealed class SelectionAdorner : Adorner
    {
        /// <summary>
        /// The start point
        /// </summary>
        private Point? _startPoint;

        /// <summary>
        /// The end point
        /// </summary>
        private Point? _endPoint;

        /// <summary>
        /// The free form points
        /// </summary>
        private readonly List<Point> _freeFormPoints = new List<Point>();

        /// <summary>
        /// Gets the current selection frame.
        /// </summary>
        /// <value>
        /// The current selection frame.
        /// </value>
        public SelectionFrame CurrentSelectionFrame { get; private set; } = new SelectionFrame();

        /// <summary>
        /// The image transform
        /// Store the transform applied to the image
        /// </summary>
        private Transform _imageTransform;

        /// <summary>
        /// Gets or sets the tool.
        /// </summary>
        /// <value>
        /// The tool.
        /// </value>
        public SelectionTools Tool { get; internal set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.SelectionAdorner" /> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="tool">The tool.</param>
        /// <param name="transform">The transform.</param>
        public SelectionAdorner(UIElement adornedElement, SelectionTools tool, Transform transform = null)
            : base(adornedElement)
        {
            Tool = tool;
            _imageTransform = transform ?? Transform.Identity; // Use the provided transform, or default to Identity if none provided
        }

        /// <summary>
        /// Updates the selection for rectangle and ellipse tools.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        public void UpdateSelection(Point start, Point end)
        {
            // Apply transformation to the start and end points if necessary
            _startPoint = _imageTransform.Transform(start);
            _endPoint = _imageTransform.Transform(end);

            InvalidateVisual();
        }

        /// <summary>
        /// Adds a point for the free form tool.
        /// </summary>
        /// <param name="point">The free form point.</param>
        public void AddFreeFormPoint(Point point)
        {
            _freeFormPoints.Add(_imageTransform.Transform(point));
            InvalidateVisual();
        }

        /// <summary>
        /// Clears the free form points.
        /// </summary>
        public void ClearFreeFormPoints()
        {
            _freeFormPoints.Clear();
            InvalidateVisual();
        }

        /// <summary>
        /// Updates the image transform when the image is resized or cropped.
        /// </summary>
        /// <param name="transform">The new transform to apply.</param>
        public void UpdateImageTransform(Transform transform)
        {
            _imageTransform = transform ?? Transform.Identity; // Use the provided transform, or default to Identity if none provided
            InvalidateVisual();
        }

        /// <inheritdoc />
        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Create a dashed pen
            var dashedPen = new Pen(Brushes.Red, 2)
            {
                DashStyle = new DashStyle(new double[] { 2, 2 }, 0) // 2 pixels on, 2 pixels off
            };

            // Only render if at least one of the start or end points is available
            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                Rect selectionRect = new(_startPoint.Value, _endPoint.Value);

                // Update the CurrentSelectionFrame with the selection details
                CurrentSelectionFrame = new SelectionFrame
                {
                    X = (int)selectionRect.X,
                    Y = (int)selectionRect.Y,
                    Width = (int)selectionRect.Width,
                    Height = (int)selectionRect.Height,
                    Tool = Tool.ToString()  // Store the current tool as a string
                };

                switch (Tool)
                {
                    case SelectionTools.SelectRectangle:
                    case SelectionTools.Erase:
                        // Draw the erase area (which can behave similarly to a rectangle tool, but with different logic)
                        drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)), dashedPen, selectionRect);
                        break;

                    case SelectionTools.SelectEllipse:
                        // Calculate the center of the rectangle
                        var center = new Point(
                            selectionRect.Left + selectionRect.Width / 2,
                            selectionRect.Top + selectionRect.Height / 2);

                        // Draw the ellipse with the calculated center and half the width and height as radii
                        drawingContext.DrawEllipse(null, dashedPen, center, selectionRect.Width / 2, selectionRect.Height / 2);
                        break;

                    case SelectionTools.SelectPixel:
                        // Select a single pixel (this can be visualized as a very small rectangle)
                        drawingContext.DrawRectangle(Brushes.Red, dashedPen, new Rect(_startPoint.Value, new Size(1, 1)));
                        break;

                    case SelectionTools.FreeForm:
                        if (_freeFormPoints.Count > 1)
                        {
                            var geometry = new StreamGeometry();

                            using (var ctx = geometry.Open())
                            {
                                ctx.BeginFigure(_freeFormPoints[0], false, false);
                                ctx.PolyLineTo(_freeFormPoints.Skip(1).ToArray(), true, false);
                            }

                            drawingContext.DrawGeometry(null, dashedPen, geometry);
                        }

                        break;
                }
            }
        }
    }
}
