/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        RemoveShapeCommand.cs
 * PURPOSE:     Undoable edits of a document. A command stores just enough to apply and revert itself:
 *              ids and small records for structure/shape edits, tile patches for pixel edits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;

namespace Imaging.Objects.Commands
{
    /// <summary>Removes a shape from a shape layer.</summary>
    public sealed class RemoveShapeCommand : IDocumentCommand
    {
        private readonly Guid _layerId;

        private readonly Guid _shapeId;

        private int _index;

        private Shape? _removed;

        /// <summary>Initializes a new instance of the <see cref="RemoveShapeCommand" /> class.</summary>
        public RemoveShapeCommand(Guid layerId, Guid shapeId)
        {
            _layerId = layerId;
            _shapeId = shapeId;
        }

        /// <inheritdoc />
        public string Description => "Delete shape";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            var layer = document.GetLayer<ShapeLayer>(_layerId);
            _index = layer.IndexOf(_shapeId);
            if (_index < 0) throw new KeyNotFoundException(string.Format(ImageResource.ErrorShapeNotFound, _shapeId));

            _removed = layer.RemoveAt(_index);
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _removed.GetBounds());
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            if (_removed is null) return;

            document.GetLayer<ShapeLayer>(_layerId).Insert(_index, _removed);
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _removed.GetBounds());
        }
    }
}