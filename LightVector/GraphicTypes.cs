/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/GraphicTypes.cs
 * PURPOSE:     Types of graphic objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LightVector
{
    /// <summary>
    ///     Describes the typ of Vector Object
    /// </summary>
    public enum GraphicTypes
    {
        /// <summary>
        ///     The line
        /// </summary>
        Point = 0,

        /// <summary>
        ///     The line
        /// </summary>
        Line = 1,

        /// <summary>
        ///     The curve
        /// </summary>
        Curve = 2
    }
}
