/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        Layers.cs
 * PURPOSE:     The layer types of a document: pixel layers and shape layers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Snapshot of the user-visible settings of a layer. Used by the undoable property command.
    /// </summary>
    public sealed record LayerProperties(string Name, bool Visible, double Opacity, BlendMode Blend);
}
