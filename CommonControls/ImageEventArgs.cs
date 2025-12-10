/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonControls
* FILE:        CommonControls/ImageEventArgs.cs
* PURPOSE:     Image Event Detail Information can be further extended in the future
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;

namespace CommonControls;

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
}