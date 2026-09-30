/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        DocumentChangedEventArgs.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
///     Change notification. <see cref="Region" /> is the affected area in document coordinates, so a view can
///     redraw only that (structural changes report the whole document).
/// </summary>
public sealed class DocumentChangedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="DocumentChangedEventArgs" /> class.</summary>
    public DocumentChangedEventArgs(DocumentChangeKind kind, Guid layerId, PixelRect region)
    {
        Kind = kind;
        LayerId = layerId;
        Region = region;
    }

    /// <summary>Gets the kind of change.</summary>
    public DocumentChangeKind Kind { get; }

    /// <summary>Gets the layer concerned.</summary>
    public Guid LayerId { get; }

    /// <summary>Gets the affected region.</summary>
    public PixelRect Region { get; }
}
