/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        RectShape.cs
 * PURPOSE:     Ther Rectangle Shape.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Shapes;

/// <summary>
/// An axis-aligned rectangle.
/// </summary>
/// <seealso cref="Imaging.Objects.Shapes.Shape" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.Shape&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.RectShape&gt;" />
public sealed record RectShape(double X, double Y, double Width, double Height) : Shape
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
