/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        DocumentChangeKind.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>What happened to a document.</summary>
public enum DocumentChangeKind
{
    /// <summary>A layer was inserted.</summary>
    LayerAdded,

    /// <summary>A layer was removed.</summary>
    LayerRemoved,

    /// <summary>The stacking order changed.</summary>
    LayerMoved,

    /// <summary>Name, visibility, opacity or blend mode changed.</summary>
    LayerPropertiesChanged,

    /// <summary>Shapes were added, removed or replaced.</summary>
    ShapesChanged,

    /// <summary>Pixels of a raster layer changed.</summary>
    PixelsChanged
}