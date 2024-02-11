using System.Collections.Generic;
using System.Diagnostics;

namespace Mathematics
{
    public sealed class Rasterize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        internal static List<Triangle> WorldMatrix(IEnumerable<Triangle> triangles, Transform transform)
        {
            var lst = new List<Triangle>();

            foreach (var triangle in triangles)
            {
                var triScaled = new Triangle
                {
                    [0] = triangle[0] * Projection3DCamera.ModelMatrix(transform),
                    [1] = triangle[1] * Projection3DCamera.ModelMatrix(transform),
                    [2] = triangle[2] * Projection3DCamera.ModelMatrix(transform),
                };
                lst.Add(triScaled);
            }

            return lst;
        }

        internal static List<Triangle> ViewPort(IEnumerable<Triangle> triangles, Vector3D vCamera)
        {
            var lst = new List<Triangle>();

            foreach (var triangle in triangles)
            {
                var lineOne = triangle[1] - triangle[0];

                var lineTwo = triangle[2] - triangle[0];

                var normal = lineOne.CrossProduct(lineTwo);

                normal = normal.Normalize();

                var comparer = triangle[0] - vCamera;

                if (normal * comparer > 0) continue;

                lst.Add(triangle);
            }

            return lst;
        }

        internal static List<Triangle> Convert2DTo3D(List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                var one = Projection3DCamera.ProjectionTo3D(triangle[0]);
                var two = Projection3DCamera.ProjectionTo3D(triangle[1]);
                var three = Projection3DCamera.ProjectionTo3D(triangle[2]);
                triangle.Set(one, two, three);
            }

            return triangles;
        }

        internal static List<Triangle> Convert2DTo3DOrthographic(List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                var one = Projection3DCamera.OrthographicProjectionTo3D(triangle[0]);
                var two = Projection3DCamera.OrthographicProjectionTo3D(triangle[1]);
                var three = Projection3DCamera.OrthographicProjectionTo3D(triangle[2]);
                triangle.Set(one, two, three);
            }

            return triangles;
        }

        internal static List<Triangle> MoveIntoView(List<Triangle> triangles, int Width, int Height)
        {
            var lst = new List<Triangle>();
            var raster = new Rasterize { Width = Width, Height = Height };

            foreach (var triangle in triangles)
            {
                var triScaled = new Triangle();
                var cache = new Vector3D { X = triangle[0].X, Y = triangle[0].Y, Z = triangle[0].Z };
                triScaled[0] = raster.ConvertToRaster(cache);

                cache = new Vector3D { X = triangle[1].X, Y = triangle[1].Y, Z = triangle[1].Z };
                triScaled[1] = raster.ConvertToRaster(cache);

                cache = new Vector3D { X = triangle[2].X, Y = triangle[2].Y, Z = triangle[2].Z };
                triScaled[2] = raster.ConvertToRaster(cache);

                lst.Add(triScaled);
            }

            return lst;
        }

        /// <summary>
        ///     Converts to raster.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>New Coordinates to center the View into the Image</returns>
        public Vector3D ConvertToRaster(Vector3D v)
        {
            return new Vector3D((int)((v.X + 1) * 0.5d * Width),
                (int)((1 - ((v.Y + 1) * 0.5d)) * Height),
                -v.Z);
        }
    }
}
