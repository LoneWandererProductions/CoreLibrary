/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        FilterFill.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>A named filter of the imaging pipeline, applied to what is below the shape.</summary>
public sealed record FilterFill(string Name) : FillSpec;