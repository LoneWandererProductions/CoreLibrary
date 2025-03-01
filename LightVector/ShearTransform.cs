/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/ShearTransform.cs
 * PURPOSE:     Basic Transform Class, mostly for Bezier
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace LightVector
{
    /// <summary>
    ///     Sheer Transform for Curve
    /// </summary>
    /// <seealso cref="Transform" />
    public abstract class ShearTransform : Transform
    {
        protected ShearTransform(double shearX, double shearY)
        {
            ShearX = shearX;
            ShearY = shearY;
        }

        public double ShearX { get; }
        public double ShearY { get; }
    }
}
