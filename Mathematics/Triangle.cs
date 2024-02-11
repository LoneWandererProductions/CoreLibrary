using System;
using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public class Triangle
    {
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
        ///     Initializes a new instance of the <see cref="Triangle" /> class.
        /// </summary>
        public Triangle()
        {
            Vertices = new Vector3D[3];

            Vertices[0] = new Vector3D();
            Vertices[1] = new Vector3D();
            Vertices[2] = new Vector3D();
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

        public static IEnumerable<Vector3D> GetCoordinates(IEnumerable<Triangle> render)
        {
            var lst = new List<Vector3D>();

            foreach (var triangle in render)
            {
                lst.AddRange(triangle.Vertices);
            }

            return lst;
        }

        public void Set(Vector3D one, Vector3D two, Vector3D three)
        {
            Vertices[0] = one;
            Vertices[1] = two;
            Vertices[2] = three;
        }

        public override string ToString()
        {
            return string.Concat(MathResources.StrOne, Vertices[0].ToString(), MathResources.StrTwo,
                Vertices[1].ToString(), MathResources.StrThree,
                Vertices[2].ToString());
        }
    }
}
