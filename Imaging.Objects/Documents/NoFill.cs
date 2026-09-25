/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        NoFill.cs
 * PURPOSE:     The no Fill record.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
/// No fill.
/// </summary>
/// <seealso cref="Imaging.Objects.Documents.FillSpec" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.FillSpec&gt;" />
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.NoFill&gt;" />
public sealed record NoFill : FillSpec;
