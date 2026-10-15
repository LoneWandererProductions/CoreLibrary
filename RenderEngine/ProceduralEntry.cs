/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        ProceduralEntry.cs
 * PURPOSE:     Internal record bundling catalog info and the generation delegate
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Texture;
using System;

namespace RenderEngine
{
    /// <summary>
    ///Internal record bundling catalog info and the generation delegate
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;RenderEngine.ProceduralEntry&gt;" />
    internal readonly record struct ProceduralEntry(
        string Name,
        string Description,
        Func<int, int, object, RawTextureBuffer?> Generator
    );
}