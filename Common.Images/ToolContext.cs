/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        ToolContext.cs
 * PURPOSE:     First step of the "State/Strategy" refactor from ImageZoom.xaml.cs's own TODO roadmap
 *              (sections 1 and 2): a per-tool mouse handler, so Canvas_MouseDown/Move/Up dispatch to one
 *              of these instead of each containing its own switch(SelectionTool) statement. Concrete
 *              handlers (Common.Images.ImageZoom.MoveToolHandler/DotToolHandler/GestureToolHandler) are
 *              private nested classes of ImageZoom itself - see ImageZoom.ToolHandlers.cs - because they
 *              need direct access to session state ImageZoom already owns (the adorner, the stroke buffer,
 *              the pan origin, ...) that has no reason to become public surface just to support this.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Common.Images.Interfaces;
using System.Windows;

namespace Common.Images
{
    /// <summary>
    ///     Per-mouse-event data a tool handler needs, computed once by <see cref="ImageZoom" /> before
    ///     dispatch rather than every handler re-deriving its own coordinates. Deliberately minimal for now
    ///     (just the two coordinate spaces every existing tool already needed) - the TODO's "carry image
    ///     transforms, image size, modifiers (Ctrl/Shift)" can be added as more fields here later without
    ///     changing <see cref="IToolHandler" />'s signature or any handler's body, which is the point of
    ///     having a context object instead of a growing parameter list.
    /// </summary>
    public readonly struct ToolContext
    {
        /// <summary>Initializes a new instance of the <see cref="ToolContext" /> struct.</summary>
        /// <param name="imagePosition">See <see cref="ImagePosition" />.</param>
        /// <param name="canvasPosition">See <see cref="CanvasPosition" />.</param>
        public ToolContext(Point imagePosition, Point canvasPosition)
        {
            ImagePosition = imagePosition;
            CanvasPosition = canvasPosition;
        }

        /// <summary>
        ///     Mouse position in image-local (unscaled) pixel space - what <see cref="SelectionFrame" /> and
        ///     every drawing tool (Rectangle, Ellipse, FreeForm, Dot) already worked in.
        /// </summary>
        public Point ImagePosition { get; }

        /// <summary>
        ///     Mouse position in the scroll canvas's (zoom-scaled) space. Only panning needs this - see
        ///     <c>ImageZoom._panStartPoint</c>'s remarks on why it must not be mixed with
        ///     <see cref="ImagePosition" /> in the same calculation.
        /// </summary>
        public Point CanvasPosition { get; }
    }
}
