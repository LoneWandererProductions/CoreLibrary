/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        UnmanagedTileBox.cs
 * PURPOSE:     Container for unmanaged tile sorting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects;

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

        /// <summary>
        /// Gets the tile x.
        /// </summary>
        /// <value>
        /// The tile x.
        /// </value>
        public int TileX { get; init; }

        /// <summary>
        /// Gets the tile y.
        /// </summary>
        /// <value>
        /// The tile y.
        /// </value>
        public int TileY { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is standing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is standing; otherwise, <c>false</c>.
        /// </value>
        public bool IsStanding { get; init; }

        /// <summary>
        /// Gets the anchor x.
        /// </summary>
        /// <value>
        /// The anchor x.
        /// </value>
        public int AnchorX { get; init; }

        /// <summary>
        /// Gets the anchor y.
        /// </summary>
        /// <value>
        /// The anchor y.
        /// </value>
        public int AnchorY { get; init; }
    }
}