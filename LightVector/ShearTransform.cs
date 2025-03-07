/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/ShearTransform.cs
 * PURPOSE:     Basic Transform Class, mostly for Bezier
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace LightVector
{
    /// <summary>
    ///     Sheer Transform for Curve
    /// </summary>
    /// <seealso cref="Transform" />
    public abstract class ShearTransform : Transform
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ShearTransform" /> class.
        /// </summary>
        /// <param name="shearX">The shear x.</param>
        /// <param name="shearY">The shear y.</param>
        protected ShearTransform(double shearX, double shearY)
        {
            ShearX = shearX;
            ShearY = shearY;
        }

        /// <summary>
        ///     Gets the shear x.
        /// </summary>
        /// <value>
        ///     The shear x.
        /// </value>
        public double ShearX { get; }

        /// <summary>
        ///     Gets the shear y.
        /// </summary>
        /// <value>
        ///     The shear y.
        /// </value>
        public double ShearY { get; }
    }
}
