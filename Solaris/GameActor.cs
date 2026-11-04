/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        GameActor.cs
 * PURPOSE:     The Units on the map.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using Imaging.Objects;

namespace Solaris
{
    /// <summary>
    /// Represents a dynamic entity on the game field (avatar, opponent, NPC).
    /// </summary>
    public sealed class GameActor : IDisposable
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; init; }

        /// <summary>
        /// Gets or sets the index of the tile.
        /// </summary>
        /// <value>
        /// The index of the tile.
        /// </value>
        public int TileIndex { get; set; }

        /// <summary>
        /// Gets or sets the sprite.
        /// </summary>
        /// <value>
        /// The sprite.
        /// </value>
        public Bitmap? Sprite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is standing sprite.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is standing sprite; otherwise, <c>false</c>.
        /// </value>
        public bool IsStandingSprite { get; set; } = true;

        /// <summary>
        /// Gets or sets the layer.
        /// </summary>
        /// <value>
        /// The layer.
        /// </value>
        public int Layer { get; set; } = 10;

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        internal UnmanagedImageBuffer? Buffer { get; private set; }

        /// <summary>
        /// Updates the buffer.
        /// </summary>
        public void UpdateBuffer()
        {
            Buffer?.Dispose();
            Buffer = Sprite != null ? UnmanagedImageBuffer.FromBitmap(Sprite) : null;
        }


        //<inheritdoc />
        public void Dispose()
        {
            Buffer?.Dispose();
            Buffer = null;
        }
    }
}
