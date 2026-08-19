/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        ProjectionMode.cs
 * PURPOSE:     Projection Mode for the Viewport
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Solaris
{
    /// <summary>
    /// Specifies the projection model for spatial coordinate transformations.
    /// </summary>
    public enum ProjectionMode
    {
        /// <summary>
        /// The orthographic 2d
        /// </summary>
        Orthographic2D,

        /// <summary>
        /// The isometric
        /// </summary>
        Isometric
    }
}
