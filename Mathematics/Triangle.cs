using System;
using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public class Triangle
    {
        /// <summary>
        /// Creates the triangle set.
        /// Triangles need to be supplied on a CLOCKWISE order
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>A list with Triangles, three Vectors in one Object</returns>
        public static List<Triangle> CreateTri(List<TertiaryVector> triangles)
        {
            List<Triangle> polygons = new List<Triangle>();
            for (int i = 0; i <= triangles.Count - 3; i += 3)
            {
                var v1 = triangles[i];
                var v2 = triangles[i + 1];
                var v3 = triangles[i + 2];

                var triangle = new Triangle((Vector3D)v1, (Vector3D)v2, (Vector3D)v3);

                polygons.Add(triangle);
            }

            return polygons;
        }

        public Triangle(Vector3D[] vertices)
        {
            Array.Resize(ref vertices, 3);

            Vertices = vertices;
        }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            Vertices = new Vector3D[3];

            Vertices[0] = v1;
            Vertices[1] = v2;
            Vertices[2] = v3;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        protected Triangle()
        {
        }

        public Vector3D Normal
        {
            get
            {
                var u = Vertices[1] - Vertices[0];
                var v = Vertices[2] - Vertices[0];

                return u.CrossProduct(v).Normalize();
            }
        }

        public int VertexCount => Vertices.Length;

        public Vector3D[] Vertices { get; set; }

        public Vector3D this[int i]
        {
            get => Vertices[i];
            set => Vertices[i] = value;
        }

        public override string ToString()
        {
            return string.Concat("1: ", Vertices[0].ToString(), " 2: ", Vertices[1].ToString(), " 3: ", Vertices[2].ToString());
        }
    }
}
