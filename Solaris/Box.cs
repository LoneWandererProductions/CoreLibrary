/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Box.cs
 * PURPOSE:     Helper Object to handle the drawing and processing Process
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;

namespace Solaris
{
    /// <summary>
    ///     Contains the Image Tile information
    /// </summary>
    internal sealed class Box
    {
        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        internal Bitmap Image { get; init; }

        /// <summary>
        /// Gets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        internal int X { get; init; }

        /// <summary>
        /// Gets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        internal int Y { get; init; }

        /// <summary>
        /// Gets the layer.
        /// </summary>
        /// <value>
        /// The layer.
        /// </value>
        internal int Layer { get; init; }
    }
}
