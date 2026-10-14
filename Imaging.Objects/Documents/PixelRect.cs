/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        PixelRect.cs
 * PURPOSE:     Small UI-free geometry primitives for the document model.
 *              Deliberately not System.Drawing / WPF types, so the model can be serialized, tested and reused.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    /// An integer pixel rectangle. Used for dirty regions and raster patches.
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.PixelRect&gt;" />
    public readonly record struct PixelRect(int X, int Y, int Width, int Height)
    {
        /// <summary>
        /// Gets the empty rectangle.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static PixelRect Empty => default;

        /// <summary>
        /// Gets the exclusive right edge.
        /// </summary>
        public int Right => X + Width;

        /// <summary>
        /// Gets the exclusive bottom edge.
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Gets a value indicating whether the rectangle has no area.
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        /// <summary>
        /// Creates a rectangle from its edges (right and bottom are exclusive).
        /// </summary>
        /// <param name="left">The left edge.</param>
        /// <param name="top">The top edge.</param>
        /// <param name="right">The right edge.</param>
        /// <param name="bottom">The bottom edge.</param>
        /// <returns>The created <see cref="PixelRect"/>.</returns>
        public static PixelRect FromLtrb(int left, int top, int right, int bottom)
        {
            return new PixelRect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        /// <summary>
        /// Gets the smallest rectangle containing both.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The smallest rectangle containing both.</returns>
        public PixelRect Union(PixelRect other)
        {
            if (IsEmpty) return other;
            if (other.IsEmpty) return this;

            return FromLtrb(Math.Min(X, other.X), Math.Min(Y, other.Y), Math.Max(Right, other.Right),
                Math.Max(Bottom, other.Bottom));
        }

        /// <summary>
        /// Gets the overlap of both, or <see cref="Empty" />.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The overlap of both, or <see cref="Empty" />.</returns>
        public PixelRect Intersect(PixelRect other)
        {
            if (IsEmpty || other.IsEmpty) return Empty;

            var result = FromLtrb(Math.Max(X, other.X), Math.Max(Y, other.Y), Math.Min(Right, other.Right),
                Math.Min(Bottom, other.Bottom));

            return result.IsEmpty ? Empty : result;
        }

        /// <summary>
        /// Grows the rectangle by <paramref name="amount" /> pixels on every side.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The inflated rectangle.</returns>
        public PixelRect Inflate(int amount)
        {
            return IsEmpty ? this : new PixelRect(X - amount, Y - amount, Width + (2 * amount), Height + (2 * amount));
        }
    }
}