/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Enums
 * FILE:        MaskShape.cs
 * PURPOSE:     Enum that shows all allowed shapes
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Local

namespace Imaging.Enums
{
    /// <summary>
    ///     Texture Shapes
    /// </summary>
    public enum MaskShape
    {
        /// <summary>
        ///     The rectangle
        /// </summary>
        Rectangle = 0,

        /// <summary>
        ///     The circle
        /// </summary>
        Circle = 1,

        /// <summary>
        ///     The polygon
        /// </summary>
        Polygon = 2
    }
}