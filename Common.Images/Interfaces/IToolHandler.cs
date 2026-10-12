/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images.Interfaces
 * FILE:        IToolHandler.cs
 * PURPOSE:     First step of the "State/Strategy" refactor from ImageZoom.xaml.cs's own TODO roadmap
 *              (sections 1 and 2): a per-tool mouse handler, so Canvas_MouseDown/Move/Up dispatch to one
 *              of these instead of each containing its own switch(SelectionTool) statement. Concrete
 *              handlers (Common.Images.ImageZoom.MoveToolHandler/DotToolHandler/GestureToolHandler) are
 *              private nested classes of ImageZoom itself - see ImageZoom.ToolHandlers.cs - because they
 *              need direct access to session state ImageZoom already owns (the adorner, the stroke buffer,
 *              the pan origin, ...) that has no reason to become public surface just to support this.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Common.Images.Interfaces
{
    /// <summary>
    ///     One tool's mouse behavior. Each <see cref="ImageZoomTools" /> value maps to exactly one handler
    ///     (see <c>ImageZoom.ResolveHandler</c>); several values can share one handler instance when their
    ///     behavior is genuinely the same shape, driven by data rather than by which class it is - that's
    ///     what <c>GestureToolHandler</c> does for Rectangle/Ellipse/FreeForm/Polygon/Trace via
    ///     <see cref="GestureCatalog" />, instead of five near-identical classes.
    /// </summary>
    internal interface IToolHandler
    {
        /// <summary>
        /// Called once, on mouse-down, before any move events for this gesture.
        /// </summary>
        /// <param name="context">The context.</param>
        void OnMouseDown(ToolContext context);

        /// <summary>
        /// Called for every mouse-move while the button is held, after <see cref="OnMouseDown" />.
        /// </summary>
        /// <param name="context">The context.</param>
        void OnMouseMove(ToolContext context);

        /// <summary>
        /// Called once, on mouse-up, ending the gesture started by <see cref="OnMouseDown" />.
        /// </summary>
        /// <param name="context">The context.</param>
        void OnMouseUp(ToolContext context);

        /// <summary>
        ///     Cancels/clears any in-progress gesture state without committing anything. Called whenever the
        ///     active tool changes (<c>ImageZoom.OnSelectionToolChanged</c>) and before every new gesture
        ///     starts (<c>ImageZoom.AttachAdorner</c>) - replaces the old
        ///     <c>SelectionAdorner.ClearFreeFormPoints()</c>, which resides with the handler's own state now
        ///     instead of the adorner's.
        /// </summary>
        void Reset();
    }
}