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
        ///     Generates the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The world transform.</param>
        /// <param name="orthogonal">The orthogonal.</param>
        /// <returns>
        ///     Converted 3d View
        /// </returns>
        public List<PolyTriangle> Generate(List<PolyTriangle> triangles, Transform transform, bool? orthogonal)
        {
            var cache = ProjectionRaster.WorldMatrix(triangles, transform);
            cache = transform.CameraType
                ? ProjectionRaster.OrbitCamera(cache, transform)
                : ProjectionRaster.PointAt(cache, transform);
            cache = ProjectionRaster.ViewPort(cache, transform.Position);

            cache = orthogonal == true
                ? ProjectionRaster.Convert2DTo3D(cache)
                : ProjectionRaster.Convert2DTo3DOrthographic(cache);

            //Todo Move into View and check if Triangles are even visible

            return ProjectionRaster.MoveIntoView(cache, Projection3DRegister.Width, Projection3DRegister.Height);
        }
    }
}
