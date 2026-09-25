/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        Commands.cs
 * PURPOSE:     Undoable edits of a document. A command stores just enough to apply and revert itself:
 *              ids and small records for structure/shape edits, tile patches for pixel edits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Commands
{
    /// <summary>Moves a layer in the stack.</summary>
    public sealed class MoveLayerCommand : IDocumentCommand
    {
        private readonly Guid _layerId;

        private readonly int _newIndex;

        private int _oldIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveLayerCommand" /> class.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="newIndex">The new index.</param>
        public MoveLayerCommand(Guid layerId, int newIndex)
        {
            _layerId = layerId;
            _newIndex = newIndex;
        }

        /// <inheritdoc />
        public string Description => "Move layer";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            _oldIndex = document.IndexOf(_layerId);
            document.MoveLayer(_layerId, _newIndex);
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            document.MoveLayer(_layerId, _oldIndex);
        }
    }
}
