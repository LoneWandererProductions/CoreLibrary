/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     ImageCompare
* FILE:        ImageCompare/ImageSimilar.cs
* PURPOSE:     Struct to Compare if Images are Similar
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using ExtendedSystemObjects;

namespace ImageCompare
{
    /// <inheritdoc />
    /// <summary>
    ///     Struct for checking duplicate Images
    /// </summary>
    /// <seealso cref="T:System.IComparable`1" />
    internal readonly struct ImageSimilar : IComparable<ImageSimilar>
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
            return obj is ImageSimilar other && Equals(other);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Image);
        }

        /// <summary>
        ///     Gets the image Color Values.
        /// </summary>
        /// <value>
        ///     The image.
        /// </value>
        internal byte[,] Image { get; init; }

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
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        internal int Id { get; init; }

        public byte[] Hash { get; init; }

        /// <summary>
        ///     Checks if Image is equal to another
        ///     Here we only check the Color values
        /// </summary>
        /// <param name="other">Compares it to another SimImage Object</param>
        /// <returns>
        ///     True if this Object is equal to <paramref name="other" /> else false"/>.
        /// </returns>
        public bool Equals(ImageSimilar other)
        {
            //if (Hash == null || other.Hash == null)
            //{
            //    return false;
            //}

            return other.R.Interval(R, ImageResources.ColorThreshold) &&
                   other.G.Interval(G, ImageResources.ColorThreshold) &&
                   other.B.Interval(B, ImageResources.ColorThreshold);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares the specified arrays.
        /// </summary>
        /// <param name="other">Object we compare to</param>
        /// <returns>
        ///     Compare result to <paramref name="other" />.
        /// </returns>
        public int CompareTo(ImageSimilar other)
        {
            if (Image == null) return 0;

            if (!other.R.Interval(R, ImageResources.ColorThreshold)) return 0;

            if (!other.G.Interval(G, ImageResources.ColorThreshold)) return 0;

            if (!other.B.Interval(B, ImageResources.ColorThreshold)) return 0;

            return 1;
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(ImageSimilar left, ImageSimilar right)
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
        public static bool operator !=(ImageSimilar left, ImageSimilar right)
        {
            return !(left == right);
        }
    }
}