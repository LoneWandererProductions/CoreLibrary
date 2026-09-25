/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        ReplaceShapeCommand.cs
 * PURPOSE:     Undoable edits of a document. A command stores just enough to apply and revert itself:
 *              ids and small records for structure/shape edits, tile patches for pixel edits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;

namespace Imaging.Objects.Commands
{
    /// <summary>
    ///     Replaces a shape by a changed copy with the same id: move, resize, restyle. The dirty region covers
    ///     the old and the new position.
    /// </summary>
    public sealed class ReplaceShapeCommand : IDocumentCommand
    {
        private readonly Guid _layerId;

        private readonly Shape _after;

        private Shape? _before;

        private int _index;

        /// <summary>Initializes a new instance of the <see cref="ReplaceShapeCommand" /> class.</summary>
        public ReplaceShapeCommand(Guid layerId, Shape after)
        {
            ArgumentNullException.ThrowIfNull(after);
            _layerId = layerId;
            _after = after;
        }

        /// <inheritdoc />
        public string Description => "Edit shape";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            var layer = document.GetLayer<ShapeLayer>(_layerId);
            _index = layer.IndexOf(_after.Id);
            if (_index < 0) throw new KeyNotFoundException(string.Format(ImageResource.ErrorShapeNotFound, _after.Id));

            _before = layer.Replace(_index, _after);
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _before.GetBounds().Union(_after.GetBounds()));
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            if (_before is null) return;

            document.GetLayer<ShapeLayer>(_layerId).Replace(_index, _before);
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _before.GetBounds().Union(_after.GetBounds()));
        }
    }
}
