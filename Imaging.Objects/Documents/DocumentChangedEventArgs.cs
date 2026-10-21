/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        DocumentChangedEventArgs.cs
 * PURPOSE:     EVent to notify if Image whas changed, including the Type of Change and where.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Change notification. <see cref="Region" /> is the affected area in document coordinates, so a view can
    ///     redraw only that (structural changes report the whole document).
    /// </summary>
    public sealed class DocumentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentChangedEventArgs" /> class.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="region">The region.</param>
        public DocumentChangedEventArgs(DocumentChangeKind kind, Guid layerId, PixelRect region)
        {
            Kind = kind;
            LayerId = layerId;
            Region = region;
        }

        /// <summary>
        /// Gets the kind of change.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        public DocumentChangeKind Kind { get; }

        /// <summary>
        /// Gets the layer concerned.
        /// </summary>
        /// <value>
        /// The layer identifier.
        /// </value>
        public Guid LayerId { get; }

        /// <summary>
        /// Gets the affected region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public PixelRect Region { get; }
    }
}
