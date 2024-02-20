using System.Collections.Generic;

namespace Mathematics
{
    public sealed class Rasterize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        ///     Worlds the matrix.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Do all Transformations for the 3D Model</returns>
        internal static List<Triangle> WorldMatrix(List<Triangle> triangles, Transform transform)
        {
            var lst = new List<Triangle>(triangles.Count);

            foreach (var triangle in triangles)
            {
                var triScaled = new Triangle
                {
                    [0] = triangle[0] * Projection3DCamera.ModelMatrix(transform),
                    [1] = triangle[1] * Projection3DCamera.ModelMatrix(transform),
                    [2] = triangle[2] * Projection3DCamera.ModelMatrix(transform)
                };
                lst.Add(triScaled);
            }

            return lst;
        }

        /// <summary>
        ///     View Camera.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Look though the lense</returns>
        internal static List<Triangle> PointAt(IEnumerable<Triangle> triangles, Transform transform)
        {
            var lst = new List<Triangle>();

            foreach (var triangle in triangles)
            {
                var triScaled = new Triangle
                {
                    [0] = triangle[0] * Projection3DCamera.OrbitCamera(transform),
                    [1] = triangle[1] * Projection3DCamera.OrbitCamera(transform),
                    [2] = triangle[2] * Projection3DCamera.OrbitCamera(transform)
                };
                lst.Add(triScaled);
            }

            return lst;
        }

        /// <summary>
        ///     Views the port.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="vCamera">The position of the camera as vector.</param>
        /// <returns>Visible Vector Planes</returns>
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

                if (normal * comparer > 0)
                {
                    continue;
                }

                //Todo here we would add some shading and textures

                lst.Add(triangle);
            }

            return lst;
        }

        /// <summary>
        ///     Convert2s the d to3 d.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>Coordinates converted into 3D Space</returns>
        internal static List<Triangle> Convert2DTo3D(List<Triangle> triangles)
        {
            var lst = new List<Triangle>(triangles.Count);

            foreach (var triangle in triangles)
            {
                var tri3D = new Triangle
                {
                    [0] = Projection3DCamera.ProjectionTo3D(triangle[0]),
                    [1] = Projection3DCamera.ProjectionTo3D(triangle[1]),
                    [2] = Projection3DCamera.ProjectionTo3D(triangle[2])
                };
                lst.Add(tri3D);
            }

            return lst;
        }

        /// <summary>
        ///     Convert2s the d to3 d orthographic.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>Coordinates converted into 3D Space</returns>
        internal static List<Triangle> Convert2DTo3DOrthographic(List<Triangle> triangles)
        {
            var lst = new List<Triangle>(triangles.Count);

            foreach (var triangle in triangles)
            {
                var tri3D = new Triangle
                {
                    [0] = Projection3DCamera.OrthographicProjectionTo3D(triangle[0]),
                    [1] = Projection3DCamera.OrthographicProjectionTo3D(triangle[1]),
                    [2] = Projection3DCamera.OrthographicProjectionTo3D(triangle[2])
                };
                lst.Add(tri3D);
            }

            return lst;
        }

        /// <summary>
        ///     Moves the into view.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Center on Screen</returns>
        internal static List<Triangle> MoveIntoView(IEnumerable<Triangle> triangles, int width, int height)
        {
            var lst = new List<Triangle>();
            var raster = new Rasterize { Width = width, Height = height };

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
