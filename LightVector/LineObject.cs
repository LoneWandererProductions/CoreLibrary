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
using System.Numerics;  // Use System.Numerics for cross-platform math and geometry

namespace LightVector
{
    /// <summary>
    ///     The line object class.
    /// </summary>
    [Serializable]
    public sealed class LineObject : GraphicObject
    {
        /// <summary>
        ///     Gets or sets the start point.
        /// </summary>
        public Vector2 StartPoint { get; set; }

        /// <summary>
        ///     Gets or sets the end point.
        /// </summary>
        public Vector2 EndPoint { get; set; }

        public override void ApplyTransformation(Transform transformation)
        {
            if (transformation is ScaleTransform scale)
            {
                // Apply scaling transformation using Vector2
                StartPoint *= new Vector2(scale.ScaleX, scale.ScaleY);
                EndPoint *= new Vector2(scale.ScaleX, scale.ScaleY);
            }
            else if (transformation is RotateTransform rotate)
            {
                // Apply rotation transformation using Matrix3x2
                float angleRad = (float)(rotate.Angle * (Math.PI / 180));  // Convert to radians
                var rotationMatrix = Matrix3x2.CreateRotation(angleRad);

                StartPoint = Vector2.Transform(StartPoint, rotationMatrix);
                EndPoint = Vector2.Transform(EndPoint, rotationMatrix);
            }
            else if (transformation is TranslateTransform translate)
            {
                // Apply translation transformation using Vector2
                StartPoint += new Vector2((float)translate.X, (float)translate.Y);
                EndPoint += new Vector2((float)translate.X, (float)translate.Y);
            }
        }
    }
}
