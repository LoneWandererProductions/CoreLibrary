/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        GestureCatalog.cs
 * PURPOSE:     Single registry describing how each selection gesture behaves,
 *              replacing several independent switch statements that previously
 *              had to be kept in sync by hand across ImageZoom and SlimControls.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Imaging.Enums;

namespace Common.Images
{
    /// <summary>
    /// What shape a gesture draws on screen while the mouse is down.
    /// </summary>
    public enum SelectionShape
    {
        /// <summary>
        /// This tool doesn't use the generic drag-shape handling in
        /// <see cref="ImageZoom" />'s MouseMove at all - it has its own bespoke
        /// behavior (e.g. panning, or a tool driven entirely by its own
        /// mouse-down/adorner logic).
        /// </summary>
        None,

        /// <summary>
        /// A rectangular drag box between the mouse-down point and the current
        /// mouse position (used by Rectangle and Ellipse).
        /// </summary>
        Box,

        /// <summary>
        /// A freehand outline built up one point at a time as the mouse moves
        /// (used by FreeForm and Polygon).
        /// </summary>
        Freeform
    }

    /// <summary>
    /// Everything <see cref="ImageZoom" /> and the image-editing pipeline need to
    /// know about one selection gesture, in one place.
    /// </summary>
    /// <param name="Tool">The gesture this describes.</param>
    /// <param name="MouseMoveShape">
    /// How <see cref="ImageZoom" />'s MouseMove handler should update the adorner
    /// while dragging. <see cref="SelectionShape.None" /> for tools with their own
    /// bespoke handling (e.g. <see cref="ImageZoomTools.Trace" />, which drives
    /// itself via <c>IsTracing</c> rather than the generic box/freeform path).
    /// </param>
    /// <param name="CapturesFrameOnMouseUp">
    /// Whether releasing the mouse should capture the adorner's
    /// <see cref="SelectionFrame" /> and fire <c>SelectedFrameCommand</c>. This
    /// replaces the old hand-written <c>isDrawingTool</c> list in
    /// <see cref="ImageZoom" />'s MouseUp handler, which a new gesture (Polygon)
    /// was once left out of.
    /// </param>
    /// <param name="MaskShape">
    /// The pixel mask this gesture's committed frame should be treated as when
    /// filling/texturing/filtering/erasing. This replaces
    /// <c>SlimControls.Translator.MapCodeToTool</c>'s separate switch, which could
    /// (and once did) disagree with what actually got drawn on screen.
    /// </param>
    public sealed record GestureBehavior(
        ImageZoomTools Tool,
        SelectionShape MouseMoveShape,
        bool CapturesFrameOnMouseUp,
        MaskShape MaskShape);

    /// <summary>
    /// Registry of known selection gestures. Adding a new box- or freeform-style
    /// selection tool is exactly one entry here: it is then automatically drawn
    /// correctly while dragging, captured and committed on release, and given the
    /// right pixel mask when the edit is actually applied - there is no second
    /// switch statement anywhere else that also needs to know about it.
    /// </summary>
    /// <remarks>
    /// <see cref="ImageZoomTools.Move" /> (panning) and <see cref="ImageZoomTools.Dot" />
    /// (single-click point tools: pencil, eraser, color picker) are deliberately not
    /// registered here. They aren't shape selections at all - they're handled by
    /// their own dedicated code in <see cref="ImageZoom" /> - so forcing them into
    /// this table would just mean fields that don't apply to them.
    /// </remarks>
    public static class GestureCatalog
    {
        private static readonly Dictionary<ImageZoomTools, GestureBehavior> Registry = new()
        {
            [ImageZoomTools.Rectangle] = new GestureBehavior(
                ImageZoomTools.Rectangle, SelectionShape.Box, CapturesFrameOnMouseUp: true, MaskShape.Rectangle),
            [ImageZoomTools.Ellipse] = new GestureBehavior(
                ImageZoomTools.Ellipse, SelectionShape.Box, CapturesFrameOnMouseUp: true, MaskShape.Circle),
            [ImageZoomTools.FreeForm] = new GestureBehavior(
                ImageZoomTools.FreeForm, SelectionShape.Freeform, CapturesFrameOnMouseUp: true, MaskShape.Polygon),
            [ImageZoomTools.Polygon] = new GestureBehavior(
                ImageZoomTools.Polygon, SelectionShape.Freeform, CapturesFrameOnMouseUp: true, MaskShape.Polygon),

            // Trace keeps its existing bespoke MouseDown behavior (IsTracing = true)
            // untouched - SelectionShape.None means MouseMove does nothing extra for
            // it, exactly as before this catalog existed. It's registered here only
            // so MouseUp still captures/commits its frame and so it has an explicit,
            // discoverable mask instead of silently falling back to one.
            [ImageZoomTools.Trace] = new GestureBehavior(
                ImageZoomTools.Trace, SelectionShape.None, CapturesFrameOnMouseUp: true, MaskShape.Rectangle)
        };

        /// <summary>
        /// Looks up the behavior for a gesture.
        /// </summary>
        /// <param name="tool">The gesture to look up.</param>
        /// <param name="behavior">The registered behavior, if any.</param>
        /// <returns><c>true</c> if <paramref name="tool" /> is a registered shape gesture.</returns>
        public static bool TryGet(ImageZoomTools tool, out GestureBehavior behavior) =>
            Registry.TryGetValue(tool, out behavior!);

        /// <summary>
        /// The pixel mask a gesture's frame should be treated as, or
        /// <see cref="MaskShape.Rectangle" /> for anything not registered here
        /// (matching the previous default-case behavior of
        /// <c>Translator.MapCodeToTool</c> for tools - Move, Dot - that never
        /// produce a fillable frame in the first place).
        /// </summary>
        public static MaskShape MaskShapeFor(ImageZoomTools tool) =>
            TryGet(tool, out var behavior) ? behavior.MaskShape : MaskShape.Rectangle;
    }
}
