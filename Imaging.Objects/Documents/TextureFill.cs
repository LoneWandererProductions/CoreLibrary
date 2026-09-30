/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        TextureFill.cs
 * PURPOSE:     The Texture Fill record.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
/// A named texture of the imaging pipeline.
/// </summary>
/// <seealso cref="Imaging.Objects.Documents.FillSpec" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.FillSpec&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.TextureFill&gt;" />
public sealed record TextureFill(string Name) : FillSpec;
