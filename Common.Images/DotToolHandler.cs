/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        DotToolHandler.cs
 * PURPOSE:     Pencil/Eraser/Color-Picker - any tool mapped to <see cref="ImageZoomTools.Dot" />.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Common.Images.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Common.Images
{
    /// <summary>
    ///     Pencil/Eraser/Color-Picker - any tool mapped to <see cref="ImageZoomTools.Dot" />. Was the
    ///     <c>ImageZoomTools.Dot</c> cases of <c>Canvas_MouseDown</c>/<c>Canvas_MouseMove</c>/
    ///     <c>Canvas_MouseUp</c>. Owns the single preview point the adorner used to draw the little red
    ///     square from (<see cref="IPreviewSource.PreviewStart" />/<see cref="IPreviewSource.PreviewEnd" />
    ///     are always the same point here - Dot never has a separate drag end).
    /// </summary>
    internal sealed class DotToolHandler : IToolHandler, IPreviewSource
    {
        /// <summary>
        /// The stroke flush interval
        /// </summary>
        private static readonly TimeSpan StrokeFlushInterval = TimeSpan.FromMilliseconds(25);


        /// <summary>
        /// The owner
        /// </summary>
        private readonly ImageZoom _owner;

        /// <summary>
        /// The current click/drag point, already converted to image-pixel space, or null when idle.
        /// </summary>
        private Point? _previewPoint;

        public DotToolHandler(ImageZoom owner) => _owner = owner;

        /// <inheritdoc />
        public Point? PreviewStart => _previewPoint;

        /// <inheritdoc />
        public Point? PreviewEnd => _previewPoint;

        /// <inheritdoc />
        public IReadOnlyList<Point> FreeFormPoints => Array.Empty<Point>();

        /// <inheritdoc />
        public IReadOnlyList<SelectionFrame> CommittedFrames => Array.Empty<SelectionFrame>();

        /// <inheritdoc />
        public void OnMouseDown(ToolContext context)
        {
            _owner.StrokeBuffer.Clear();
            _owner.StrokeHasFlushed = false;

            // Was SelectionAdorner.UpdateSelection(_startPoint, _startPoint) - the adorner transformed
            // and stored both ends itself; now the handler converts the point (via the adorner's still-
            // view-owned ToImageSpace) and holds onto it, and the adorner just reads it back to draw.
            _previewPoint = _owner.SelectionAdorner?.ToImageSpace(context.ImagePosition);
            _owner.SelectionAdorner?.InvalidateVisual();
        }

        /// <inheritdoc />
        public void OnMouseMove(ToolContext context)
        {
            // Dragging the pencil/eraser is buffered and throttled (see FlushStroke) rather than
            // submitted on every move event, since each flush is a real image edit.
            _owner.StrokeBuffer.Add(context.ImagePosition);
            if (DateTime.UtcNow - _owner.LastStrokeFlush >= StrokeFlushInterval)
            {
                _owner.FlushStroke();
            }

            // Preview square deliberately not updated here, matching the old adorner-owned behavior:
            // UpdateSelection was only ever called from MouseDown for Dot, so the square stayed pinned
            // to the mouse-down point for the rest of the drag. Left exactly as it was - see this
            // handler's own remarks on why this move preserves behavior rather than also fixing it.
        }

        /// <inheritdoc />
        public void OnMouseUp(ToolContext context)
        {
            // Clear the preview square.
            _previewPoint = null;
            _owner.SelectionAdorner?.InvalidateVisual();

            // Use the actual release position, not the stale mouse-down position: a drag that ends
            // somewhere else used to always paint only the point where the drag started.
            _owner.StrokeBuffer.Add(context.ImagePosition);

            // Reliable: retries briefly rather than dropping the stroke's tail if a previous flush is
            // still in flight.
            _ = _owner.FlushStrokeOnReleaseAsync();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _previewPoint = null;
            _owner.StrokeBuffer.Clear();
            _owner.StrokeHasFlushed = false;
        }
    }
}