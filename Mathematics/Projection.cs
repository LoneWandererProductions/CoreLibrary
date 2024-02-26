/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/IProjection.cs
 * PURPOSE:     Implementation of thhe 3D Projection Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
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
        /// <returns>
        ///     Converted 3d View
        /// </returns>
        public List<PolyTriangle> Generate(List<PolyTriangle> triangles, Transform transform)
        {
            var cache = ProjectionRaster.WorldMatrix(triangles, transform);
            switch (transform.CameraType)
            {
                case Cameras.Orbit:
                    cache = ProjectionRaster.OrbitCamera(cache, transform);
                    break;
                case Cameras.PointAt:
                    cache = ProjectionRaster.PointAt(cache, transform);
                    break;
                default:
                    cache = ProjectionRaster.OrbitCamera(cache, transform);
                    break;
            }

            cache = ProjectionRaster.Clipping(cache, transform.Position);

            cache = transform.DisplayType switch
            {
                Display.Normal => ProjectionRaster.Convert2DTo3D(cache),
                Display.Orthographic => ProjectionRaster.Convert2DTo3DOrthographic(cache),
                _ => ProjectionRaster.Convert2DTo3D(cache)
            };

            return ProjectionRaster.MoveIntoView(cache, Projection3DRegister.Width, Projection3DRegister.Height);
        }
    }
}
