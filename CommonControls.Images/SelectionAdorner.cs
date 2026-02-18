/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        SelectionAdorner.cs
 * PURPOSE:     Extensions for ImageZoom, handle all the selection Tools.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable BadBracesSpaces
// ReSharper disable MissingSpace

using System.Collections.Generic;
using System.Linq;
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

        // NEW: Store completed frames here
        private readonly List<SelectionFrame> _committedFrames = new();

        // NEW: Store completed FreeForm paths here
        private readonly List<List<Point>> _committedFreeForms = new();

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
        ///     Gets a list of all frames committed during this session.
        /// </summary>
        public List<SelectionFrame> GetCommittedFrames()
        {
            return new List<SelectionFrame>(_committedFrames);
        }

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
            // Use TransformMousePosition (Inverse) to store the point in Logic/Image space
            FreeFormPoints.Add(TransformMousePosition(point));
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
        ///     Commits the current shape to the internal list and clears the temporary drawing data.
        /// </summary>
        public void CommitCurrentShape()
        {
            if (Tool == ImageZoomTools.FreeForm)
            {
                if (FreeFormPoints.Count > 1)
                {
                    // Clone points to a new list
                    _committedFreeForms.Add(new List<Point>(FreeFormPoints));

                    // Add to frames list for data export (bounding box of the freeform)
                    // Logic to calculate bounding box of freeform can go here if needed
                }

                FreeFormPoints.Clear();
            }
            else if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var selectionRect = new Rect(_startPoint.Value, _endPoint.Value);

                var frame = new SelectionFrame
                {
                    X = (int)selectionRect.X,
                    Y = (int)selectionRect.Y,
                    Width = (int)selectionRect.Width,
                    Height = (int)selectionRect.Height,
                    Tool = Tool
                };

                _committedFrames.Add(frame);

                // Reset current points
                _startPoint = null;
                _endPoint = null;
            }

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

        /// <summary>
        /// Captures the current shape data, returns it, and immediately clears the visual drawing.
        /// </summary>
        /// <summary>
        /// Captures the current shape data, returns it, and immediately clears the visual drawing.
        /// </summary>
        public SelectionFrame CaptureAndClear()
        {
            // 1. Prepare data variables
            int x = 0, y = 0, width = 0, height = 0;
            List<Point> points = new List<Point>();

            // 2. Calculate based on Tool Type
            if (Tool == ImageZoomTools.FreeForm || Tool == ImageZoomTools.Trace)
            {
                if (FreeFormPoints.Count > 0)
                {
                    // Copy existing points
                    points = new List<Point>(FreeFormPoints);

                    // --- FIX: AUTO-CLOSE THE SHAPE ---
                    // If we have a valid shape (>2 points) and it isn't closed, close it.
                    // This ensures the "Fill" algorithm knows where the boundary is.
                    if (points.Count > 2)
                    {
                        var start = points[0];
                        var end = points[points.Count - 1];

                        // Simple check: if start != end, add start to the end
                        if (start != end)
                        {
                            points.Add(start);
                        }
                    }
                    // ---------------------------------

                    // Calculate Bounding Box (after closing)
                    double minX = points.Min(p => p.X);
                    double minY = points.Min(p => p.Y);
                    double maxX = points.Max(p => p.X);
                    double maxY = points.Max(p => p.Y);

                    x = (int)minX;
                    y = (int)minY;
                    width = (int)(maxX - minX);
                    height = (int)(maxY - minY);
                }
            }
            else if (_startPoint.HasValue && _endPoint.HasValue)
            {
                // Rectangle / Ellipse
                var selectionRect = new Rect(_startPoint.Value, _endPoint.Value);
                x = (int)selectionRect.X;
                y = (int)selectionRect.Y;
                width = (int)selectionRect.Width;
                height = (int)selectionRect.Height;
            }
            else if (Tool == ImageZoomTools.Dot && _startPoint.HasValue)
            {
                // Dot
                x = (int)_startPoint.Value.X;
                y = (int)_startPoint.Value.Y;
                width = 1;
                height = 1;
            }

            // 3. Create the Frame
            // We use 'points' (which now includes the closing point)
            var frame = new SelectionFrame
            {
                Tool = Tool,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Points = points
            };

            // 4. Cleanup
            _committedFrames.Clear();
            _committedFreeForms.Clear();
            FreeFormPoints.Clear();
            _startPoint = null;
            _endPoint = null;

            InvalidateVisual();

            return frame;
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
            // Define Pens
            var activePen = new Pen(Brushes.Red, 2) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
            var committedPen = new Pen(Brushes.Blue, 1); // Solid line for finished shapes
            var committedBrush = new SolidColorBrush(Color.FromArgb(30, 0, 0, 255)); // Faint fill

            // 1. Draw Committed (Past) Shapes
            foreach (var frame in _committedFrames)
            {
                // We must apply the current transform to the saved raw coordinates to ensure they zoom/pan correctly
                Point p1 = _imageTransform.Transform(new Point(frame.X, frame.Y));
                Point p2 = _imageTransform.Transform(new Point(frame.X + frame.Width, frame.Y + frame.Height));
                var rect = new Rect(p1, p2);

                if (frame.Tool == ImageZoomTools.Ellipse)
                    drawingContext.DrawEllipse(committedBrush, committedPen,
                        new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), rect.Width / 2, rect.Height / 2);
                else
                    drawingContext.DrawRectangle(committedBrush, committedPen, rect);
            }

            // 1b. Draw Committed FreeForms
            foreach (var points in _committedFreeForms)
            {
                if (points.Count <= 1) continue;
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    // Transform points back to visual space
                    var p0 = _imageTransform.Transform(points[0]);
                    ctx.BeginFigure(p0, false, false);
                    var transformedPoints = points.Select(p => _imageTransform.Transform(p)).ToList();
                    ctx.PolyLineTo(transformedPoints.Skip(1).ToArray(), true, false);
                }

                drawingContext.DrawGeometry(null, committedPen, geometry);
            }

            // 2. Draw Current (Active) Shape (Existing Logic)
            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                Rect selectionRect = new(_startPoint.Value, _endPoint.Value);

                // ... [Update CurrentSelectionFrame logic here from original code] ...

                switch (Tool)
                {
                    case ImageZoomTools.Rectangle:
                        drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)), activePen,
                            selectionRect);
                        break;
                    case ImageZoomTools.Ellipse:
                        var center = new Point(selectionRect.Left + selectionRect.Width / 2,
                            selectionRect.Top + selectionRect.Height / 2);
                        drawingContext.DrawEllipse(null, activePen, center, selectionRect.Width / 2,
                            selectionRect.Height / 2);
                        break;
                    case ImageZoomTools.Dot:
                        drawingContext.DrawRectangle(Brushes.Red, activePen,
                            new Rect(_startPoint.Value, new Size(1, 1)));
                        break;
                }
            }

            // 3. Draw Current FreeForm
            if (Tool == ImageZoomTools.FreeForm && FreeFormPoints.Count > 1)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    // Current FreeFormPoints are already in View Coordinates (handled in OnMouseMove or AddFreeFormPoint logic)
                    // Note: Check if your AddFreeFormPoint stores Transformed or View points. 
                    // Based on your original code: AddFreeFormPoint stores transformed (Logic Coordinates).
                    // So we must Inverse Transform them for display.

                    var p0 = _imageTransform.Transform(FreeFormPoints[0]);
                    ctx.BeginFigure(p0, false, false);
                    var transformedPoints = FreeFormPoints.Select(p => _imageTransform.Transform(p)).ToList();
                    ctx.PolyLineTo(transformedPoints.Skip(1).ToArray(), true, false);
                }

                drawingContext.DrawGeometry(null, activePen, geometry);
            }
        }
    }
}