/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Vector3D.cs
 * PURPOSE:     Basic 3D Vector implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassCanBeSealed.Global

using System;

namespace Mathematics
{
    /// <summary>
    ///     Basic Vector Implementation
    /// </summary>
    public class Vector3D
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector3D" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector3D" /> class.
        /// </summary>
        public Vector3D()
        {
        }

        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public double X { get; set; }

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public double Y { get; set; }

        /// <summary>
        ///     Gets or sets the z.
        /// </summary>
        /// <value>
        ///     The z.
        /// </value>
        public double Z { get; set; }

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

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Equal or not</returns>
        public bool Equals(Vector3D other)
        {
            return X.Equals(other?.X) && Y.Equals(other?.Y) && Z.Equals(other?.Z);
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
            return second is not null && first is not null && Math.Abs(first.X - second.X) < MathResources.Tolerance &&
                   Math.Abs(first.Y - second.Y) < MathResources.Tolerance &&
                   Math.Abs(first.Z - second.Z) < MathResources.Tolerance;
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
            return second is not null && first is not null && (Math.Abs(first.X - second.X) > MathResources.Tolerance ||
                                                               Math.Abs(first.Y - second.Y) > MathResources.Tolerance ||
                                                               Math.Abs(first.Z - second.Z) > MathResources.Tolerance);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        ///     Converts the Vector to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("X: ", X, " Y: ", Y, " Z: ", Z);
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
        ///     The result of the operator.
        /// </returns>
        public static double operator *(Vector3D first, Vector3D second)
        {
            return (first.X * second.X) + (first.Y * second.Y) + (first.Z * second.Z);
        }

        /// <summary>
        ///     Implements the operator *.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="scalar">The scalar.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Vector3D operator *(Vector3D v, double scalar)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
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
            {
                X = (Y * second.Z) - (Z * second.Y),
                Y = (Z * second.X) - (X * second.Z),
                Z = (X * second.Y) - (Y * second.X)
            };
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
            return new Vector3D { X = X / l, Y = Y / l, Z = Z / l };
        }

        /// <summary>
        ///     Angle between this and the other Vector
        /// </summary>
        /// <returns>Angle between both Vectors</returns>
        public double Angle(Vector3D second)
        {
            return Math.Acos((this * second) / (this.VectorLength() * second.VectorLength()));
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="Vector3D" /> to <see cref="Coordinate2D" />.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static explicit operator Coordinate2D(Vector3D first)
        {
            return new Coordinate2D(first.RoundedX, first.RoundedY);
        }

        /// <summary>
        ///     Converts to matrix.
        ///     In this case especially for 3D Projection
        /// </summary>
        /// <returns>Vector transformed to Matrix</returns>
        public static explicit operator BaseMatrix(Vector3D first)
        {
            var matrix = new double[4, 4];
            matrix[0, 0] = first.X;
            matrix[0, 1] = first.Y;
            matrix[0, 2] = first.Z;
            matrix[0, 3] = 1;
            return new BaseMatrix(matrix);
        }
    }
}
