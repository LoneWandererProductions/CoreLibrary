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
    /// Sheer Transform for Curve
    /// </summary>
    /// <seealso cref="Transform" />
    public abstract class ShearTransform : Transform
    {
        public double ShearX { get; }
        public double ShearY { get; }

        protected ShearTransform(double shearX, double shearY)
        {
            ShearX = shearX;
            ShearY = shearY;
        }
    }

}
