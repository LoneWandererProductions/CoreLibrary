/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        ShaderResources.cs
 * PURPOSE:     Centralized storage of all shader programs for rendering effects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine
{
    /// <summary>
    /// Provides centralized shader program sources for various rendering effects.
    /// Avoids duplication by reusing vertex and fragment shaders where possible.
    /// </summary>
    public static class ShaderResource
    {
        // -----------------------------
        // Skybox Shader
        // -----------------------------
        /// <summary>
        /// Vertex shader for rendering a skybox.
        /// Outputs vertex positions transformed by view and projection matrices.
        /// </summary>
        public const string SkyboxVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
out vec3 vTexCoord;
uniform mat4 view;
uniform mat4 projection;
void main() {
    vTexCoord = aPosition;
    gl_Position = projection * view * vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Fragment shader for rendering a skybox.
        /// Samples from a cubemap texture using the interpolated vertex coordinates.
        /// </summary>
        public const string SkyboxFragmentShader = @"#version 450 core
in vec3 vTexCoord;
out vec4 FragColor;
uniform samplerCube uSkybox;
void main() {
    FragColor = texture(uSkybox, vTexCoord);
}";

        /// <summary>
        /// Array of vertex positions for a cube used by the skybox shader.
        /// </summary>
        public static readonly float[] SkyboxVertices = new float[]
        {
            // positions for a cube
            -1f, 1f, -1f, -1f, -1f, -1f, 1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f, -1f, -1f, 1f, -1f, -1f, -1f, 1f, -1f, -1f,
            -1f, -1f, 1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f, -1f, -1f, 1f, 1f, -1f, -1f, 1f, -1f, 1f, 1f, 1f, 1f, 1f, 1f,
            1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f, 1f, -1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, -1f, 1f, -1f, -1f, 1f,
            -1f, 1f, -1f, 1f, 1f, -1f, 1f, 1f, 1f, 1f, 1f, 1f, -1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f,
            1f, -1f, -1f, 1f, -1f, -1f, -1f, -1f, 1f, 1f, -1f, 1f
        };

        // -----------------------------
        // Solid Color Shader
        // -----------------------------
        /// <summary>
        /// Vertex shader for solid color rendering.
        /// Outputs vertex positions directly.
        /// </summary>
        public const string SolidColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
void main() { gl_Position = vec4(aPosition, 1.0); }";

        /// <summary>
        /// Fragment shader for solid color rendering.
        /// Outputs the uniform color.
        /// </summary>
        public const string SolidColorFragmentShader = @"#version 450 core
uniform vec4 uColor;
out vec4 FragColor;
void main() { FragColor = uColor; }";

        // -----------------------------
        // Texture Mapping Shader
        // -----------------------------
        /// <summary>
        /// Vertex shader for texture mapping.
        /// Passes texture coordinates to the fragment shader.
        /// </summary>
        public const string TextureMappingVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
void main() {
    gl_Position = vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
}";

        /// <summary>
        /// Fragment shader for texture mapping.
        /// Samples from a 2D texture.
        /// </summary>
        public const string TextureMappingFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
void main() { FragColor = texture(uTexture, vTexCoord); }";

        // -----------------------------
        // Vertex Color Shader
        // -----------------------------
        /// <summary>
        /// Vertex shader for vertex color rendering.
        /// Passes per-vertex color to the fragment shader.
        /// </summary>
        public const string VertexColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;
out vec3 vColor;
void main() {
    gl_Position = vec4(aPosition, 1.0);
    vColor = aColor;
}";

        /// <summary>
        /// Fragment shader for vertex color rendering.
        /// Outputs the interpolated vertex color.
        /// </summary>
        public const string VertexColorFragmentShader = @"#version 450 core
in vec3 vColor;
out vec4 FragColor;
void main() { FragColor = vec4(vColor, 1.0); }";

        // -----------------------------
        // Gradient Shader (uses VertexColor shaders)
        // -----------------------------
        public const string GradientVertexShader = VertexColorVertexShader;
        public const string GradientFragmentShader = VertexColorFragmentShader;

        // -----------------------------
        // Sprite Shader (uses TextureMapping shaders)
        // -----------------------------
        public const string SpriteVertexShader = TextureMappingVertexShader;
        public const string SpriteFragmentShader = TextureMappingFragmentShader;

        // -----------------------------
        // Wireframe Shader
        // -----------------------------
        public const string WireframeVertexShader = SolidColorVertexShader;

        public const string WireframeFragmentShader = @"#version 450 core
out vec4 FragColor;
void main() { FragColor = vec4(0.0, 1.0, 0.0, 1.0); }";

        // -----------------------------
        // Texture Array Tilemap Shader
        // -----------------------------
        /// <summary>
        /// Vertex shader for texture array tilemap rendering.
        /// Passes tile index and texture coordinates to the fragment shader.
        /// </summary>
        public const string TextureArrayTilemapVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in float aTileIndex;
out vec2 vTexCoord;
flat out int vTileIndex;
uniform mat4 view;
uniform mat4 projection;
void main() {
    gl_Position = projection * view * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
    vTileIndex = int(aTileIndex);
}";

        /// <summary>
        /// Fragment shader for texture array tilemap rendering.
        /// Samples from a 2D texture array.
        /// </summary>
        public const string TextureArrayTilemapFragmentShader = @"#version 450 core
in vec2 vTexCoord;
flat in int vTileIndex;
out vec4 FragColor;
uniform sampler2DArray uTextureArray;
void main() { FragColor = texture(uTextureArray, vec3(vTexCoord, vTileIndex)); }";

        // -----------------------------
        // General-purpose shaders
        // -----------------------------
        /// <summary>
        /// Basic fragment shader: outputs vertex color with alpha 1.
        /// Can be used for simple colored objects.
        /// </summary>
        public static string FragmentShaderSource => @"
#version 450 core
in vec3 vColor;
out vec4 FragColor;
void main()
{
    FragColor = vec4(vColor, 1.0);
}";

        /// <summary>
        /// Basic vertex shader: passes position and color from vertex to fragment shader.
        /// Can be used for simple colored objects.
        /// </summary>
        public static string VertexShaderSource => @"
#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;
out vec3 vColor;
void main()
{
    gl_Position = vec4(aPosition, 1.0);
    vColor = aColor;
}";
    }
}
