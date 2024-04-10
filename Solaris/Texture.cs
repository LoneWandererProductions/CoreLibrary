/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Aurora.cs
 * PURPOSE:     Texture object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Solaris
{
    /// <summary>
    ///     Texture Container
    /// </summary>
    public sealed class Texture
    {
        /// <summary>
        ///     Gets the path.
        /// </summary>
        /// <value>
        ///     The path.
        /// </value>
        public string Path { get; init; }

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
    }
}