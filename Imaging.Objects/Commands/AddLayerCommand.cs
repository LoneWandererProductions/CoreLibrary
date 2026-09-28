/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Commands;
 * FILE:        AddLayerCommand.cs
 * PURPOSE:     Add Layer command for the Layered Image Format.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Commands;

/// <summary>
/// Inserts a layer (at the top unless an index is given).
/// </summary>
/// <seealso cref="IDocumentCommand" />
/// <seealso cref="IDisposable" />
public sealed class AddLayerCommand : IDocumentCommand, IDisposable
{
    /// <summary>
    /// The index
    /// </summary>
    private readonly int? _index;

    /// <summary>
    /// The layer
    /// </summary>
    private readonly Layer _layer;

    /// <summary>
    /// The applied
    /// </summary>
    private bool _applied;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLayerCommand" /> class.
    /// </summary>
    /// <param name="layer">The layer.</param>
    /// <param name="index">The index.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public AddLayerCommand(Layer layer, int? index = null)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _layer = layer;
        _index = index;
    }

    /// <inheritdoc />
    public string Description => "Add layer";

    /// <inheritdoc />
    public void Apply(Document document)
    {
        document.InsertLayer(_index ?? document.Layers.Count, _layer);
        _applied = true;
    }

    /// <inheritdoc />
    public void Revert(Document document)
    {
        document.RemoveLayer(_layer.Id, out _);
        _applied = false;
    }

    /// <inheritdoc />
    /// <summary>
    /// Releases the layer if it is not part of a document (undone and discarded).
    /// </summary>
    public void Dispose()
    {
        if (!_applied) (_layer as IDisposable)?.Dispose();
    }
}