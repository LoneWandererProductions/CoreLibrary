/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        SelectionAdorner.cs
 * PURPOSE:     Extensions for ImageZoom, handle all the selection Tools.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    /// Adorner for ImageZoom tool selection overlay.
    /// Handles rendering and logic for bounding boxes, free-form paths, polygons, and point selections.
    /// </summary>
    internal sealed class SelectionAdorner : Adorner
    {
        /// <summary>
        /// The starting point of the mouse selection in image coordinates.
        /// </summary>
        private Point? _startPoint;

        /// <summary>
        /// The ending/current point of the mouse selection in image coordinates.
        /// </summary>
        private Point? _endPoint;

        /// <summary>
        /// The active image transformation matrix used for converting between visual UI element space and image space.
        /// </summary>
        private Transform _imageTransform = Transform.Identity;

        /// <summary>
        /// The committed frames
        /// </summary>
        private readonly List<SelectionFrame> _committedFrames = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The element to bind this adorner to.</param>
        /// <param name="tool">The active selection tool mode.</param>
        /// <param name="transform">Optional image transform matrix applied to the adorned element.</param>
        public SelectionAdorner(UIElement adornedElement, ImageZoomTools tool, Transform? transform = null)
            : base(adornedElement)
        {
            Tool = tool;
            _imageTransform = transform ?? Transform.Identity;
            IsTracing = tool == ImageZoomTools.Trace;
        }

        /// <summary>
        /// Gets or sets a value indicating whether active point tracing is enabled.
        /// </summary>
        public bool IsTracing { get; set; }

        /// <summary>
        /// Gets or sets the collection of points defining a free-form, trace, or polygon selection.
        /// </summary>
        public List<Point> FreeFormPoints { get; set; } = new();

        /// <summary>
        /// Gets the active selection tool mode.
        /// </summary>
        public ImageZoomTools Tool { get; internal set; }

        /// <summary>
        /// Gets the current selection frame data containing bounding parameters and shape coordinates.
        /// </summary>
        public SelectionFrame CurrentSelectionFrame { get; private set; } = new();

        /// <summary>
        /// Updates the bounding selection box with new start and end mouse points.
        /// </summary>
        /// <param name="start">The raw mouse starting position in visual coordinates.</param>
        /// <param name="end">The raw mouse ending position in visual coordinates.</param>
        public void UpdateSelection(Point start, Point end)
        {
            _startPoint = TransformMousePosition(start);
            _endPoint = TransformMousePosition(end);
            UpdateCurrentSelectionFrame();
            InvalidateVisual();
        }

        /// <summary>
        /// Adds a point to the free-form/polygon point collection and updates the visual state.
        /// </summary>
        /// <param name="point">The raw mouse position to add in visual coordinates.</param>
        public void AddFreeFormPoint(Point point)
        {
            FreeFormPoints.Add(TransformMousePosition(point));
            UpdateCurrentSelectionFrame();
            InvalidateVisual();
        }

        /// <summary>
        /// Clears all free-form/polygon points and updates the visual state.
        /// </summary>
        public void ClearFreeFormPoints()
        {
            FreeFormPoints.Clear();
            UpdateCurrentSelectionFrame();
            InvalidateVisual();
        }

        /// <summary>
        /// Updates the image transform matrix applied to mouse coordinates and triggers a re-render.
        /// </summary>
        /// <param name="transform">The new transformation matrix to apply.</param>
        public void UpdateImageTransform(Transform? transform)
        {
            _imageTransform = transform ?? Transform.Identity;
            InvalidateVisual();
        }

        /// <summary>
        /// Captures the current selection frame and resets active drawing state.
        /// </summary>
        /// <returns>The captured <see cref="SelectionFrame"/> representation.</returns>
        public SelectionFrame CaptureAndClear()
        {
            var frame = CurrentSelectionFrame;

            // Reset active drawing state
            FreeFormPoints.Clear();
            _startPoint = null;
            _endPoint = null;
            IsTracing = false;
            CurrentSelectionFrame = new SelectionFrame();

            InvalidateVisual();
            return frame;
        }

        /// <summary>
        /// Returns all committed selection frames (including any uncommitted active shape) and resets internal state.
        /// </summary>
        public List<SelectionFrame> GetCommittedFrames()
        {
            if (FreeFormPoints.Count > 0 || (_startPoint.HasValue && _endPoint.HasValue))
            {
                CommitCurrentFrame();
            }

            var frames = new List<SelectionFrame>(_committedFrames);
            _committedFrames.Clear();
            return frames;
        }

        /// <summary>
        /// Commits the active selection frame into the committed frames buffer and resets active drawing state.
        /// </summary>
        public void CommitCurrentFrame()
        {
            if ((CurrentSelectionFrame.Width > 0 && CurrentSelectionFrame.Height > 0) ||
                CurrentSelectionFrame.Points is { Count: > 0 })
            {
                _committedFrames.Add(CurrentSelectionFrame);
            }

            FreeFormPoints.Clear();
            _startPoint = null;
            _endPoint = null;
            CurrentSelectionFrame = new SelectionFrame();
            InvalidateVisual();
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the active selection shape based on current coordinates and active tool mode.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a rendering pass.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var activePen = new Pen(Brushes.Red, 2) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
            var fillBrush = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));

            // Render committed polygon / selection frames
            foreach (var frame in _committedFrames)
            {
                if (frame.Points is { Count: > 1 })
                {
                    var committedGeo = new StreamGeometry();
                    using (var ctx = committedGeo.Open())
                    {
                        var p0 = _imageTransform.Transform(frame.Points[0]);
                        ctx.BeginFigure(p0, isClosed: frame.Tool == ImageZoomTools.Polygon,
                            isFilled: frame.Tool == ImageZoomTools.Polygon);
                        var pts = frame.Points.Skip(1).Select(p => _imageTransform.Transform(p)).ToArray();
                        ctx.PolyLineTo(pts, true, false);
                    }

                    drawingContext.DrawGeometry(frame.Tool == ImageZoomTools.Polygon ? fillBrush : null, activePen,
                        committedGeo);
                }
                else if (frame.Width > 0 || frame.Height > 0)
                {
                    var vStart = _imageTransform.Transform(new Point(frame.X, frame.Y));
                    var vEnd = _imageTransform.Transform(new Point(frame.X + frame.Width, frame.Y + frame.Height));
                    drawingContext.DrawRectangle(fillBrush, activePen, new Rect(vStart, vEnd));
                }
            }

            // Render active shape being drawn
            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var vStart = _imageTransform.Transform(_startPoint.Value);
                var vEnd = _imageTransform.Transform(_endPoint.Value);
                var selectionRect = new Rect(vStart, vEnd);

                switch (Tool)
                {
                    case ImageZoomTools.Rectangle:
                        drawingContext.DrawRectangle(fillBrush, activePen, selectionRect);
                        break;

                    case ImageZoomTools.Ellipse:
                        var center = new Point(selectionRect.Left + selectionRect.Width / 2,
                            selectionRect.Top + selectionRect.Height / 2);
                        drawingContext.DrawEllipse(fillBrush, activePen, center, selectionRect.Width / 2,
                            selectionRect.Height / 2);
                        break;

                    case ImageZoomTools.Dot:
                        drawingContext.DrawRectangle(Brushes.Red, activePen, new Rect(vStart, new Size(2, 2)));
                        break;
                }
            }

            if (Tool is ImageZoomTools.FreeForm or ImageZoomTools.Trace or ImageZoomTools.Polygon &&
                FreeFormPoints.Count > 0)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    var p0 = _imageTransform.Transform(FreeFormPoints[0]);
                    var isClosed = Tool == ImageZoomTools.Polygon;
                    ctx.BeginFigure(p0, isClosed, isClosed);

                    if (FreeFormPoints.Count > 1)
                    {
                        var transformedPoints =
                            FreeFormPoints.Skip(1).Select(p => _imageTransform.Transform(p)).ToArray();
                        ctx.PolyLineTo(transformedPoints, true, false);
                    }
                }

                drawingContext.DrawGeometry(Tool == ImageZoomTools.Polygon ? fillBrush : null, activePen, geometry);
            }
        }

        /// <summary>
        /// Transforms raw visual mouse coordinates back into unscaled image coordinates using the inverse transform.
        /// </summary>
        /// <param name="mousePosition">The raw mouse position relative to the visual container.</param>
        /// <returns>The transformed mouse position in image space.</returns>
        private Point TransformMousePosition(Point mousePosition)
        {
            if (_imageTransform.Inverse == null) return mousePosition;

            var transformed = _imageTransform.Inverse.Transform(mousePosition);
            return new Point(Math.Round(transformed.X), Math.Round(transformed.Y));
        }

        /// <summary>
        /// Recalculates bounding coordinates and builds the current <see cref="SelectionFrame"/> state.
        /// </summary>
        private void UpdateCurrentSelectionFrame()
        {
            int x = 0, y = 0, width = 0, height = 0;
            var points = new List<Point>();

            if (Tool is ImageZoomTools.FreeForm or ImageZoomTools.Trace or ImageZoomTools.Polygon)
            {
                if (FreeFormPoints.Count > 0)
                {
                    points = new List<Point>(FreeFormPoints);

                    // Ensure closed loops for multi-point polygon shapes
                    if (points.Count > 2 && Tool is ImageZoomTools.Polygon or ImageZoomTools.FreeForm)
                    {
                        if (points[0] != points[^1])
                        {
                            points.Add(points[0]);
                        }
                    }

                    var minX = points.Min(p => p.X);
                    var minY = points.Min(p => p.Y);
                    var maxX = points.Max(p => p.X);
                    var maxY = points.Max(p => p.Y);

                    x = (int)minX;
                    y = (int)minY;
                    width = (int)(maxX - minX);
                    height = (int)(maxY - minY);
                }
            }
            else if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var selectionRect = new Rect(_startPoint.Value, _endPoint.Value);
                x = (int)selectionRect.X;
                y = (int)selectionRect.Y;
                width = (int)selectionRect.Width;
                height = (int)selectionRect.Height;
            }
            else if (Tool == ImageZoomTools.Dot && _startPoint.HasValue)
            {
                x = (int)_startPoint.Value.X;
                y = (int)_startPoint.Value.Y;
                width = 1;
                height = 1;
            }

            CurrentSelectionFrame = new SelectionFrame
            {
                Tool = Tool,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Points = points
            };
        }
    }
}