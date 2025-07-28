/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveContainer.cs
 * PURPOSE:     Container that holds some basic Image Informations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace LightVector;

/// <summary>
///     Save Container
/// </summary>
[XmlRoot(ElementName = "Element")]
public sealed class SaveContainer
{
    /// <summary>
    ///     Gets or sets the objects.
    /// </summary>
    /// <value>
    ///     The objects.
    /// </value>
    public List<SaveObject> Objects { get; init; } = new();

    /// <summary>
    ///     Gets or sets the width.
    ///     For now unused
    /// </summary>
    /// <value>
    ///     The width.
    /// </value>
    public int Width { get; init; }

    /// <summary>
    ///     Gets the height.
    ///     For now unused
    /// </summary>
    /// <value>
    ///     The height.
    /// </value>
    public int Height { get; init; }
}
