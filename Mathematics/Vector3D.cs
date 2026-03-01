/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Vector3D.cs
 * PURPOSE:     Basic 3D Vector implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

using System;
using DataFormatter;

namespace Mathematics
{
    /// <inheritdoc cref="IEquatable" />
    /// <summary>
    ///     Basic Vector Implementation
    /// </summary>
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector3D" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        // Passes to the master constructor below, ensuring W defaults to 1.0
        public Vector3D(double x, double y, double z) : this(x, y, z, 1.0d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3D"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector3D(double x, double y) : this(x, y, 0d, 1.0d)
        {
        }

        /// <summary>
        /// Master Constructor
        /// Initializes a new instance of the <see cref="Vector3D"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="w">The w.</param>
        public Vector3D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public double X { get; }

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public double Y { get; }

        /// <summary>
        ///     Gets or sets the z.
        /// </summary>
        /// <value>
        ///     The z.
        /// </value>
        public double Z { get; }

        /// <summary>
        ///     Gets or sets the w, for internal use only, used with the 3D to 2D calculation,
        ///     This has no relevance, whatsoever for the other calculations.
        /// </summary>
        /// <value>
        ///     The w  Value, which is 1 when initialized.
        /// </value>
        public double W { get; } = 1;

        /// <summary>
        ///     Gets the zero Vector.
        /// </summary>
        /// <value>
        ///     The zero Vector.
        /// </value>
        public static Vector3D ZeroVector { get; } = new(0d, 0d, 0d);

        /// <summary>
        ///     Gets the Unit vector.
        /// </summary>
        /// <value>
        ///     The Unit vector.
        /// </value>
        public static Vector3D UnitVector { get; } = new(1d, 1d, 1d);

        /// <summary>
        ///     Gets the rounded x.
        /// </summary>
        /// <value>
        ///     The rounded x.
        /// </value>
        public int RoundedX => (int)Math.Round(X, 0);

        /// <summary>
        ///     Gets the rounded y.
        /// </summary>
        /// <value>
        ///     The rounded y.
        /// </value>
        public int RoundedY => (int)Math.Round(Y, 0);

        /// <summary>
        ///     Gets the rounded z.
        /// </summary>
        /// <value>
        ///     The rounded z.
        /// </value>
        public int RoundedZ => (int)Math.Round(Z, 0);

        /// <inheritdoc />
        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Equal or not</returns>
        public bool Equals(Vector3D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        /// <summary>
        ///     Sets the w.
        /// </summary>
        /// <param name="w">The w.</param>
        internal Vector3D WithW(double w) => new(X, Y, Z, w);

        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Vector3D other && Equals(other);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(Vector3D first, Vector3D second)
        {
            return Math.Abs(first.X - second.X) < MathResources.Tolerance &&
                   Math.Abs(first.Y - second.Y) < MathResources.Tolerance &&
                   Math.Abs(first.Z - second.Z) < MathResources.Tolerance &&
                   Math.Abs(first.W - second.W) < MathResources.Tolerance;
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(Vector3D first, Vector3D second)
        {
            return (Math.Abs(first.X - second.X) > MathResources.Tolerance ||
                    Math.Abs(first.Y - second.Y) > MathResources.Tolerance ||
                    Math.Abs(first.Z - second.Z) > MathResources.Tolerance ||
                    Math.Abs(first.W - second.W) > MathResources.Tolerance);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        /// <summary>
        ///     Converts the Vector to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(MathResources.StrX, X, MathResources.StrY, Y, MathResources.StrZ, Z);
        }

        /// <summary>
        ///     Implements the operator +.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator +(Vector3D first, Vector3D second)
        {
            return new Vector3D(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        /// <summary>
        ///     Implements the operator -.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator -(Vector3D first)
        {
            return new Vector3D(-first.X, -first.Y, -first.Z);
        }

        /// <summary>
        ///     Implements the operator -.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator -(Vector3D first, Vector3D second)
        {
            return new Vector3D(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        }

        /// <summary>
        ///     Implements the operator *.
        ///     The Dot product.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator, the dot product
        /// </returns>
        public static double operator *(Vector3D first, Vector3D second)
        {
            return (first.X * second.X) + (first.Y * second.Y) + (first.Z * second.Z);
        }

        /// <summary>
        ///     Implements the operator *. Left Hand
        ///     Multiplication Factors can be switched.
        /// </summary>
        /// <param name="first">The v.</param>
        /// <param name="scalar">The scalar.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator *(Vector3D first, double scalar)
        {
            return new Vector3D(first.X * scalar, first.Y * scalar, first.Z * scalar);
        }

        /// <summary>
        ///     Implements the operator *. Right Hand.
        ///     Multiplication Factors can be switched.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <param name="first">The v.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator *(double scalar, Vector3D first)
        {
            return first * scalar;
        }

        /// <summary>
        ///     Implements the operator /.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="scalar">The scalar.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator /(Vector3D v, double scalar)
        {
            return new Vector3D(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        /// <summary>
        ///     Implements the operator *. Cross Product
        /// </summary>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The Cross Product of the Vectors.
        /// </returns>
        public Vector3D CrossProduct(Vector3D second)
        {
            return new Vector3D
            (
                (Y * second.Z) - (Z * second.Y),
                (Z * second.X) - (X * second.Z),
                (X * second.Y) - (Y * second.X)
            );
        }

        /// <summary>
        ///     Get the Vector length.
        ///     (or Magnitude)
        /// </summary>
        /// <returns>Length of the Vector</returns>
        public double VectorLength()
        {
            return Math.Sqrt(this * this);
        }

        /// <summary>
        ///     Normalizes this instance.
        ///     Unit Vector
        /// </summary>
        /// <returns>Normalized Vector</returns>
        public Vector3D Normalize()
        {
            var l = VectorLength();
            // Safety check to prevent NaN corruption
            return l > 0 ? new Vector3D(X / l, Y / l, Z / l, W) : ZeroVector;
        }

        /// <summary>
        ///     Angle between this and the other Vector
        /// </summary>
        /// <returns>Angle between both Vectors in rad</returns>
        public double Angle(Vector3D second)
        {
            return Math.Acos(this * second / (VectorLength() * second.VectorLength()));
        }

        /// <summary>
        ///     Converts to matrix.
        ///     In this case especially for 3D Projection
        /// </summary>
        /// <returns>Vector transformed to Matrix</returns>
        public static explicit operator BaseMatrix(Vector3D first)
        {
            var matrix = new[,] { { first.X, first.Y, first.Z, first.W } };

            return new BaseMatrix(matrix);
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="TertiaryVector" /> to <see cref="Vector3D" />.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static explicit operator Vector3D(TertiaryVector first)
        {
            return new Vector3D(first.X, first.Y, first.Z);
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="TertiaryFace" /> to <see cref="Vector3D" />.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static explicit operator Vector3D(TertiaryFace first)
        {
            return new Vector3D(first.X, first.Y, first.Z);
        }
    }
}
