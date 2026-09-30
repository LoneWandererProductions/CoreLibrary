/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        ShapeLayer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Shapes;

namespace Imaging.Objects.Documents;

/// <summary>
///     A layer of geometry. Nothing is rasterized until a renderer asks for it. Shapes are drawn in list
///     order, the last one on top.
/// </summary>
public sealed class ShapeLayer : Layer
{
    private readonly List<Shape> _shapes = new();

    /// <summary>Creates an empty shape layer.</summary>
    public ShapeLayer(string name = "Shapes")
        : base(Guid.NewGuid(), name)
    {
    }

    /// <summary>Creates an empty shape layer with a known id (used by the loader).</summary>
    public ShapeLayer(Guid id, string? name)
        : base(id, name)
    {
    }

    /// <summary>Gets the shapes, bottom to top.</summary>
    public IReadOnlyList<Shape> Shapes => _shapes;

    /// <summary>Gets the index of the shape with the given id, or -1.</summary>
    public int IndexOf(Guid shapeId)
    {
        for (var i = 0; i < _shapes.Count; i++)
        {
            if (_shapes[i].Id == shapeId) return i;
        }

        return -1;
    }

    /// <summary>Gets the shape with the given id, or null.</summary>
    public Shape? Find(Guid shapeId)
    {
        var index = IndexOf(shapeId);
        return index < 0 ? null : _shapes[index];
    }

    /// <summary>Inserts a shape. Ids must be unique inside the layer.</summary>
    public void Insert(int index, Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _shapes.Count);

        if (IndexOf(shape.Id) >= 0)
            throw new ArgumentException(string.Format(ImageResource.ErrorShapeExists, shape.Id));

        _shapes.Insert(index, shape);
    }

    /// <summary>Removes and returns the shape at <paramref name="index" />.</summary>
    public Shape RemoveAt(int index)
    {
        var shape = _shapes[index];
        _shapes.RemoveAt(index);
        return shape;
    }

    /// <summary>Replaces the shape at <paramref name="index" /> (same id required) and returns the old one.</summary>
    public Shape Replace(int index, Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        var old = _shapes[index];
        if (old.Id != shape.Id) throw new ArgumentException(ImageResource.ErrorShapeIdMismatch);

        _shapes[index] = shape;
        return old;
    }

    /// <inheritdoc />
    public override Layer Duplicate(string? name = null)
    {
        var copy = new ShapeLayer(Guid.NewGuid(), name ?? $"{Name} copy");
        CopySettingsTo(copy);

        // Shapes are immutable, so sharing the records is safe.
        copy._shapes.AddRange(_shapes);
        return copy;
    }
}
