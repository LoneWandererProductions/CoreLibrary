/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Ui2DShaders.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine
{
    /// <summary>
    /// Provides GLSL shader sources for the 2D renderer.
    /// </summary>
    internal static class Ui2DShaders
    {
        /// <summary>
        /// Vertex shader for 2D solid color quads/lines.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="flipY">If true, flips Y to match OpenGL bottom-left origin.
        /// If false, keeps Y increasing downward like software renderer.</param>
        /// <returns></returns>
        public static string ColorVertex(int width, int height, bool flipY = true) => $@"
#version 410 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec4 aColor;
out vec4 vColor;
void main()
{{
    vec2 pos = aPos / vec2({width},{height}) * 2.0 - 1.0;
    {(flipY ? "pos.y = -pos.y;" : "")}
    gl_Position = vec4(pos, 0.0, 1.0);
    vColor = aColor;
}}";

        /// <summary>
        /// Fragment shader for 2D solid color quads/lines.
        /// </summary>
        public static string ColorFragment() => @"
#version 410 core
in vec4 vColor;
out vec4 FragColor;
void main()
{
    FragColor = vColor;
}";

        /// <summary>
        /// Vertex shader for textured quads.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="flipY">If true, flips Y and optionally V coordinate to match OpenGL default.</param>
        /// <returns></returns>
        public static string TextureVertex(int width, int height, bool flipY = true) => $@"
#version 410 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTex;
out vec2 vTex;
void main()
{{
    vec2 pos = aPos / vec2({width},{height}) * 2.0 - 1.0;
    {(flipY ? "pos.y = -pos.y;" : "")}
    gl_Position = vec4(pos, 0.0, 1.0);
    vTex = {(flipY ? "vec2(aTex.x, 1.0 - aTex.y)" : "aTex")};
}}";

        /// <summary>
        /// Fragment shader for textured quads.
        /// </summary>
        public static string TextureFragment() => @"
#version 410 core
in vec2 vTex;
uniform sampler2D uTexture;
out vec4 FragColor;
void main()
{
    FragColor = texture(uTexture, vTex);
}";
    }
}