/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Compare
 * FILE:        Coordinate.cs
 * PURPOSE:     A more clever way to handle some 2D coordinate Stuff
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable BadBracesSpaces
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable UnusedMember.Global

using System.Diagnostics;

namespace Imaging.Compare
{
    /// <inheritdoc />
    /// <summary>
    ///     Coordinate 2d Helper Class
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Coordinate" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Coordinate" /> class.
        /// </summary>
        /// <param name="x">The x in double.</param>
        /// <param name="y">The yin double.</param>
        public Coordinate(double x, double y)
        {
            X = (int)Math.Round(x, 1, MidpointRounding.AwayFromZero);
            Y = (int)Math.Round(y, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Coordinate" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        public Coordinate(int x, int y, int width)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Coordinate" /> class.
        /// </summary>
        public Coordinate()
        {
        }

        /// <summary>
        ///     Gets the null point.
        /// </summary>
        /// <value>
        ///     The null point.
        /// </value>
        public static Coordinate NullPoint { get; } = new(0, 0);

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public int Y { get; }

        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public int X { get; }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Equal or not</returns>
        public bool Equals(Coordinate other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is Coordinate other && Equals(other);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(Coordinate first, Coordinate second)
        {
            return first.X == second.X && first.Y == second.Y;
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(Coordinate first, Coordinate second)
        {
            return first.X != second.X || first.Y != second.Y;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
