/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        DirtyTracker.cs
 * PURPOSE:     Track the states of the different changes
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Solaris
{
    /// <summary>
    /// Manages and aggregates invalidation flags and modified tile indices for deferred targeted rendering.
    /// </summary>
    public class DirtyTracker
    {
        /// <summary>
        /// Gets the active bitwise dirty flags indicating which layers require repainting.
        /// </summary>
        /// <value>
        /// The current dirty flags state.
        /// </value>
        public DirtyFlags Flags { get; private set; } = DirtyFlags.None;

        /// <summary>
        /// Gets the collection of specific spatial tile indices queued for sub-region re-blitting.
        /// </summary>
        /// <value>
        /// The set of dirty tile indices.
        /// </value>
        public HashSet<int> DirtyTileIndices { get; } = new();

        /// <summary>
        /// Queues a specific tile index and sets the corresponding layer dirty flag.
        /// </summary>
        /// <param name="tileIndex">The 1D spatial tile index to invalidate.</param>
        /// <param name="layer">The target layer flag. Defaults to <see cref="DirtyFlags.TileMap"/>.</param>
        public void MarkTileDirty(int tileIndex, DirtyFlags layer = DirtyFlags.TileMap)
        {
            DirtyTileIndices.Add(tileIndex);
            Flags |= layer;
        }

        /// <summary>
        /// Sets a specific layer dirty flag for a full-layer redraw pass.
        /// </summary>
        /// <param name="layer">The layer flag to invalidate.</param>
        public void MarkLayerDirty(DirtyFlags layer)
        {
            Flags |= layer;
        }

        /// <summary>
        /// Resets all dirty flags and clears the queued tile indices.
        /// </summary>
        public void Clear()
        {
            DirtyTileIndices.Clear();
            Flags = DirtyFlags.None;
        }
    }
}
