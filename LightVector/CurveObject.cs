/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/CurveObject.cs
 * PURPOSE:     Hold the Curve Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Numerics;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     The curve object class.
    /// </summary>
    [Serializable]
    public sealed class CurveObject : GraphicObject
    {
        /// <summary>
        ///     Gets or sets the vectors (Vector2).
        /// </summary>
        public List<Vector2> Vectors { get; set; } = new();

        /// <summary>
        ///     Gets or sets the tension.
        /// </summary>
        public double Tension { get; init; } = 1.0;

        /// <inheritdoc />
        /// <summary>
        ///     Applies a transformation to each vector in the curve object.
        /// </summary>
        /// <param name="transformation">The transformation to apply (scale, rotate, translate).</param>
        public override void ApplyTransformation(Transform transformation)
        {
            for (int i = 0; i < Vectors.Count; i++)
            {
                if (transformation is ScaleTransform scale)
                {
                    // Apply scaling transformation to each vector
                    Vectors[i] = new Vector2(Vectors[i].X * (float)scale.ScaleX, Vectors[i].Y * (float)scale.ScaleY);
                }
                else if (transformation is RotateTransform rotate)
                {
                    // Apply rotation transformation to each vector
                    Vectors[i] = RotateVector(Vectors[i], rotate.Angle);
                }
                else if (transformation is TranslateTransform translate)
                {
                    // Translation involves modifying the vector's position (x and y) directly
                    Vectors[i] = new Vector2(Vectors[i].X + (float)translate.X, Vectors[i].Y + (float)translate.Y);
                }
            }
        }

        /// <summary>
        ///     Rotate a vector by a specified angle (in degrees).
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <returns>The rotated vector.</returns>
        private Vector2 RotateVector(Vector2 vector, double angle)
        {
            // Convert angle to radians
            double angleRad = angle * (Math.PI / 180);

            // Use the rotation matrix to rotate the vector
            float cosTheta = (float)Math.Cos(angleRad);
            float sinTheta = (float)Math.Sin(angleRad);

            // Rotate the vector
            float xNew = vector.X * cosTheta - vector.Y * sinTheta;
            float yNew = vector.X * sinTheta + vector.Y * cosTheta;

            return new Vector2(xNew, yNew);
        }
    }
}
