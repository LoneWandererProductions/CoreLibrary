/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        PolygonShape.cs
 * PURPOSE:     The Polygon Shape.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using System.Collections.Immutable;

namespace Imaging.Objects.Shapes;

/// <summary>
/// A closed polygon with at least three points.
/// </summary>
/// <seealso cref="Imaging.Objects.Shapes.Shape" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.Shape&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.PolygonShape&gt;" />
public sealed record PolygonShape(ImmutableArray<PointD> Points) : Shape
{
    /// <inheritdoc />
    public override PixelRect GetBounds()
    {
        return PointBounds(Points);
    }

    /// <inheritdoc />
    public override Shape Translate(double dx, double dy)
    {
        return this with { Points = MovePoints(Points, dx, dy) };
    }

    /// <inheritdoc />
    public override bool IsValid()
    {
        return IsStyleValid() && ArePointsValid(Points, 3);
    }
}