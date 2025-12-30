/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        SelectionAdorner.cs
 * PURPOSE:     Extensions for ImageZoom, handle all the selection Tools.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable BadBracesSpaces
// ReSharper disable MissingSpace

using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonControls.Images
{
    /// <inheritdoc />
    /// <summary>
    ///     Adorner for ImageZoom
    /// </summary>
    /// <seealso cref="T:System.Windows.Documents.Adorner" />
    internal sealed class SelectionAdorner : Adorner
    {
        /// <summary>
        ///     The end point
        /// </summary>
        private Point? _endPoint;

        /// <summary>
        ///     The image transform
        ///     Store the transform applied to the image
        /// </summary>
        private Transform _imageTransform;

        /// <summary>
        ///     The start point
        /// </summary>
        private Point? _startPoint;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.SelectionAdorner" /> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="tool">The tool.</param>
        /// <param name="transform">The transform.</param>
        public SelectionAdorner(UIElement adornedElement, ImageZoomTools tool, Transform transform = null)
            : base(adornedElement)
        {
            Tool = tool;
            _imageTransform =
                transform ?? Transform.Identity; // Use the provided transform, or default to Identity if none provided

            // Hook into mouse events
            adornedElement.PreviewMouseDown += OnMouseDown;
            adornedElement.PreviewMouseMove += OnMouseMove;
            adornedElement.PreviewMouseUp += OnMouseUp;
        }

        /// <summary>
        ///     The free form points
        /// </summary>
        public List<Point> FreeFormPoints { get; set; } = new();

        /// <summary>
        ///     The is tracing
        /// </summary>
        public bool IsTracing { get; set; }

        /// <summary>
        ///     Gets the current selection frame.
        /// </summary>
        /// <value>
        ///     The current selection frame.
        /// </value>
        public SelectionFrame CurrentSelectionFrame { get; private set; } = new();

        /// <summary>
        ///     Gets or sets the tool.
        /// </summary>
        /// <value>
        ///     The tool.
        /// </value>
        public ImageZoomTools Tool { get; internal set; }

        /// <summary>
        ///     Updates the selection for rectangle and ellipse tools.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        public void UpdateSelection(Point start, Point end)
        {
            // Apply transformation to the start and end points if necessary
            _startPoint = TransformMousePosition(start);
            _endPoint = TransformMousePosition(end);

            InvalidateVisual();
        }

        /// <summary>
        ///     Adds a point for the free form tool.
        /// </summary>
        /// <param name="point">The free form point.</param>
        public void AddFreeFormPoint(Point point)
        {
            FreeFormPoints.Add(_imageTransform.Transform(point));
            InvalidateVisual();
        }

        /// <summary>
        ///     Clears the free form points.
        /// </summary>
        public void ClearFreeFormPoints()
        {
            FreeFormPoints.Clear();
            InvalidateVisual();
        }

        /// <summary>
        ///     Updates the image transform when the image is resized or cropped.
        /// </summary>
        /// <param name="transform">The new transform to apply.</param>
        public void UpdateImageTransform(Transform transform)
        {
            _imageTransform = transform ?? Transform.Identity;
            InvalidateVisual();
        }

        /// <summary>
        ///     Called when [mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var transformedPoint = TransformMousePosition(e.GetPosition(this));

            if (Tool == ImageZoomTools.Trace && e.LeftButton == MouseButtonState.Pressed)
            {
                IsTracing = true;
                FreeFormPoints.Clear(); // Clear existing points for a new trace
                FreeFormPoints.Add(transformedPoint);
                CaptureMouse(); // Ensure we capture all mouse events
            }
        }

        /// <summary>
        ///     Called when [mouse move].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsTracing && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = TransformMousePosition(e.GetPosition(this));
                FreeFormPoints.Add(currentPoint);
                InvalidateVisual(); // Redraw to show the updated trace
            }
        }

        /// <summary>
        ///     Called when [mouse up].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsTracing && e.LeftButton == MouseButtonState.Released)
            {
                IsTracing = false;
                ReleaseMouseCapture(); // Release mouse capture
            }
        }

        /// <summary>
        ///     Transforms the mouse position.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        /// <returns>Transformed Point.</returns>
        private Point TransformMousePosition(Point mousePosition)
        {
            if (_imageTransform == null)
            {
                return mousePosition;
            }

            return _imageTransform.Inverse.Transform(mousePosition);
        }

        /// <inheritdoc />
        /// <summary>
        ///     When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        ///     The rendering instructions for this element are not used directly when this method is invoked, and are instead
        ///     preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        ///     The drawing instructions for a specific element. This context is provided to the layout
        ///     system.
        /// </param>
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
                    Tool = Tool // Store the current tool as a string
                };

                switch (Tool)
                {
                    case ImageZoomTools.Rectangle:
                        // Draw the erase area (which can behave similarly to a rectangle tool, but with different logic)
                        drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)), dashedPen,
                            selectionRect);
                        break;

                    case ImageZoomTools.Ellipse:
                        // Calculate the center of the rectangle
                        var center = new Point(
                            selectionRect.Left + selectionRect.Width / 2,
                            selectionRect.Top + selectionRect.Height / 2);

                        // Draw the ellipse with the calculated center and half the width and height as radii
                        drawingContext.DrawEllipse(null, dashedPen, center, selectionRect.Width / 2,
                            selectionRect.Height / 2);
                        break;

                    case ImageZoomTools.Dot:
                        // Select a single pixel (this can be visualized as a very small rectangle)
                        drawingContext.DrawRectangle(Brushes.Red, dashedPen,
                            new Rect(_startPoint.Value, new Size(1, 1)));
                        break;

                    case ImageZoomTools.FreeForm:
                        if (FreeFormPoints.Count > 1)
                        {
                            var geometry = new StreamGeometry();

                            using (var ctx = geometry.Open())
                            {
                                ctx.BeginFigure(FreeFormPoints[0], false, false);
                                ctx.PolyLineTo(FreeFormPoints.Skip(1).ToArray(), true, false);
                            }

                            drawingContext.DrawGeometry(null, dashedPen, geometry);
                        }

                        break;
                }
            }
        }
    }
}
