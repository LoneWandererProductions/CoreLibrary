/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Common.Images
* FILE:        ImageEventArgs.cs
* PURPOSE:     Image Event Detail Information can be further extended in the future
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    ///     We need the Id of the clicked Image
    /// </summary>
    public sealed class ImageEventArgs : EventArgs
    {
        /// <summary>
        ///     The tile id.
        /// </summary>
        public int Id { get; internal init; }

        /// <summary>
        /// The Identifier of the Thumbnails control instance (e.g., "Group_0").
        /// </summary>
        public string SenderTag { get; internal init; }
    }
}
