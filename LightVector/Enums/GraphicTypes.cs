/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector.Enums
 * FILE:        GraphicTypes.cs
 * PURPOSE:     Types of graphic objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LightVector.Enums
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
        ///     The bezier curve
        /// </summary>
        BezierCurve = 2,

        /// <summary>
        ///     The polygon
        /// </summary>
        Polygon = 3,

        /// <summary>
        ///     The circle
        /// </summary>
        Circle = 4,

        /// <summary>
        ///     The oval
        /// </summary>
        Oval = 5
    }
}
