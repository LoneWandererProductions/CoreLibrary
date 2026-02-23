/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        ProjectionRaster.cs
 * PURPOSE:     helper Class, that does all the heavy lifting.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;

namespace Mathematics
{
    internal static class ProjectionRaster
    {
        /// <summary>
        ///     Worlds the matrix.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Do all Transformations for the 3D Model</returns>
        internal static List<PolyTriangle> WorldMatrix(List<PolyTriangle> triangles, Transform transform)
        {
            var lst = new List<PolyTriangle>(triangles.Count);

            lst.AddRange(triangles.Select(triangle => new[]
            {
                triangle[0] * Projection3DCamera.ModelMatrix(transform),
                triangle[1] * Projection3DCamera.ModelMatrix(transform),
                triangle[2] * Projection3DCamera.ModelMatrix(transform)
            }).Select(array => new PolyTriangle(array)));

            return lst;
        }

        /// <summary>
        ///     Orbit Camera.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Look though the lens</returns>
        internal static List<PolyTriangle> OrbitCamera(IEnumerable<PolyTriangle> triangles, Transform transform)
        {
            return triangles.Select(triangle => new[]
            {
                triangle[0] * Projection3DCamera.OrbitCamera(transform),
                triangle[1] * Projection3DCamera.OrbitCamera(transform),
                triangle[2] * Projection3DCamera.OrbitCamera(transform)
            }).Select(array => new PolyTriangle(array)).ToList();
        }

        /// <summary>
        ///     Points at Camera.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Look though the lens</returns>
        internal static List<PolyTriangle> PointAt(IEnumerable<PolyTriangle> triangles, Transform transform)
        {
            return triangles
                .Select(triangle => new[]
                {
                    triangle[0] * Projection3DCamera.PointAt(transform),
                    triangle[1] * Projection3DCamera.PointAt(transform),
                    triangle[2] * Projection3DCamera.PointAt(transform)
                }).Select(array => new PolyTriangle(array)).ToList();
        }

        /// <summary>
        ///     Backfaces culled.
        ///     Shoelace formula: https://en.wikipedia.org/wiki/Shoelace_formula#Statement
        ///     Division by 2 is not necessary, since all we care about is if the value is positive/negative
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>All visible Triangles</returns>
        internal static List<PolyTriangle> Clipping(IEnumerable<PolyTriangle> triangles)
        {
            var lst = new List<PolyTriangle>();

            foreach (var triangle in triangles)
            {
                double sum = 0;

                for (var i = 0; i < triangle.VertexCount; i++)
                {
                    sum += (triangle[i].X * triangle[(i + 1) % triangle.VertexCount].Y) -
                           (triangle[i].Y * triangle[(i + 1) % triangle.VertexCount].X);
                }

                if (sum >= 0)
                {
                    continue;
                }

                lst.Add(triangle);
            }

            return lst;
        }

        /// <summary>
        ///     Convert2s the d to3 d.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>Coordinates converted into 3D Space</returns>
        internal static List<PolyTriangle> Convert2DTo3D(List<PolyTriangle> triangles)
        {
            var lst = new List<PolyTriangle>(triangles.Count);
            lst.AddRange(triangles
                .Select(triangle => new[]
                {
                    Projection3DCamera.ProjectionTo3D(triangle[0]), Projection3DCamera.ProjectionTo3D(triangle[1]),
                    Projection3DCamera.ProjectionTo3D(triangle[2])
                }).Select(array => new PolyTriangle(array)));

            return lst;
        }

        /// <summary>
        ///     Convert2s the d to3 d orthographic.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <returns>Coordinates converted into 3D Space</returns>
        internal static List<PolyTriangle> Convert2DTo3DOrthographic(List<PolyTriangle> triangles)
        {
            var lst = new List<PolyTriangle>(triangles.Count);
            lst.AddRange(triangles.Select(triangle => new[]
            {
                Projection3DCamera.OrthographicProjectionTo3D(triangle[0]),
                Projection3DCamera.OrthographicProjectionTo3D(triangle[1]),
                Projection3DCamera.OrthographicProjectionTo3D(triangle[2])
            }).Select(array => new PolyTriangle(array)));

            return lst;
        }

        /// <summary>
        ///     Moves the into view.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        ///     Center on Screen
        /// </returns>
        internal static List<PolyTriangle> MoveIntoView(IEnumerable<PolyTriangle> triangles, int width, int height)
        {
            var lst = new List<PolyTriangle>();

            // Pre-calculate the scales once per frame/call, not per vertex!
            double scaleX = 0.5d * width;
            double scaleY = 0.5d * height;

            foreach (var triangle in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    var v = triangle[i];

                    // Safety check: Prevent DivisionByZero if W is 0
                    double w = (v.W != 0) ? v.W : 1.0d;

                    // Combine Divide (Normalize), Invert, Offset, and Scale into branchless math
                    double newX = (-(v.X / w) + 1.0d) * scaleX;
                    double newY = (-(v.Y / w) + 1.0d) * scaleY;
                    double newZ = v.Z / w;

                    // Create the immutable struct and explicitly overwrite the old one in the triangle
                    triangle[i] = new Vector3D(newX, newY, newZ, w);
                }

                lst.Add(triangle);
            }

            return lst;
        }

        /// <summary>
        ///     Moves the orthographic Object into view.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        ///     Center on Screen
        /// </returns>
        public static List<PolyTriangle> MoveIntoViewOrthographic(IEnumerable<PolyTriangle> triangles, int width, int height)
        {
            var lst = new List<PolyTriangle>();

            // Calculate scale modifiers once outside the loop to save CPU cycles
            double scaleX = 0.25d * width;
            double scaleY = 0.25d * height;

            foreach (var triangle in triangles)
            {
                // Loop through the 3 vertices of the triangle
                for (int i = 0; i < 3; i++)
                {
                    var v = triangle[i];

                    // 1. Invert X/Y, Add Offset, and Scale all in one branchless step
                    double newX = (-v.X + 2.5d) * scaleX;
                    double newY = (-v.Y + 3.0d) * scaleY;

                    // 2. Create the new immutable Vector3D and assign it back.
                    // Note: We preserve the original Z and W values!
                    triangle[i] = new Vector3D(newX, newY, v.Z, v.W);
                }

                lst.Add(triangle);
            }

            return lst;
        }
    }
}
