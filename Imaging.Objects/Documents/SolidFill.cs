/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        SolidFill.cs
 * PURPOSE:     The Solid Fill record.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
/// A solid colour, straight 0xAARRGGBB.
/// </summary>
/// <seealso cref="Imaging.Objects.Documents.FillSpec" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.FillSpec&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.SolidFill&gt;" />
public sealed record SolidFill(uint Argb) : FillSpec;