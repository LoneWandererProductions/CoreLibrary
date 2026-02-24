/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector.Enums
 * FILE:        VectorLineJoin.cs
 * PURPOSE:     A platform-agnostic enum. The Renderer maps this to PenLineJoin.Bevel, etc.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace LightVector.Enums
{

    /// <summary>
    /// A platform-agnostic enum for line join styles. The Renderer will map this to the appropriate PenLineJoin value.
    /// </summary>
    public enum VectorLineJoin
    {
        Bevel,
        Miter,
        Round
    }
}
