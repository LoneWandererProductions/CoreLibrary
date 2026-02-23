/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        PolyTriangle.cs
 * PURPOSE:     Helper Object to handle the description of the 3d object. It also supports more than 3 Vectors, in case we want to go full polygon.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Text;
using DataFormatter;

namespace Mathematics
{
    /// <summary>
    ///     Helper Object to handle the description of a 3D object.
    ///     Supports triangles and larger polygons.
    /// </summary>
    public sealed class PolyTriangle : IEquatable<PolyTriangle>
    {
        /// <summary>
        /// Gets the vertices.
        /// </summary>
        /// <value>
        /// The vertices.
        /// </value>
        public Vector3D[] Vertices { get; }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        /// <value>
        /// The vertex count.
        /// </value>
        public int VertexCount => Vertices.Length;

        /// <summary>
        /// Initializes a new instance for a generic polygon (N-gon).
        /// </summary>
        /// <param name="array">The array.</param>
        public PolyTriangle(IReadOnlyList<Vector3D> array)
        {
            Vertices = new Vector3D[array.Count];
            for (var i = 0; i < array.Count; i++)
            {
                Vertices[i] = array[i];
            }
        }

        /// <summary>
        ///     HIGH PERFORMANCE CONSTRUCTOR: Specifically for standard Triangles.
        ///     Prevents double heap-allocation of temporary arrays.
        /// </summary>
        public PolyTriangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            Vertices = new[] { v1, v2, v3 };
        }

        /// <summary>
        /// Gets the normal of the polygon.
        /// </summary>
        /// <value>
        /// The normal.
        /// </value>
        public Vector3D Normal
        {
            get
            {
                // Safety check: A line or point doesn't have a 3D normal
                if (VertexCount < 3) return Vector3D.ZeroVector;

                var u = Vertices[1] - Vertices[0];
                var v = Vertices[2] - Vertices[0];

                return u.CrossProduct(v).Normalize();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vector3D"/> with the specified i.
        /// </summary>
        /// <value>
        /// The <see cref="Vector3D"/>.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public Vector3D this[int i]
        {
            get => Vertices[i];
            set => Vertices[i] = value;
        }

        /// <summary>
        /// Creates the triangle set from a raw list of vertices.
        /// Triangles need to be supplied in a CLOCKWISE order.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>List of Poly Triangles.</returns>
        public static List<PolyTriangle> CreateTri(List<TertiaryVector> triangles)
        {
            // Pre-allocate list capacity for a slight performance boost
            var polygons = new List<PolyTriangle>(triangles.Count / 3);

            for (var i = 0; i <= triangles.Count - 3; i += 3)
            {
                // Use the new highly optimized constructor directly
                var tri = new PolyTriangle(
                    (Vector3D)triangles[i],
                    (Vector3D)triangles[i + 1],
                    (Vector3D)triangles[i + 2]
                );

                polygons.Add(tri);
            }

            return polygons;
        }

        /// <summary>
        /// Gets the plot point.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Vector2D GetPlotPoint(int id)
        {
            if (id >= VertexCount || id < 0) return Vector2D.ZeroVector;

            return (Vector2D)Vertices[id];
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(PolyTriangle other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (VertexCount != other.VertexCount) return false;

            for (var i = 0; i < VertexCount; i++)
            {
                if (Vertices[i] != other.Vertices[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is PolyTriangle other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            // Safely combine the hashes of up to the first 3 vertices 
            // to drastically reduce hash collisions for connected geometry.
            if (VertexCount >= 3)
                return HashCode.Combine(Vertices[0], Vertices[1], Vertices[2]);
            if (VertexCount == 2)
                return HashCode.Combine(Vertices[0], Vertices[1]);
            if (VertexCount == 1)
                return Vertices[0].GetHashCode();

            return 0;
        }

        public static bool operator ==(PolyTriangle first, PolyTriangle second)
        {
            if (first is null) return second is null;
            return first.Equals(second);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(PolyTriangle first, PolyTriangle second) => !(first == second);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Vertices.Length; i++)
            {
                sb.Append(i).Append(MathResources.Separator).Append(Vertices[i]).Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
