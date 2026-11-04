/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Aurora.cs
 * PURPOSE:     Texture object
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Diagnostics;

namespace Solaris
{
    /// <summary>
    ///     Texture Container
    /// </summary>
    [DebuggerDisplay("Path = {Path}, Layer = {Layer}, Id = {Id}")]
    public sealed class Texture
    {
        /// <summary>
        ///     Gets the path.
        /// </summary>
        /// <value>
        ///     The path.
        /// </value>
        public string? Path { get; init; }

        /// <summary>
        ///     Gets the layer.
        /// </summary>
        /// <value>
        ///     The layer.
        /// </value>
        public int Layer { get; init; }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public int Id { get; init; }

        /// <summary>
        /// When true, the sprite remains upright with its full height and alpha channels
        /// instead of being clipped into a horizontal isometric floor diamond.
        /// </summary>
        public bool IsStandingSprite { get; init; } = false;

        /// <summary>
        /// Gets the anchor x. (Standard: Width / 2).
        /// </summary>
        /// <value>
        /// The anchor x.
        /// </value>
        public int? AnchorX { get; init; }

        /// <summary>
        /// Vertical bottom anchor point in the buffer that aligns with the center of the tile (slice.Y).
        /// If set to 0 or null, Buffer.Height (bottom edge of the buffer) is used.
        /// </summary>
        /// <value>
        /// The anchor y.
        /// </value>
        public int? AnchorY { get; init; }

        /// <summary>
        /// Gets the height offset.
        /// </summary>
        /// <value>
        /// The height offset.
        /// </value>
        public int HeightOffset { get; init; } = 0;
    }
}
