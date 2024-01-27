using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public static class Projection3DMesh
    {
        /// <summary>
        /// Gets or sets the polygons.
        ///  Collection of polygons, which is a collection of vertices (tris or quads), for future reference we can use any geometry in the future
        /// </summary>
        /// <value>
        /// The polygons.
        /// </value>
        public static List<Triangle> Polygons { get; set; } = new List<Triangle>();

        // Triangles need to be supplied on a CLOCKWISE order
        public static void CreateTri(List<TertiaryVector> triangles)
        {
            for (int i = 0; i < triangles.Count - 3; i += 3)
            {
                var v1 = triangles[i];
                var v2 = triangles[i+1];
                var v3 = triangles[i+2];

                var triangle = new Triangle((Vector3D)v1, (Vector3D)v2, (Vector3D)v3);

                Polygons.Add(triangle);
            }
        }
    }

    public sealed class Triangle
    {
        public Vector3D Normal
        {
            get
            {
                var u = Vertices[1] - Vertices[0];
                var v = Vertices[2] - Vertices[0];

                return u.CrossProduct(v).Normalize();
            }
        }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            Vertices = new Vector3D[3];

            Vertices[0] = v1;
            Vertices[1] = v2;
            Vertices[2] = v3;
        }

        public int VertexCount => Vertices.Length;

        public Vector3D[] Vertices { get; }

        public Vector3D this[int i]
        {
            get => Vertices[i];
            set => Vertices[i] = value;
        }
    }
}
