/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/TertiaryVector.cs
 * PURPOSE:     A really basic obj that holds three double values, needed for obj Files
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace DataFormatter
{
    /// <summary>
    ///     Three Numbers, here it will describe a 3dimensional Vector
    /// </summary>
    public sealed class TertiaryVector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TertiaryVector" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public TertiaryVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TertiaryVector" /> class.
        /// </summary>
        public TertiaryVector()
        {
        }

        /// <summary>
        ///     Gets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public double X { get; init; }

        /// <summary>
        ///     Gets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public double Y { get; init; }

        /// <summary>
        ///     Gets the z.
        /// </summary>
        /// <value>
        ///     The z.
        /// </value>
        public double Z { get; init; }
    }
}
