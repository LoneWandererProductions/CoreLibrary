/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/IProjection.cs
 * PURPOSE:     Implementation of thhe 3D Projection Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Mathematics
{
    /// <inheritdoc />
    /// <summary>
    ///     The Projection class.
    ///     Handle all 3D Operations in an isolated class.
    /// </summary>
    public sealed class Projection : IProjection
    {
        /// <inheritdoc />
        /// <summary>
        /// Generates the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The world transform.</param>
        /// <param name="orthogonal">The orthogonal.</param>
        /// <returns>
        /// Converted 3d View
        /// </returns>
        public List<Triangle> Generate(List<Triangle> triangles, Transform transform, bool? orthogonal)
        {
            var cache = Rasterize.WorldMatrix(triangles, transform);
            cache = transform.CameraType ? Rasterize.OrbitCamera(cache, transform) : Rasterize.PointAt(cache, transform);
            cache = Rasterize.ViewPort(cache, transform.Position);

            cache = orthogonal == true ? Rasterize.Convert2DTo3D(cache) : Rasterize.Convert2DTo3DOrthographic(cache);

            //Todo Move into View and check if Triangles are even visible

            return Rasterize.MoveIntoView(cache, Projection3DRegister.Width, Projection3DRegister.Height);
        }
    }
}
