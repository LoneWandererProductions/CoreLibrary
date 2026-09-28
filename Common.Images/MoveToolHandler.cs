/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        MoveToolHandler.cs
 * PURPOSE:      Panning. Was the pre-switch <c>if (SelectionTool == ImageZoomTools.Move)</c> block in
 *              <c>Canvas_MouseDown</c> (mouse-down setup) plus the <c>ImageZoomTools.Move</c> cases of
 *              <c>Canvas_MouseMove</c>/<c>Canvas_MouseUp</c> (the latter was already just "release capture,
 *              do nothing else" - see <see cref="OnMouseUp" />). Draws no preview, so unlike the other two
 *              handlers this does not implement <see cref="IPreviewSource" />.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Common.Images.Interfaces;
using System;
using System.Windows;
using System.Windows.Media;

namespace Common.Images
{
    /// <summary>
    ///     Panning. Was the pre-switch <c>if (SelectionTool == ImageZoomTools.Move)</c> block in
    ///     <c>Canvas_MouseDown</c> (mouse-down setup) plus the <c>ImageZoomTools.Move</c> cases of
    ///     <c>Canvas_MouseMove</c>/<c>Canvas_MouseUp</c> (the latter was already just "release capture,
    ///     do nothing else" - see <see cref="OnMouseUp" />). Draws no preview, so unlike the other two
    ///     handlers this does not implement <see cref="IPreviewSource" />.
    /// </summary>
    internal sealed class MoveToolHandler : IToolHandler
    {
        /// <summary>
        /// The owner
        /// </summary>
        private readonly ImageZoom _owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveToolHandler"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public MoveToolHandler(ImageZoom owner) => _owner = owner;

        /// <inheritdoc />
        public void OnMouseDown(ToolContext context)
        {
            // Capture the drag origin in Canvas (scaled) space - see _panStartPoint's own remarks on
            // why this must be a separate point from _startPoint (image-local/unscaled space, used by
            // the drawing tools instead). Mixing the two here used to make panning feel erratic, worse
            // the further zoom was from 100%.
            _owner.PanStartPoint = context.CanvasPosition;

            // Capture the current image transform offset as the origin for panning.
            var matrix = _owner.BtmImage.RenderTransform.Value;
            _owner.OriginPoint = new Point(matrix.OffsetX, matrix.OffsetY);
        }

        /// <inheritdoc />
        public void OnMouseMove(ToolContext context)
        {
            var currentCanvasPos = context.CanvasPosition;
            var transform = (MatrixTransform)_owner.BtmImage.RenderTransform;
            var matrix = transform.Matrix;

            // 1. Calculate intended new offsets. Both sides of each subtraction must be in the same
            // coordinate space - currentCanvasPos is in Canvas (scaled) space, so the drag origin has
            // to be too (_panStartPoint), not _startPoint (image-local/unscaled space).
            var newX = _owner.OriginPoint.X + (currentCanvasPos.X - _owner.PanStartPoint.X);
            var newY = _owner.OriginPoint.Y + (currentCanvasPos.Y - _owner.PanStartPoint.Y);

            // 2. Boundary Checks
            var viewWidth = _owner.ScrollView.ActualWidth;
            var viewHeight = _owner.ScrollView.ActualHeight;
            var tWidth = _owner.BtmImage.ActualWidth * matrix.M11;
            var tHeight = _owner.BtmImage.ActualHeight * matrix.M22;

            // 3. Clamp panning only if image is larger than view
            if (tWidth > viewWidth)
                matrix.OffsetX = Math.Max(Math.Min(newX, 0), viewWidth - tWidth);
            if (tHeight > viewHeight)
                matrix.OffsetY = Math.Max(Math.Min(newY, 0), viewHeight - tHeight);

            _owner.BtmImage.RenderTransform = new MatrixTransform(matrix);
            _owner.SelectionAdorner?.UpdateImageTransform(_owner.BtmImage.RenderTransform);
        }

        /// <inheritdoc />
        public void OnMouseUp(ToolContext context)
        {
            // Just release capture (already done by Canvas_MouseUp before dispatch), do nothing else.
        }

        /// <inheritdoc />
        public void Reset()
        {
            // Nothing in progress to cancel - panning has no gesture state of its own.
        }
    }
}