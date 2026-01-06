/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        ShaderTypeApp.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine
{
    /// <summary>
    /// Defined Shaders for application use.
    /// </summary>
    public enum ShaderTypeApp
    {
        SolidColor = 0,
        TexturedQuad = 1,
        VertexColor = 2,
        Wireframe = 3,
        TextureArrayTilemap = 4,
        SolidColor2D = 5,
        VertexColor2D = 6,
        TexturedQuad2D = 7,
        PhongLighting = 8, // reserved for future implementation
        Instancing = 9, // reserved for future implementation
        PostProcessing = 10, // reserved for future implementation

        // Placeholders
        WaterRipple = 11, // placeholder
        VolumetricFog = 12 // placeholde
    }
}
