/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/CircleObject.cs
 * PURPOSE:     Generic circle Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Numerics;

namespace LightVector;

/// <inheritdoc />
/// <summary>
///     Represents a circle object.
/// </summary>
[System.Serializable]
public sealed class CircleObject : GraphicObject
{
    /// <summary>
    ///     The center of the circle.
    /// </summary>
    public Vector2 Center { get; set; }

    /// <summary>
    ///     The radius of the circle.
    /// </summary>
    public float Radius { get; set; }

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
    ///     Apply transformation (scaling, rotation, translation) to the circle.
    /// </summary>
    /// <param name="transformation">The transformation.</param>
    public override void ApplyTransformation(Transform transformation)
    {
        switch (transformation)
        {
            case ScaleTransform scale:
                // Scale the radius (the center is unchanged)
                Radius *= scale.ScaleX; // Assuming uniform scale
                break;
            case RotateTransform rotate:
                // Rotate the center of the circle
                Center = RotateVector(Center, rotate.Angle);
                break;
            case TranslateTransform translate:
                // Translate the center of the circle
                Center = new Vector2(Center.X + (float)translate.X, Center.Y + (float)translate.Y);
                break;
        }
    }

    /// <summary>
    /// Rotates the vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="angle">The angle.</param>
    /// <returns>New Vector.</returns>
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
