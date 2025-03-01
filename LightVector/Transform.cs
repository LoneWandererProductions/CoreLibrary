/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/Transform.cs
 * PURPOSE:     Basic Transform Class 
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace LightVector
{
    public class Transform
    {
        // Base class for all transformations (e.g., scaling, rotating, translating)
    }

    /// <summary>
    ///     Transform Scale
    /// </summary>
    /// <seealso cref="Transform" />
    public sealed class ScaleTransform : Transform
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScaleTransform" /> class.
        /// </summary>
        /// <param name="scaleX">The scale x.</param>
        /// <param name="scaleY">The scale y.</param>
        public ScaleTransform(float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        public float ScaleX { get; }
        public float ScaleY { get; }
    }

    /// <summary>
    ///     Transform Rotate
    /// </summary>
    /// <seealso cref="Transform" />
    public sealed class RotateTransform : Transform
    {
        public RotateTransform(double angle)
        {
            Angle = angle;
        }

        public RotateTransform()
        {
        }

        public double Angle { get; }
    }

    /// <summary>
    ///     Transform Translate
    /// </summary>
    /// <seealso cref="Transform" />
    public sealed class TranslateTransform : Transform
    {
        public TranslateTransform(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }
}
