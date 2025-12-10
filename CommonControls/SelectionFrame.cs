/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/SelectionFrame.cs
 * PURPOSE:     Selection Frame
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace CommonControls;

/// <summary>
///     The Selection Frame on the Image
/// </summary>
[DebuggerDisplay("X = {X}, Y = {Y}, Width = {Width}, Height = {Height}, Tool = {Tool}")]
public sealed class SelectionFrame
{
    /// <summary>
    ///     Gets or sets the x.
    /// </summary>
    /// <value>
    ///     The x.
    /// </value>
    public int X { get; init; }

    /// <summary>
    ///     Gets or sets the y.
    /// </summary>
    /// <value>
    ///     The y.
    /// </value>
    public int Y { get; internal init; }

    /// <summary>
    ///     Gets or sets the width.
    /// </summary>
    /// <value>
    ///     The width.
    /// </value>
    public int Width { get; internal init; }

    /// <summary>
    ///     Gets or sets the height.
    /// </summary>
    /// <value>
    ///     The height.
    /// </value>
    public int Height { get; internal init; }

    /// <summary>
    ///     Gets the tool.
    /// </summary>
    /// <value>
    ///     The tool.
    /// </value>
    public ImageZoomTools Tool { get; internal init; }
}