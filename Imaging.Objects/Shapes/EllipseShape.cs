/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        EllipseShape.cs
 * PURPOSE:     Shape Info fo Ellipse
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Shapes;

/// <summary>
/// An axis-aligned ellipse, described by its bounding box (like a selection frame).
/// </summary>
/// <seealso cref="Imaging.Objects.Shapes.Shape" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.Shape&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.EllipseShape&gt;" />
public sealed record EllipseShape(double X, double Y, double Width, double Height) : Shape
{
    /// <inheritdoc />
    public override PixelRect GetBounds()
    {
        return BoundsOf(Math.Min(X, X + Width), Math.Min(Y, Y + Height), Math.Max(X, X + Width),
            Math.Max(Y, Y + Height));
    }

    /// <inheritdoc />
    public override Shape Translate(double dx, double dy)
    {
        return this with { X = X + dx, Y = Y + dy };
    }

    /// <inheritdoc />
    public override bool IsValid()
    {
        return IsStyleValid() && double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Width) &&
               double.IsFinite(Height) && Width >= 0 && Height >= 0;
    }
}