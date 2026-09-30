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
    /// <summary>
    /// Removes a layer. The command keeps it alive so undo can put it back.
    /// </summary>
    /// <seealso cref="IDocumentCommand" />
    /// <seealso cref="IDisposable" />
    public sealed class RemoveLayerCommand : IDocumentCommand, IDisposable
    {
        private readonly Guid _layerId;

        private int _index;

        private Layer? _removed;

        /// <summary>Initializes a new instance of the <see cref="RemoveLayerCommand" /> class.</summary>
        public RemoveLayerCommand(Guid layerId)
        {
            _layerId = layerId;
        }

        /// <inheritdoc />
        public string Description => "Delete layer";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            _removed = document.RemoveLayer(_layerId, out _index);
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            if (_removed is null) return;

            document.InsertLayer(_index, _removed);
            _removed = null;
        }

        /// <summary>Releases the removed layer once it can no longer be restored.</summary>
        public void Dispose()
        {
            (_removed as IDisposable)?.Dispose();
            _removed = null;
        }
    }
}
