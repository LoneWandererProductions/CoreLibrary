/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/Transform.cs
 * PURPOSE:     Basic Transform Class 
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace LightVector
{
    /// <summary>
    ///     Empty Constructor
    /// </summary>
    public class Transform
    {
        // Base class for all transformations (e.g., scaling, rotating, translating)
    }

    /// <inheritdoc />
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

        /// <summary>
        ///     Gets the scale x.
        /// </summary>
        /// <value>
        ///     The scale x.
        /// </value>
        public float ScaleX { get; }

        /// <summary>
        ///     Gets the scale y.
        /// </summary>
        /// <value>
        ///     The scale y.
        /// </value>
        public float ScaleY { get; }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Transform Rotate
    /// </summary>
    /// <seealso cref="Transform" />
    public sealed class RotateTransform : Transform
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RotateTransform" /> class.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public RotateTransform(double angle)
        {
            Angle = angle;
        }

        /// <summary>
        ///     Gets the angle.
        /// </summary>
        /// <value>
        ///     The angle.
        /// </value>
        public double Angle { get; }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Transform Translate
    /// </summary>
    /// <seealso cref="Transform" />
    public sealed class TranslateTransform : Transform
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TranslateTransform" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public TranslateTransform(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Gets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public double X { get; }

        /// <summary>
        ///     Gets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public double Y { get; }
    }
}
