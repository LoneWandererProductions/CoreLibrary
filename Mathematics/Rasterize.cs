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
        ///     Orbit Camera.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Look though the lens</returns>
        internal static List<Triangle> OrbitCamera(IEnumerable<Triangle> triangles, Transform transform)
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
        /// Points at Camera.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Look though the lens</returns>
        internal static List<Triangle> PointAt(IEnumerable<Triangle> triangles, Transform transform)
        {
            var lst = new List<Triangle>();

            foreach (var triangle in triangles)
            {
                var triScaled = new Triangle
                {
                    [0] = triangle[0] * Projection3DCamera.PointAt(transform),
                    [1] = triangle[1] * Projection3DCamera.PointAt(transform),
                    [2] = triangle[2] * Projection3DCamera.PointAt(transform)
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

                //Todo add a better algorithm!

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

            foreach (var triangle in triangles)
            {
                // Scale into view, we moved the normalising into cartesian space
                // out of the matrix.vector function from the previous videos, so
                // do this manually
                triangle[0] /= triangle[0].W;
                triangle[1] /= triangle[1].W;
                triangle[2] /= triangle[2].W;

                // X/Y are inverted so put them back
                triangle[0].X *= -1.0f;
                triangle[1].X *= -1.0f;
                triangle[2].X *= -1.0f;
                triangle[0].Y *= -1.0f;
                triangle[1].Y *= -1.0f;
                triangle[2].Y *= -1.0f;

                // Offset verts into visible normalised space
                var vOffsetView = new Vector3D(1, 1, 0);
                triangle[0] += vOffsetView;
                triangle[1] += vOffsetView;
                triangle[2] += vOffsetView;

                triangle[0].X *= 0.5d * width;
                triangle[0].Y *= 0.5d * height;
                triangle[1].X *= 0.5d * width;
                triangle[1].Y *= 0.5d * height;
                triangle[2].X *= 0.5d * width;
                triangle[2].Y *= 0.5d * height;

                lst.Add(triangle);
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
