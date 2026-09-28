/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        BlendMode.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
///     How a layer is combined with the ones below. Only <see cref="Normal" /> exists so far;
///     the value is stored in files already so more modes can be added without a format change.
/// </summary>
public enum BlendMode
{
    /// <summary>Straight-alpha "source over".</summary>
    Normal = 0
}