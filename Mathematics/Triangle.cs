/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Triangle.cs
 * PURPOSE:     Helper Object to handle the description of the 3d object.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    /// <summary>
    /// In the future will be retooled to polygons.
    /// </summary>
    public sealed class Triangle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            Vertices = new Vector3D[3];

            Vertices[0] = v1;
            Vertices[1] = v2;
            Vertices[2] = v3;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Triangle" /> class.
        /// </summary>
        public Triangle()
        {
            Vertices = new Vector3D[3];

            Vertices[0] = new Vector3D();
            Vertices[1] = new Vector3D();
            Vertices[2] = new Vector3D();
        }

        /// <summary>
        /// Gets the normal.
        /// </summary>
        /// <value>
        /// The normal.
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
        /// Gets the vertex count.
        /// </summary>
        /// <value>
        /// The vertex count.
        /// </value>
        public int VertexCount => Vertices.Length;

        public Vector3D[] Vertices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Vector3D" /> with the specified i.
        /// </summary>
        /// <value>
        /// The <see cref="Vector3D" />.
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
        public static List<Triangle> CreateTri(List<TertiaryVector> triangles)
        {
            var polygons = new List<Triangle>();

            for (var i = 0; i <= triangles.Count - 3; i += 3)
            {
                var v1 = triangles[i];
                var v2 = triangles[i + 1];
                var v3 = triangles[i + 2];

                var triangle = new Triangle((Vector3D)v1, (Vector3D)v2, (Vector3D)v3);

                polygons.Add(triangle);
            }

            return polygons;
        }

        /// <summary>
        /// Gets the plot point.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>2d Vector, we only need these anyways for drawing.</returns>
        public Vector2D GetPlotPoint(int id)
        {
            return id > VertexCount || id < 0 ? null : (Vector2D) Vertices[id];
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(MathResources.StrOne, Vertices[0].ToString(), MathResources.StrTwo,
                Vertices[1].ToString(), MathResources.StrThree,
                Vertices[2].ToString());
        }
    }
}
