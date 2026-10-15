/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        StrokeBatch.cs
 * PURPOSE:     The points of one flush of a pencil/eraser drag, plus where in the stroke it sits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    ///     One batch of a pencil/eraser drag. It still <em>is</em> an <see cref="IReadOnlyList{T}" /> of
    ///     points, so existing <c>SelectedPointCommand</c> handlers keep working unchanged; handlers that
    ///     care can test for this type to learn where in the stroke the batch sits (one undo step per
    ///     stroke, connecting the last point of the previous batch to the first of this one).
    /// </summary>
    public sealed class StrokeBatch : IReadOnlyList<Point>
    {
        private readonly Point[] _points;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StrokeBatch" /> class.
        /// </summary>
        /// <param name="points">The points of this batch.</param>
        /// <param name="isFirst">True for the first batch of a stroke (mouse-down).</param>
        /// <param name="isLast">True for the final batch of a stroke (mouse-up).</param>
        public StrokeBatch(Point[] points, bool isFirst, bool isLast)
        {
            _points = points;
            IsFirst = isFirst;
            IsLast = isLast;
        }

        /// <summary>Gets a value indicating whether this is the first batch of a stroke.</summary>
        public bool IsFirst { get; }

        /// <summary>Gets a value indicating whether this is the last batch of a stroke.</summary>
        public bool IsLast { get; }

        /// <inheritdoc />
        public int Count => _points.Length;

        /// <inheritdoc />
        public Point this[int index] => _points[index];

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator()
        {
            return ((IEnumerable<Point>)_points).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}