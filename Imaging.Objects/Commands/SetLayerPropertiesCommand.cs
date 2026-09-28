/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        Commands.cs
 * PURPOSE:     Change Layer Properties Command.
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
        /// <summary>
        /// The after
        /// </summary>
        private readonly LayerProperties _after;

        /// <summary>
        /// The layer identifier
        /// </summary>
        private readonly Guid _layerId;

        /// <summary>
        /// The before
        /// </summary>
        private LayerProperties? _before;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetLayerPropertiesCommand" /> class.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="after">The after.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
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