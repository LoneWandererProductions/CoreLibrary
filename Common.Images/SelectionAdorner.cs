/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        SelectionAdorner.cs
 * PURPOSE:     Draws the selection overlay for ImageZoom's tools. Restricted to visualization-only duties
 *              (TODO roadmap section 4): it no longer owns the in-progress point lists/frames itself - it
 *              reads them each render pass from whichever IToolHandler is currently active, via Source (see
 *              IPreviewSource). What it still owns is exactly what genuinely is a view concern: the current
 *              image transform, and converting a raw mouse position into image-pixel space with it
 *              (ToImageSpace) - handlers call that themselves before storing a point, rather than the
 *              adorner doing the storing.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using Common.Images.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    /// Adorner for ImageZoom tool selection overlay.
    /// Handles rendering for bounding boxes, free-form paths, polygons, and point selections - the point/
    /// frame data itself lives on the active IToolHandler (see Source), not here.
    /// </summary>
    internal sealed class SelectionAdorner : Adorner
    {
        /// <summary>
        /// The active image transformation matrix used for converting between visual UI element space and image space.
        /// </summary>
        private Transform _imageTransform = Transform.Identity;

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
        }

        /// <summary>
        /// Gets the active selection tool mode.
        /// </summary>
        public ImageZoomTools Tool { get; internal set; }

        /// <summary>
        /// The handler to read preview data from when rendering - set by <c>ImageZoom</c> (in
        /// <c>AttachAdorner</c>/<c>OnSelectionToolChanged</c>) to whichever <c>IToolHandler</c> is now
        /// active, or left null for a tool (Move) that draws no preview at all.
        /// </summary>
        public IPreviewSource? Source { get; set; }

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
        /// Transforms a raw visual mouse coordinate into unscaled image-pixel space using the inverse of the
        /// current image transform. Public so an <see cref="IToolHandler"/> can convert a point itself
        /// before storing it (see e.g. <c>GestureToolHandler.OnMouseMove</c>) - this used to happen inside
        /// the adorner's own point-storing methods, which no longer exist here.
        /// </summary>
        /// <param name="mousePosition">The raw mouse position relative to the visual container.</param>
        /// <returns>The transformed mouse position in image space.</returns>
        public Point ToImageSpace(Point mousePosition)
        {
            if (_imageTransform.Inverse == null) return mousePosition;

            var transformed = _imageTransform.Inverse.Transform(mousePosition);
            return new Point(Math.Round(transformed.X), Math.Round(transformed.Y));
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the active selection shape based on <see cref="Source"/>'s current data and <see cref="Tool"/>.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a rendering pass.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var activePen = new Pen(Brushes.Red, 2) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
            var fillBrush = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));

            var committedFrames = Source?.CommittedFrames ?? Array.Empty<SelectionFrame>();
            var freeFormPoints = Source?.FreeFormPoints ?? Array.Empty<Point>();
            var previewStart = Source?.PreviewStart;
            var previewEnd = Source?.PreviewEnd;

            // Render committed polygon / selection frames
            foreach (var frame in committedFrames)
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
            if (previewStart is { } && previewEnd is { })
            {
                var vStart = _imageTransform.Transform(previewStart.Value);
                var vEnd = _imageTransform.Transform(previewEnd.Value);
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
                freeFormPoints.Count > 0)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    var p0 = _imageTransform.Transform(freeFormPoints[0]);
                    var isClosed = Tool == ImageZoomTools.Polygon;
                    ctx.BeginFigure(p0, isClosed, isClosed);

                    if (freeFormPoints.Count > 1)
                    {
                        var transformedPoints =
                            freeFormPoints.Skip(1).Select(p => _imageTransform.Transform(p)).ToArray();
                        ctx.PolyLineTo(transformedPoints, true, false);
                    }
                }

                drawingContext.DrawGeometry(Tool == ImageZoomTools.Polygon ? fillBrush : null, activePen, geometry);
            }
        }
    }
}