/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/PolyTriangle.cs
 * PURPOSE:     Helper Object to handle the description of the 3d object. It also supports more than 3 Vectors, in case we want to go full polygon.s
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    /// <summary>
    ///     In the future will be retooled to polygons.
    /// </summary>
    public sealed class PolyTriangle
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PolyTriangle" /> class.
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
        ///     Gets the normal.
        /// </summary>
        /// <value>
        ///     The normal.
        /// </value>
        public Vector3D Normal
        {
            get
            {
                var u = Vertices[1] - Vertices[0];
                var v = Vertices[2] - Vertices[0];

                return u.CrossProduct(v).Normalize();
            }
        }

        /// <summary>
        ///     Gets the vertex count.
        /// </summary>
        /// <value>
        ///     The vertex count.
        /// </value>
        public int VertexCount => Vertices.Length;

        /// <summary>
        ///     Gets or sets the vertices.
        /// </summary>
        /// <value>
        ///     The vertices.
        /// </value>
        public Vector3D[] Vertices { get; }

        /// <summary>
        ///     Gets or sets the <see cref="Vector3D" /> with the specified i.
        /// </summary>
        /// <value>
        ///     The <see cref="Vector3D" />.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns>vector by id</returns>
        public Vector3D this[int i]
        {
            get => Vertices[i];
            set => Vertices[i] = value;
        }

        /// <summary>
        ///     Creates the triangle set.
        ///     Triangles need to be supplied on a CLOCKWISE order
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>A list with Triangles, three Vectors in one Object</returns>
        public static List<PolyTriangle> CreateTri(List<TertiaryVector> triangles)
        {
            var polygons = new List<PolyTriangle>();

            for (var i = 0; i <= triangles.Count - 3; i += 3)
            {
                var v1 = triangles[i];
                var v2 = triangles[i + 1];
                var v3 = triangles[i + 2];

                var array = new[] { (Vector3D)v1, (Vector3D)v2, (Vector3D)v3 };
                var tri = new PolyTriangle(array);

                polygons.Add(tri);
            }

            return polygons;
        }

        /// <summary>
        ///     Gets the plot point.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>2d Vector, we only need these anyways for drawing.</returns>
        public Vector2D GetPlotPoint(int id)
        {
            return id > VertexCount || id < 0 ? null : (Vector2D)Vertices[id];
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var str = string.Empty;

            for (var i = 0; i < Vertices.Length; i++)
            {
                str = string.Concat(str, i, MathResources.Separator, Vertices[i].ToString(), Environment.NewLine);
            }

            return str;
        }
    }
}
