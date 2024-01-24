/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Coordinate2D.cs
 * PURPOSE:     A more clever way to handle some 2D coordinate Stuff
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

using System;


namespace Mathematics
{
    /// <summary>
    /// Coordinate 2d Helper Class
    /// </summary>
    public sealed class Coordinate2D
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate2D"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Coordinate2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate2D"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        public Coordinate2D(int x, int y, int width)
        {
            X = x;
            Y = y;
            Id = CalculateId(x, y, width);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate2D"/> class.
        /// </summary>
        public Coordinate2D()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static Coordinate2D GetInstance(int id, int width)
        {
            return new Coordinate2D {X = IdToX(id, width), Y = IdToY(id, width)};
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Equal or not</returns>
        public bool Equals(Coordinate2D other)
        {
            return X.Equals(other?.X) && Y.Equals(other?.Y);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Coordinate2D other && Equals(other);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Coordinate2D first, Coordinate2D second)
        {
            return second is not null && first is not null && first.X == second.X && first.Y == second.Y;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Coordinate2D first, Coordinate2D second)
        {
            return second is not null && first is not null && first.X != second.X && first.Y != second.Y;
        }

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

        /// <summary>
        /// Gets the identifier of the Coordinate in the 2D System.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; set; }

        /// <summary>
        /// Calculates the identifier.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <returns>The id of the coordinate</returns>
        private static int CalculateId(int x, int y, int width)
        {
            return (y * width) + x;
        }

        /// <summary>
        /// Identifiers to x.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        private static int IdToX(int masterId, int width)
        {
            return masterId % width;
        }

        /// <summary>
        /// Identifiers to y.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        private static int IdToY(int masterId, int width)
        {
            return masterId / width;
        }
    }
}
