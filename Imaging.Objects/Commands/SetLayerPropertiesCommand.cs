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
    ///     Changes name, visibility, opacity and/or blend mode. For a slider, execute one command when the drag ends,
    ///     not one per mouse move.
    /// </summary>
    public sealed class SetLayerPropertiesCommand : IDocumentCommand
    {
        private readonly LayerProperties _after;

        private readonly Guid _layerId;

        private LayerProperties? _before;

        /// <summary>Initializes a new instance of the <see cref="SetLayerPropertiesCommand" /> class.</summary>
        public SetLayerPropertiesCommand(Guid layerId, LayerProperties after)
        {
            ArgumentNullException.ThrowIfNull(after);
            _layerId = layerId;
            _after = after;
        }

        /// <inheritdoc />
        public string Description => "Change layer";

        /// <inheritdoc />
        public void Apply(Document document)
        {
            var layer = document.GetLayer(_layerId);
            _before ??= layer.Properties;
            layer.Properties = _after;
            document.Raise(DocumentChangeKind.LayerPropertiesChanged, _layerId, document.Bounds);
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            if (_before is null) return;

            document.GetLayer(_layerId).Properties = _before;
            document.Raise(DocumentChangeKind.LayerPropertiesChanged, _layerId, document.Bounds);
        }
    }
}
