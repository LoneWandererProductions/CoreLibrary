/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        UnmanagedTileBox.cs
 * PURPOSE:     Container for unmanaged tile sorting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using RenderEngine;

namespace Solaris
{
    /// <summary>
    /// Lightweight container for unmanaged tile sorting.
    /// </summary>
    public readonly struct UnmanagedTileBox
    {
        /// <summary>
        /// Gets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; init; }

        /// <summary>
        /// Gets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public int Y { get; init; }

        /// <summary>
        /// Gets the layer.
        /// </summary>
        /// <value>
        /// The layer.
        /// </value>
        public int Layer { get; init; }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public UnmanagedImageBuffer Buffer { get; init; }
    }
}
