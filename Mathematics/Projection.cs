﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/IProjection.cs
 * PURPOSE:     Implementation of thhe 3D Projection Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    /// <inheritdoc />
    /// <summary>
    ///     The Projection class.
    ///     Handle all 3D Operations in an isolated class.
    /// </summary>
    public class Projection : IProjection
    {
        /// <inheritdoc />
        /// <summary>
        ///     Generates the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The world transform.</param>
        /// <param name="angle">The angle of the camera.</param>
        /// <param name="vCamera">The position of the camera as vector.</param>
        /// <param name="orthogonal">The orthogonal.</param>
        /// <returns>Converted 3d View</returns>
        public List<Triangle> Generate(List<Triangle> triangles, Transform transform, Vector3D vCamera, double angle, bool? orthogonal)
        {
            var cache = Rasterize.WorldMatrix(triangles, transform);
            cache = Rasterize.ViewPort(cache, angle, vCamera);

            cache = orthogonal == true ? Rasterize.Convert2DTo3D(cache) : Rasterize.Convert2DTo3DOrthographic(cache);

            return cache;
        }


        public List<Triangle> MoveIntoView(List<Triangle> triangles, int width, int height)
        {
            return Rasterize.MoveIntoView(triangles, width, height);
        }
    }
}
