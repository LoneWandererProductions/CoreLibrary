/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        FillSpec.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text.Json.Serialization;

namespace Imaging.Objects.Documents;

/// <summary>
///     How the inside of a closed shape is filled. Texture and filter fills are data too: they name an
///     effect of the imaging pipeline instead of baking its result into pixels, so the region stays editable.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(NoFill), "none")]
[JsonDerivedType(typeof(SolidFill), "solid")]
[JsonDerivedType(typeof(TextureFill), "texture")]
[JsonDerivedType(typeof(FilterFill), "filter")]
public abstract record FillSpec
{
    /// <summary>Gets the shared "no fill" instance.</summary>
    public static FillSpec None { get; } = new NoFill();
}
