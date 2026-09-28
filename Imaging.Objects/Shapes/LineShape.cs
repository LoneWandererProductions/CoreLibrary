/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        LineShape.cs
 * PURPOSE:     The Line shape.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Shapes;

/// <summary>
/// A straight line. Open, so <see cref="Shape.Fill" /> is ignored.
/// </summary>
/// <seealso cref="Imaging.Objects.Shapes.Shape" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.Shape&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.LineShape&gt;" />
public sealed record LineShape(PointD From, PointD To) : Shape
{
    /// <inheritdoc />
    public override PixelRect GetBounds()
    {
        return BoundsOf(Math.Min(From.X, To.X), Math.Min(From.Y, To.Y), Math.Max(From.X, To.X),
            Math.Max(From.Y, To.Y));
    }

    /// <inheritdoc />
    public override Shape Translate(double dx, double dy)
    {
        return this with { From = new PointD(From.X + dx, From.Y + dy), To = new PointD(To.X + dx, To.Y + dy) };
    }

    /// <inheritdoc />
    public override bool IsValid()
    {
        return IsStyleValid() && double.IsFinite(From.X) && double.IsFinite(From.Y) && double.IsFinite(To.X) &&
               double.IsFinite(To.Y);
    }
}