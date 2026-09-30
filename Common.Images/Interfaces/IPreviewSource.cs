/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images.Interfaces
 * FILE:        IPreviewSource.cs
 * PURPOSE:     Read-only preview data a tool handler exposes for <see cref="SelectionAdorner" /> to draw, so the adorner can be restricted to visualization-only duties.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Windows;

namespace Common.Images.Interfaces
{
    /// <summary>
    ///     Read-only preview data a tool handler exposes for <see cref="SelectionAdorner" /> to draw, so the
    ///     adorner can be restricted to visualization-only duties (TODO roadmap section 4) instead of also
    ///     owning the in-progress point lists itself. Implemented by handlers that have something to preview
    ///     (<c>DotToolHandler</c>, <c>GestureToolHandler</c>); <c>MoveToolHandler</c> does not implement it,
    ///     since panning draws nothing extra - <see cref="SelectionAdorner.Source" /> is simply left null
    ///     while it is the active tool.
    /// </summary>
    internal interface IPreviewSource
    {
        /// <summary>
        /// The drag/click start point, in image-pixel space, or <c>null</c> when nothing is in progress.
        /// For <c>DotToolHandler</c> this and <see cref="PreviewEnd" /> are always the same single point.
        /// </summary>
        /// <value>
        /// The preview start.
        /// </value>
        Point? PreviewStart { get; }

        /// <summary>
        /// The current drag end point (or the same as <see cref="PreviewStart" /> for a single click).
        /// </summary>
        /// <value>
        /// The preview end.
        /// </value>
        Point? PreviewEnd { get; }

        /// <summary>
        /// Points accumulated so far for a freehand/polygon gesture, in image-pixel space.
        /// </summary>
        /// <value>
        /// The free form points.
        /// </value>
        IReadOnlyList<Point> FreeFormPoints { get; }

        /// <summary>
        /// Frames already committed in the current multi-select gesture (see <c>GestureToolHandler.GetCommittedFrames</c>).
        /// Empty for handlers that don't support multi-select.
        /// </summary>
        /// <value>
        /// The committed frames.
        /// </value>
        IReadOnlyList<SelectionFrame> CommittedFrames { get; }
    }
}
