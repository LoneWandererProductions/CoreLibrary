/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/LineObject.cs
 * PURPOSE:     Hold the Line Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Numerics;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     The line object class.
    /// </summary>
    [System.Serializable]
    public sealed class LineObject : GraphicObject
    {
        /// <summary>
        /// The direction vector from the start point to the end point.
        /// </summary>
        public Vector2 Direction { get; set; }

        /// <summary>
        /// Gets or sets the tension.
        /// See BezierCurveFactory for the use
        /// </summary>
        /// <value>
        /// The tension.
        /// </value>
        public double Tension { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Checks if this object supports the given transformation.
        /// </summary>
        /// <param name="transformation"></param>
        /// <returns>If specific transformation is supported</returns>
        public override bool SupportsTransformation(Transform transformation)
        {
            return transformation is ScaleTransform or RotateTransform;
        }

        /// <inheritdoc />
        /// <summary>
        /// Apply transformation method (scaling, rotation, etc.)
        /// Each subclass will override this method to implement specific transformation logic
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        public override void ApplyTransformation(Transform transformation)
        {
            switch (transformation)
            {
                case ScaleTransform scale:
                    // Scale the direction vector
                    Direction *= new Vector2(scale.ScaleX, scale.ScaleY);
                    break;
                case RotateTransform rotate:
                    // Rotate the direction vector
                    var angleRad = (float)(rotate.Angle * (Math.PI / 180));
                    Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(angleRad));
                    break;
            }
        }
    }
}
