/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageColor.cs
 * PURPOSE:     Struct to Search for similar Colors
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using ExtendedSystemObjects;

namespace ImageCompare
{
    /// <inheritdoc cref="IComparable" />
    /// <summary>
    ///     Helper Struct for the Color and Image Analysis
    /// </summary>
    /// <seealso cref="T:System.IComparable`1" />
    internal readonly struct ImageColor : IComparable<ImageColor>, IEquatable<ImageColor>
    {
        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is ImageColor other && Equals(other);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(R, B, G);
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(R, ImageResources.Separator, G, ImageResources.Separator, G);
        }

        /// <summary>
        ///     Gets the average Red Values.
        /// </summary>
        /// <value>
        ///     The red.
        /// </value>
        internal int R { get; init; }

        /// <summary>
        ///     Gets the average blue Values.
        /// </summary>
        /// <value>
        ///     The blue.
        /// </value>
        internal int B { get; init; }

        /// <summary>
        ///     Gets the average Green Values.
        /// </summary>
        /// <value>
        ///     The green.
        /// </value>
        internal int G { get; init; }

        /// <summary>
        ///     Gets or sets the threshold.
        /// </summary>
        /// <value>
        ///     The threshold.
        /// </value>
        internal int Threshold { get; init; }

        /// <summary>
        ///     Gets or sets the path.
        /// </summary>
        /// <value>
        ///     The path.
        /// </value>
        internal string Path { get; init; }

        /// <inheritdoc cref="IComparable" />
        /// <summary>
        ///     Compares the specified arrays.
        /// </summary>
        /// <param name="other">Object we compare to</param>
        /// <returns>
        ///     Compare result to <paramref name="other" />.
        /// </returns>
        public int CompareTo(ImageColor other)
        {
            if (!other.R.Interval(R, Threshold))
            {
                return 0;
            }

            if (!other.G.Interval(G, Threshold))
            {
                return 0;
            }

            if (!other.B.Interval(B, Threshold))
            {
                return 0;
            }

            return 1;
        }

        /// <inheritdoc cref="IComparable" />
        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(ImageColor other)
        {
            if (!other.R.Interval(R, Threshold))
            {
                return false;
            }

            if (!other.G.Interval(G, Threshold))
            {
                return false;
            }

            if (!other.B.Interval(B, Threshold))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(ImageColor left, ImageColor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(ImageColor left, ImageColor right)
        {
            return !(left == right);
        }
    }
}
