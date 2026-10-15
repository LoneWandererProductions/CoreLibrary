/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        GestureToolHandler.cs
 * PURPOSE:     Concrete IToolHandler implementations for ImageZoom. DotToolHandler and GestureToolHandler
 *              now own the in-progress point/frame data that used to live on SelectionAdorner (TODO roadmap
 *              section 4: "restrict adorner to visualization-only duties" / "let adorner read data from
 *              current IToolHandler instead of owning the point lists") and implement IPreviewSource so the
 *              adorner can read it back at render time. Every coordinate-space conversion still goes through
 *              SelectionAdorner.ToImageSpace, in exactly the same sequence the old adorner-owned methods
 *              used to call it internally - that math is delicate (see MoveToolHandler's own remarks on a
 *              similar past bug) and is deliberately left untouched by this move.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Common.Images.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Common.Images
{
    /// <summary>
    ///     Rectangle/Ellipse/FreeForm/Polygon/Trace - every tool registered in
    ///     <see cref="GestureCatalog" />. One handler serves all of them, looking up each one's shape/
    ///     capture behavior fresh from the catalog on every call (a cheap dictionary lookup) rather than
    ///     being five near-identical classes - that duplication was exactly the problem the catalog
    ///     already solved for <c>Canvas_MouseMove</c>/<c>Canvas_MouseUp</c>; a class-per-gesture split
    ///     here would have quietly brought it back. Owns the box/freeform point data and the multi-
    ///     select committed-frame buffer that used to live on <see cref="SelectionAdorner" />.
    /// </summary>
    internal sealed class GestureToolHandler : IToolHandler, IPreviewSource
    {
        /// <summary>
        /// The owner
        /// </summary>
        private readonly ImageZoom _owner;

        /// <summary>
        /// The free form points
        /// </summary>
        private readonly List<Point> _freeFormPoints = new();

        /// <summary>
        /// The committed frames
        /// </summary>
        private readonly List<SelectionFrame> _committedFrames = new();

        /// <summary>
        /// The start
        /// </summary>
        private Point? _start;

        /// <summary>
        /// The end
        /// </summary>
        private Point? _end;

        /// <summary>
        /// The current frame
        /// </summary>
        private SelectionFrame _currentFrame = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureToolHandler"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public GestureToolHandler(ImageZoom owner) => _owner = owner;

        /// <inheritdoc />
        public Point? PreviewStart => _start;

        /// <inheritdoc />
        public Point? PreviewEnd => _end;

        /// <inheritdoc />
        public IReadOnlyList<Point> FreeFormPoints => _freeFormPoints;

        /// <inheritdoc />
        public IReadOnlyList<SelectionFrame> CommittedFrames => _committedFrames;

        /// <inheritdoc />
        public void OnMouseDown(ToolContext context)
        {
            // Every registered gesture (Rectangle, Ellipse, FreeForm, Polygon, Trace) needs no extra
            // mouse-down setup beyond the AttachAdorner call Canvas_MouseDown already made before
            // dispatching here. (Trace used to set adorner.IsTracing = true here, but nothing ever read
            // that flag - see GestureCatalog.Trace's remarks; Trace is rendered/accumulated exactly like
            // FreeForm via the points list below, which is a separate, pre-existing gap: nothing
            // currently feeds Trace's OnMouseMove into that list either, since its catalog entry uses
            // SelectionShape.None. Left as-is here since fixing it is a behavior change, not a data-
            // ownership move.)
        }

        /// <inheritdoc />
        public void OnMouseMove(ToolContext context)
        {
            if (_owner.SelectionAdorner is not { } adorner) return;
            if (!GestureCatalog.TryGet(_owner.SelectionTool, out var behavior)) return;

            switch (behavior.MouseMoveShape)
            {
                case SelectionShape.Box:
                    // Re-derived from the raw mouse-down point every call, exactly as the old adorner-
                    // owned UpdateSelection did (it re-transformed _startPoint on every move too) -
                    // deliberately not cached, so a zoom change mid-drag is picked up the same way it
                    // always was.
                    _start = adorner.ToImageSpace(_owner.StartPoint);
                    _end = adorner.ToImageSpace(context.ImagePosition);
                    RecomputeCurrentFrame();
                    adorner.InvalidateVisual();
                    break;
                case SelectionShape.Freeform:
                    _freeFormPoints.Add(adorner.ToImageSpace(context.ImagePosition));
                    RecomputeCurrentFrame();
                    adorner.InvalidateVisual();
                    break;
                case SelectionShape.None:
                    // Bespoke tool (Trace) - drives itself, nothing to do here.
                    break;
            }
        }

        /// <inheritdoc />
        public void OnMouseUp(ToolContext context)
        {
            if (_owner.SelectionAdorner is null) return;

            // Driven by the catalog rather than a hand-written list here: this is exactly the list
            // Polygon was once missing from, which silently meant its selections were drawn on screen
            // but never committed to an edit.
            var isDrawingTool = GestureCatalog.TryGet(_owner.SelectionTool, out var behavior) &&
                                behavior.CapturesFrameOnMouseUp;

            if (!isDrawingTool) return;

            // Capture the data AND clear the visuals immediately.
            var frame = CaptureAndClear();

            // Validation: ensure something substantial was actually drawn.
            var isValid = (frame.Width > 0 && frame.Height > 0) || frame.Points is { Count: > 0 };
            if (!isValid) return;

            // Fire the command to the ViewModel (update the bitmap/document), then the event, if
            // anything else is listening.
            ImageZoom.SafeExecuteCommand(_owner.SelectedFrameCommand, frame);
            _owner.RaiseSelectedFrame(frame);
        }

        /// <inheritdoc />
        public void Reset()
        {
            // Matches the old SelectionAdorner.ClearFreeFormPoints' scope: clears the in-progress
            // gesture (box drag and/or freeform points), leaves any already-committed multi-select
            // frames alone.
            _freeFormPoints.Clear();
            _start = null;
            _end = null;
            _currentFrame = new SelectionFrame();
        }

        /// <summary>
        ///     Returns all committed selection frames (including any uncommitted active shape) and
        ///     resets internal state. Backs <c>ImageZoom.Canvas_MouseRightButtonUp</c>'s multi-select
        ///     path, which was already dormant before this move (its command execution is commented out
        ///     there) - kept working exactly as before, not revived or fixed.
        /// </summary>
        public List<SelectionFrame> GetCommittedFrames()
        {
            if (_freeFormPoints.Count > 0 || (_start is { } && _end is { }))
            {
                CommitCurrentFrame();
            }

            var frames = new List<SelectionFrame>(_committedFrames);
            _committedFrames.Clear();
            return frames;
        }

        /// <summary>
        ///     Commits the active selection frame into the committed-frames buffer and resets the
        ///     in-progress gesture state. Only reachable via <see cref="GetCommittedFrames" />.
        /// </summary>
        private void CommitCurrentFrame()
        {
            if ((_currentFrame.Width > 0 && _currentFrame.Height > 0) ||
                _currentFrame.Points is { Count: > 0 })
            {
                _committedFrames.Add(_currentFrame);
            }

            _freeFormPoints.Clear();
            _start = null;
            _end = null;
            _currentFrame = new SelectionFrame();
            _owner.SelectionAdorner?.InvalidateVisual();
        }

        /// <summary>
        ///     Captures <see cref="_currentFrame" /> and resets the in-progress gesture state (but not
        ///     <see cref="_committedFrames" /> - see <see cref="CommitCurrentFrame" /> for that path).
        /// </summary>
        private SelectionFrame CaptureAndClear()
        {
            var frame = _currentFrame;

            _freeFormPoints.Clear();
            _start = null;
            _end = null;
            _currentFrame = new SelectionFrame();

            _owner.SelectionAdorner?.InvalidateVisual();
            return frame;
        }

        /// <summary>
        ///     Recalculates bounding coordinates and rebuilds <see cref="_currentFrame" /> from
        ///     <see cref="_freeFormPoints" />/<see cref="_start" />/<see cref="_end" />, exactly as the
        ///     old <c>SelectionAdorner.UpdateCurrentSelectionFrame</c> did. Its old third branch (for
        ///     <see cref="ImageZoomTools.Dot" />) is dropped here rather than moved: <see cref="GestureToolHandler" />
        ///     is never resolved for Dot (see <c>ImageZoom.ResolveHandler</c>), so that branch was
        ///     unreachable the moment ownership split this way.
        /// </summary>
        private void RecomputeCurrentFrame()
        {
            int x = 0, y = 0, width = 0, height = 0;
            var points = new List<Point>();

            if (_owner.SelectionTool is ImageZoomTools.FreeForm or ImageZoomTools.Trace or ImageZoomTools.Polygon)
            {
                if (_freeFormPoints.Count > 0)
                {
                    points = new List<Point>(_freeFormPoints);

                    // Ensure closed loops for multi-point polygon shapes
                    if (points.Count > 2 &&
                        _owner.SelectionTool is ImageZoomTools.Polygon or ImageZoomTools.FreeForm)
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
            else if (_start is { } && _end is { })
            {
                var selectionRect = new Rect(_start.Value, _end.Value);
                x = (int)selectionRect.X;
                y = (int)selectionRect.Y;
                width = (int)selectionRect.Width;
                height = (int)selectionRect.Height;
            }

            _currentFrame = new SelectionFrame
            {
                Tool = _owner.SelectionTool,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Points = points
            };
        }
    }
}