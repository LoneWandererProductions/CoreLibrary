/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/IProjection.cs
 * PURPOSE:     3D Projection Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    /// <summary>
    ///     The Projection Interface.
    ///     Template for all external 3D operations.
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        ///     Generates the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The world transform.</param>
        /// <param name="angle">The angle of the camera.</param>
        /// <param name="vCamera">The position of the camera as vector.</param>
        /// <param name="orthogonal">The orthogonal.</param>
        /// <returns>Converted 3d View</returns>
        List<Triangle> Generate(List<Triangle> triangles, Transform transform, Vector3D vCamera, double angle, bool? orthogonal)
    }
}
