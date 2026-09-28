/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Shapes
 * FILE:        Shape.cs
 * PURPOSE:     Base Shape Object.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Imaging.Objects.Shapes;

/// <summary>
/// Base of all shapes. Shapes are immutable: "moving" one produces a new record with the same
/// <see cref="Id" />, which is exactly what an undoable replace command needs.
/// </summary>
/// <seealso cref="IEquatable&lt;Shape&gt;" />
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineShape), "line")]
[JsonDerivedType(typeof(RectShape), "rect")]
[JsonDerivedType(typeof(EllipseShape), "ellipse")]
[JsonDerivedType(typeof(PolygonShape), "polygon")]
[JsonDerivedType(typeof(PolylineShape), "polyline")]
public abstract record Shape
{
    /// <summary>
    /// Gets the identity of the shape inside its layer.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the outline.
    /// </summary>
    /// <value>
    /// The stroke.
    /// </value>
    public StrokeSpec Stroke { get; init; } = StrokeSpec.Default;

    /// <summary>
    /// Gets the fill. Ignored by open shapes (line, polyline).
    /// </summary>
    /// <value>
    /// The fill.
    /// </value>
    public FillSpec Fill { get; init; } = FillSpec.None;

    /// <summary>
    /// Gets the pixels this shape can touch, including half the stroke width and one pixel of
    /// anti-aliasing. This is the dirty region when the shape is added, moved or removed.
    /// </summary>
    /// <returns>Rectangle Pixel <seealso cref="IEquatable&lt;PixelRect&gt;"</returns>
    public abstract PixelRect GetBounds();

    /// <summary>
    /// Returns a copy moved by the given offset.
    /// </summary>
    /// <param name="dx">The dx.</param>
    /// <param name="dy">The dy.</param>
    /// <returns>Shape Form.</returns>
    public abstract Shape Translate(double dx, double dy);

    /// <summary>
    /// Checks that all numbers are finite and the shape has enough points to be drawn.
    /// The loader rejects files that contain invalid shapes.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool IsValid();

    /// <summary>Validation shared by all shapes.</summary>
    protected bool IsStyleValid()
    {
        return Stroke is { } && Fill is { } && double.IsFinite(Stroke.Width) && Stroke.Width >= 0;
    }

    /// <summary>Turns a floating point bounding box into stroke-padded pixel bounds.</summary>
    protected PixelRect BoundsOf(double minX, double minY, double maxX, double maxY)
    {
        var width = Stroke is null ? 0d : Math.Max(0d, Stroke.Width);
        var pad = (int)Math.Ceiling(width / 2d) + 1;

        return PixelRect.FromLtrb(
            ToInt(Math.Floor(minX)) - pad,
            ToInt(Math.Floor(minY)) - pad,
            ToInt(Math.Ceiling(maxX)) + pad,
            ToInt(Math.Ceiling(maxY)) + pad);
    }

    private static int ToInt(double value)
    {
        return (int)Math.Clamp(value, -1_000_000_000d, 1_000_000_000d);
    }

    /// <summary>Stroke-padded bounds of a point list.</summary>
    protected PixelRect PointBounds(ImmutableArray<PointD> points)
    {
        if (points.IsDefaultOrEmpty) return PixelRect.Empty;

        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
        foreach (var point in points)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        return BoundsOf(minX, minY, maxX, maxY);
    }

    /// <summary>Moves every point by the given offset.</summary>
    protected static ImmutableArray<PointD> MovePoints(ImmutableArray<PointD> points, double dx, double dy)
    {
        if (points.IsDefaultOrEmpty) return points;

        var builder = ImmutableArray.CreateBuilder<PointD>(points.Length);
        foreach (var point in points)
        {
            builder.Add(new PointD(point.X + dx, point.Y + dy));
        }

        return builder.MoveToImmutable();
    }

    /// <summary>True if there are at least <paramref name="minimum" /> points and all are finite.</summary>
    protected static bool ArePointsValid(ImmutableArray<PointD> points, int minimum)
    {
        if (points.IsDefault || points.Length < minimum) return false;

        foreach (var point in points)
        {
            if (!double.IsFinite(point.X) || !double.IsFinite(point.Y)) return false;
        }

        return true;
    }
}