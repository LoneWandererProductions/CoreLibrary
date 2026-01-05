/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderResource.cs
 * PURPOSE:     String Resources for rendering operations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine;

/// <summary>
///     Mostly static string constants.
/// </summary>
internal static class RenderResource
{
    /// <summary>
    /// The error open gl
    /// </summary>
    internal const string ErrorOpenGl = "OpenGL 4.5 or higher required.";

    /// <summary>
    /// The error input buffer
    /// </summary>
    internal const string ErrorInputBuffer = "Input buffer size does not match.";

    /// <summary>
    /// The error layer size
    /// </summary>
    internal const string ErrorLayerSize = "Layer size does not match container size.";

    /// <summary>
    /// The error invalid layer index
    /// </summary>
    internal const string ErrorInvalidLayerIndex = "Invalid layer index {0}";

    /// <summary>
    /// The error no layers
    /// </summary>
    internal const string ErrorNoLayers = "No layers to composite.";

    /// <summary>
    /// The shader skybox vertex
    /// </summary>
    internal const string ShaderSkyboxVertex = "skybox_vertex.glsl";

    /// <summary>
    /// The shader skybox fragment
    /// </summary>
    internal const string ShaderSkyboxFragment = "skybox_fragment.glsl";

    /// <summary>
    /// The error layer size mismatch
    /// </summary>
    internal const string ErrorLayerSizeMismatch = "Layer size mismatch.";
}