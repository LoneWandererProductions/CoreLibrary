/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Aurora.cs
 * PURPOSE:     Texture object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Diagnostics;

namespace Solaris;

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
}
