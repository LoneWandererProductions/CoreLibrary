/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        PolylineShape.cs
 * PURPOSE:     The Polyline Shape.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using System.Collections.Immutable;

namespace Imaging.Objects.Shapes;

/// <summary>
/// An open polyline with at least two points. Stroke only.
/// </summary>
/// <seealso cref="Imaging.Objects.Shapes.Shape" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.Shape&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Shapes.PolylineShape&gt;" />
public sealed record PolylineShape(ImmutableArray<PointD> Points) : Shape
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
        return IsStyleValid() && ArePointsValid(Points, 2);
    }
}
