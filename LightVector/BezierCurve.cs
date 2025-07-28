/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/BezierCurve.cs
 * PURPOSE:     Hold the CurvBecier e Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Numerics;

namespace LightVector;

/// <inheritdoc />
/// <summary>
///     The curve object class.
/// </summary>
[System.Serializable]
public sealed class BezierCurve : GraphicObject
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
    ///     Applies a transformation to each vector in the curve object.
    /// </summary>
    /// <param name="transformation">The transformation to apply (scale, rotate, translate).</param>
    public override void ApplyTransformation(Transform transformation)
    {
        for (var i = 0; i < Vectors.Count; i++)
        {
            switch (transformation)
            {
                case ScaleTransform scale:
                    Vectors[i] = new Vector2(Vectors[i].X * scale.ScaleX, Vectors[i].Y * scale.ScaleY);
                    break;
                case RotateTransform rotate:
                    Vectors[i] = RotateVector(Vectors[i], rotate.Angle);
                    break;
                case TranslateTransform translate:
                    Vectors[i] = new Vector2(Vectors[i].X + (float)translate.X, Vectors[i].Y + (float)translate.Y);
                    break;
                case ShearTransform shear:
                    Vectors[i] = new Vector2(
                        Vectors[i].X + ((float)shear.ShearX * Vectors[i].Y),
                        Vectors[i].Y + ((float)shear.ShearY * Vectors[i].X)
                    );
                    break;
            }
        }
    }

    /// <summary>
    ///     Rotate a vector by a specified angle (in degrees).
    /// </summary>
    /// <param name="vector">The vector to rotate.</param>
    /// <param name="angle">The angle of rotation in degrees.</param>
    /// <returns>The rotated vector.</returns>
    private static Vector2 RotateVector(Vector2 vector, double angle)
    {
        // Convert angle to radians
        var angleRad = angle * (Math.PI / 180);

        // Use the rotation matrix to rotate the vector
        var cosTheta = (float)Math.Cos(angleRad);
        var sinTheta = (float)Math.Sin(angleRad);

        // Rotate the vector
        var xNew = (vector.X * cosTheta) - (vector.Y * sinTheta);
        var yNew = (vector.X * sinTheta) + (vector.Y * cosTheta);

        return new Vector2(xNew, yNew);
    }
}
