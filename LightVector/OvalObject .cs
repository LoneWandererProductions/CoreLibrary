/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/OvalObject.cs
 * PURPOSE:     Generic oval Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Numerics;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents an oval (ellipse) object.
    /// </summary>
    [System.Serializable]
    public sealed class OvalObject : GraphicObject
    {
        /// <summary>
        ///     The center of the oval.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        ///     The horizontal radius of the oval.
        /// </summary>
        public float RadiusX { get; set; }

        /// <summary>
        ///     The vertical radius of the oval.
        /// </summary>
        public float RadiusY { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Checks if this object supports the given transformation.
        /// </summary>
        /// <param name="transformation"></param>
        /// <returns>If specific transformation is supported</returns>
        public override bool SupportsTransformation(Transform transformation)
        {
            return transformation is ScaleTransform or RotateTransform or TranslateTransform;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Apply transformation (scaling, rotation, translation) to the oval.
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        public override void ApplyTransformation(Transform transformation)
        {
            switch (transformation)
            {
                case ScaleTransform scale:
                    // Scale the radii
                    RadiusX *= scale.ScaleX;
                    RadiusY *= scale.ScaleY;
                    break;
                case RotateTransform rotate:
                    // Rotate the center of the oval
                    Center = RotateVector(Center, rotate.Angle);
                    break;
                case TranslateTransform translate:
                    // Translate the center of the oval
                    Center = new Vector2(Center.X + (float)translate.X, Center.Y + (float)translate.Y);
                    break;
            }
        }

        /// <summary>
        /// Rotates the vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>Rotated Vector</returns>
        private static Vector2 RotateVector(Vector2 vector, double angle)
        {
            // Convert angle to radians
            var angleRad = angle * (Math.PI / 180);

            // Apply rotation matrix
            var cosTheta = (float)Math.Cos(angleRad);
            var sinTheta = (float)Math.Sin(angleRad);

            // Rotate the vector
            var xNew = (vector.X * cosTheta) - (vector.Y * sinTheta);
            var yNew = (vector.X * sinTheta) + (vector.Y * cosTheta);

            return new Vector2(xNew, yNew);
        }
    }
}
