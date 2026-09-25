/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        AddShapeCommand.cs
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
    /// Adds a shape to a shape layer (on top unless an index is given).
    /// </summary>
    /// <seealso cref="IDocumentCommand" />
    public sealed class AddShapeCommand : IDocumentCommand
    {
        private readonly int? _index;

        private readonly Guid _layerId;

        private readonly Shape _shape;

        /// <summary>Initializes a new instance of the <see cref="AddShapeCommand" /> class.</summary>
        public AddShapeCommand(Guid layerId, Shape shape, int? index = null)
        {
            ArgumentNullException.ThrowIfNull(shape);
            _layerId = layerId;
            _shape = shape;
            _index = index;
        }

        /// <inheritdoc />
        public string Description => "Add shape";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            var layer = document.GetLayer<ShapeLayer>(_layerId);
            layer.Insert(_index ?? layer.Shapes.Count, _shape);
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _shape.GetBounds());
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            var layer = document.GetLayer<ShapeLayer>(_layerId);
            layer.RemoveAt(layer.IndexOf(_shape.Id));
            document.Raise(DocumentChangeKind.ShapesChanged, _layerId, _shape.GetBounds());
        }
    }
}
