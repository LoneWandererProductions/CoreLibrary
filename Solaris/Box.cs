/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Box.cs
 * PURPOSE:     Helper Object to handle the drawing and processing Process
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;

namespace Solaris;

/// <summary>
///     Contains the Image Tile information
/// </summary>
internal sealed class Box
{
    /// <summary>
    ///     Gets the image.
    /// </summary>
    /// <value>
    ///     The image.
    /// </value>
    internal Bitmap? Image { get; init; }

    /// <summary>
    ///     Gets the x.
    /// </summary>
    /// <value>
    ///     The x.
    /// </value>
    internal int X { get; init; }

    /// <summary>
    ///     Gets the y.
    /// </summary>
    /// <value>
    ///     The y.
    /// </value>
    internal int Y { get; init; }

    /// <summary>
    ///     Gets the layer.
    /// </summary>
    /// <value>
    ///     The layer.
    /// </value>
    internal int Layer { get; init; }

    /// <summary>
    ///     Converts to string.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return string.Concat(Resources.StrX, X, Resources.StrY, Y, Resources.StrLayer, Layer);
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Layer);
    }
}
