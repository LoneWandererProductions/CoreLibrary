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
    /// </summary>
    /// <remarks>
    /// All shaders are written in GLSL (version 450 core) and grouped by use case.
    /// This class serves as a single point of reference to avoid duplication and
    /// ensure consistency across modules using similar rendering logic.
    /// </remarks>
    public static class ShaderResource
    {
        /// <summary>
        /// The common transform
        /// </summary>
        public const string CommonTransform = @"
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
vec4 TransformVertex(vec3 pos) {
    return projection * view * model * vec4(pos, 1.0);
}";

        /// <summary>
        /// Vertex shader for rendering a cubemap-based skybox.
        /// </summary>
        /// <remarks>
        /// This shader transforms cube vertices using the provided <c>view</c> and
        /// <c>projection</c> matrices but ignores translation from the camera position
        /// (to simulate an infinitely distant environment). The vertex positions are
        /// passed to the fragment shader as direction vectors for cubemap sampling.
        /// </remarks>
        public const string SkyboxVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
out vec3 vTexCoord;
uniform mat4 view;
uniform mat4 projection;
void main() {
    vTexCoord = aPosition;
    mat4 viewNoTranslation = mat4(mat3(view));
    gl_Position = projection * viewNoTranslation * vec4(aPosition, 1.0);
}";


        /// <summary>
        /// Fragment shader for sampling the cubemap used in skybox rendering.
        /// </summary>
        /// <remarks>
        /// Expects a bound <c>samplerCube</c> uniform named <c>uSkybox</c>.
        /// The incoming <c>vTexCoord</c> is treated as a direction vector.
        /// </remarks>
        public const string SkyboxFragmentShader = @"#version 450 core
in vec3 vTexCoord;
out vec4 FragColor;
uniform samplerCube uSkybox;
void main() { FragColor = texture(uSkybox, vTexCoord); }";

        /// <summary>
        /// Defines cube vertex coordinates used to render the skybox.
        /// </summary>
        /// <remarks>
        /// These coordinates form a unit cube centered at the origin.
        /// The cube is rendered around the camera to create an infinite background effect.
        /// </remarks>
        public static readonly float[] SkyboxVertices = new float[]
        {
            -1f, 1f, -1f, -1f, -1f, -1f, 1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f, -1f, -1f, 1f, -1f, -1f, -1f, 1f, -1f,
            -1f, 1f, -1f, -1f, 1f, 1f, -1f, 1f, 1f, -1f, 1f, -1f, 1f, 1f, 1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f,
            1f, -1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f, -1f, 1f, 1f,
            -1f, -1f, -1f, -1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f, -1f, 1f, 1f, -1f, -1f
        };

        /// <summary>
        /// Vertex shader for rendering solid-colored primitives.
        /// </summary>
        /// <remarks>
        /// Passes vertex positions directly to clip space without additional transforms.
        /// Intended for simple shapes or debugging geometry.
        /// </remarks>
        public const string SolidColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() { gl_Position = projection * view * model * vec4(aPosition, 1.0); }";

        /// <summary>
        /// Fragment shader for solid color rendering.
        /// </summary>
        /// <remarks>
        /// Uses a uniform <c>vec4 uColor</c> for consistent flat coloring across fragments.
        /// </remarks>
        public const string SolidColorFragmentShader = @"#version 450 core
uniform vec4 uColor;
out vec4 FragColor;
void main() { FragColor = uColor; }";

        /// <summary>
        /// Vertex shader for basic texture mapping.
        /// </summary>
        /// <remarks>
        /// Passes through vertex texture coordinates.
        /// Can be extended with model/view/projection matrices for 3D textured meshes.
        /// </remarks>
        public const string TextureMappingVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
}";

        /// <summary>
        /// Fragment shader for texture mapping.
        /// </summary>
        /// <remarks>
        /// Requires <c>uniform sampler2D uTexture</c> to be bound.
        /// </remarks>
        public const string TextureMappingFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
void main() { FragColor = texture(uTexture, vTexCoord); }";

        /// <summary>
        /// Vertex shader for per-vertex coloring.
        /// </summary>
        /// <remarks>
        /// Useful for gradient-based meshes or colored wireframes.
        /// Passes vertex color to the fragment shader for interpolation.
        /// </remarks>
        public const string VertexColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;
out vec3 vColor;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    vColor = aColor;
}";

        /// <summary>
        /// Fragment shader for vertex color interpolation.
        /// </summary>
        /// <remarks>
        /// Produces a smooth color blend between vertex-defined colors.
        /// </remarks>
        public const string VertexColorFragmentShader = @"#version 450 core
in vec3 vColor;
out vec4 FragColor;
void main() { FragColor = vec4(vColor, 1.0); }";

        /// <summary>
        /// Wireframe vertex shader with optional matrix transforms.
        /// </summary>
        public const string WireframeVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Fragment shader for wireframe rendering.
        /// </summary>
        /// <remarks>
        /// Outputs a solid green color; useful for overlaying geometry outlines.
        /// </remarks>
        public const string WireframeFragmentShader = @"#version 450 core
out vec4 FragColor;
void main() { FragColor = vec4(0.0, 1.0, 0.0, 1.0); }";

        /// <summary>
        /// Wireframe vertex shader that ignores model/view/projection matrices.
        /// Input coordinates must be in clip space (-1 to 1).
        /// </summary>
        public const string WireframeVertexShaderPassThrough = @"#version 450 core
layout(location = 0) in vec3 aPosition;
void main() {
    gl_Position = vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Wireframe vertex shader that ignores model/view/projection matrices.
        /// Input coordinates must be in clip space (-1 to 1).
        /// With color parameter.
        /// </summary>
        public const string WireframeFragmentShaderPassThroughUniform = @"#version 450 core
out vec4 FragColor;
uniform vec4 uColor; // set at runtime
void main() {
    FragColor = uColor;
}";

        /// <summary>
        /// Wireframe fragment shader for pass-through vertex shader.
        /// Outputs solid green by default, can be modified to use a uniform color.
        /// </summary>
        public const string WireframeFragmentShaderPassThrough = @"#version 450 core
out vec4 FragColor;
void main() {
    FragColor = vec4(0.0, 1.0, 0.0, 1.0);
}";


        /// <summary>
        /// Vertex shader for rendering tilemaps using 2D texture arrays.
        /// </summary>
        /// <remarks>
        /// Each vertex provides a <c>float aTileIndex</c> specifying the layer to sample.
        /// View and projection matrices transform vertex positions into clip space.
        /// </remarks>
        public const string TextureArrayTilemapVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in int aTileIndex;
out vec2 vTexCoord;
flat out int vTileIndex;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
    vTileIndex = aTileIndex;
}";

        /// <summary>
        /// Fragment shader for sampling from a 2D texture array.
        /// </summary>
        /// <remarks>
        /// Requires a <c>sampler2DArray</c> uniform named <c>uTextureArray</c>.
        /// The <c>vTileIndex</c> determines which layer of the array to use.
        /// </remarks>
        public const string TextureArrayTilemapFragmentShader = @"#version 450 core
in vec2 vTexCoord;
flat in int vTileIndex;
out vec4 FragColor;
uniform sampler2DArray uTextureArray;
void main() { FragColor = texture(uTextureArray, vec3(vTexCoord, vTileIndex)); }";

        /// <summary>
        /// General-purpose vertex shader for basic colored geometry.
        /// </summary>
        /// <remarks>
        /// A minimal shader that simply passes position and color to the fragment stage.
        /// Suitable for debugging or simple unlit objects.
        /// </remarks>
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

        /// <summary>
        /// General-purpose fragment shader for colored geometry.
        /// </summary>
        /// <remarks>
        /// Accepts interpolated vertex colors and outputs them with full opacity.
        /// </remarks>
        public static string FragmentShaderSource => @"
#version 450 core
in vec3 vColor;
out vec4 FragColor;
void main()
{
    FragColor = vec4(vColor, 1.0);
}";

        /// <summary>
        /// Vertex shader for solid 2D rendering.
        /// Coordinates must already be in clip space.
        /// </summary>
        /// <remarks>
        /// Converts pixel-space coordinates into normalized device coordinates
        /// based on the <c>uViewport</c> (screen width and height).
        /// Y-axis is inverted to match standard screen coordinate systems.
        /// </remarks>
        public const string SolidColor2DVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
uniform vec2 uViewport;
void main() {
    vec2 pos = aPos / uViewport * 2.0 - 1.0;
    pos.y = -pos.y;
    gl_Position = vec4(pos, 0.0, 1.0);
}";

        /// <summary>
        /// Fragment shader for solid 2D rendering.
        /// </summary>
        /// <remarks>
        /// Uses a uniform RGBA color for all fragments.
        /// </remarks>
        public const string SolidColor2DFragmentShader = @"#version 450 core
uniform vec4 uColor;
out vec4 FragColor;
void main() { FragColor = uColor; }";

        /// <summary>
        /// Vertex shader for 2D geometry with per-vertex color attributes.
        /// Coordinates must already be in clip space.
        /// </summary>
        /// <remarks>
        /// Handles individual vertex colors and transforms coordinates
        /// using the <c>uViewport</c> parameter.
        /// </remarks>
        public const string VertexColor2DVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec4 aColor;
out vec4 vColor;
uniform vec2 uViewport;
void main() {
    vec2 pos = aPos / uViewport * 2.0 - 1.0;
    pos.y = -pos.y;
    gl_Position = vec4(pos, 0.0, 1.0);
    vColor = aColor;
}";

        /// <summary>
        /// Fragment shader for vertex-colored 2D primitives.
        /// Coordinates must already be in clip space.
        /// </summary>
        /// <remarks>
        /// Simply outputs the interpolated color from the vertex stage.
        /// </remarks>
        public const string VertexColor2DFragmentShader = @"#version 450 core
in vec4 vColor;
out vec4 FragColor;
void main() {
    FragColor = vColor;
}";

        /// <summary>
        /// Vertex shader for textured 2D quads.
        /// </summary>
        /// <remarks>
        /// Converts vertex coordinates based on <c>uViewport</c> and
        /// forwards UV coordinates for sampling in the fragment stage.
        /// </remarks>
        public const string TexturedQuad2DVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTex;
out vec2 vTex;
uniform vec2 uViewport;
void main() {
    vec2 pos = aPos / uViewport * 2.0 - 1.0;
    pos.y = -pos.y;
    gl_Position = vec4(pos, 0.0, 1.0);
    vTex = aTex;
}";

        /// <summary>
        /// Fragment shader for textured 2D quads.
        /// </summary>
        /// <remarks>
        /// Expects <c>sampler2D uTexture</c> bound and uses interpolated UVs to sample color.
        /// Commonly used for sprites and UI elements.
        /// </remarks>
        public const string TexturedQuad2DFragmentShader = @"#version 450 core
in vec2 vTex;
out vec4 FragColor;
uniform sampler2D uTexture;
void main() {
    FragColor = texture(uTexture, vTex);
}";

        /// <summary>
        /// Vertex shader for Phong lighting.
        /// Outputs position, normal, and color information for per-fragment lighting calculations.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string PhongLightingVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec3 aColor;
out vec3 vNormal;
out vec3 vFragPos;
out vec3 vColor;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() {
    vec4 worldPos = model * vec4(aPosition, 1.0);
    vFragPos = worldPos.xyz;
    vNormal = normalize(mat3(transpose(inverse(model))) * aNormal);
    vColor = aColor;
    gl_Position = projection * view * worldPos;
}";

        /// <summary>
        /// Fragment shader for Phong lighting.
        /// Implements basic ambient, diffuse, and specular components.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string PhongLightingFragmentShader = @"#version 450 core
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vColor;
out vec4 FragColor;
uniform vec3 uViewPos;
uniform vec3 uLightPos;
uniform vec3 uLightColor;
uniform float uShininess;
void main() {
    vec3 norm = normalize(vNormal);
    vec3 lightDir = normalize(uLightPos - vFragPos);
    vec3 viewDir = normalize(uViewPos - vFragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    vec3 ambient = 0.1 * uLightColor;
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * uLightColor;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uShininess);
    vec3 specular = spec * uLightColor * 0.5;
    vec3 result = (ambient + diffuse + specular) * vColor;
    FragColor = vec4(result, 1.0);
}";

        /// <summary>
        /// Vertex shader for instanced rendering.
        /// Each instance has its own transformation matrix.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string InstancingVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in mat4 aInstanceMatrix; // per-instance transformation
uniform mat4 view;
uniform mat4 projection;

void main() {
    gl_Position = projection * view * aInstanceMatrix * vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Fragment shader for instanced rendering.
        /// Uses a uniform color per instance group.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string InstancingFragmentShader = @"#version 450 core
out vec4 FragColor;
uniform vec4 uColor;
void main() {
    FragColor = uColor;
}";

        /// <summary>
        /// Vertex shader for full-screen post-processing effects.
        /// Passes normalized texture coordinates for screen sampling.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string PostProcessingVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
void main() {
    vTexCoord = aTexCoord;
    gl_Position = vec4(aPos.xy, 0.0, 1.0);
}";

        /// <summary>
        /// Fragment shader for post-processing.
        /// Samples from the rendered scene texture and applies simple tone mapping.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — reserved for future implementation.</remarks>
        public const string PostProcessingFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uScene;
uniform float uExposure;

void main() {
    vec3 hdrColor = texture(uScene, vTexCoord).rgb;
    vec3 mapped = vec3(1.0) - exp(-hdrColor * uExposure);
    FragColor = vec4(mapped, 1.0);
}";

        /// <summary>
        /// Vertex shader for water ripple effect.
        /// Passes vertex position and texture coordinates for fragment processing.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — placeholder for future ripple effect implementation.</remarks>
        public const string WaterRippleVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    vTexCoord = aTexCoord;
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Fragment shader for water ripple effect.
        /// Samples the texture and provides structure for future ripple animation.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — placeholder, can later add time-based ripple calculation.</remarks>
        public const string WaterRippleFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform float uTime; // for future ripple animation

void main() {
    // Currently just samples the texture directly
    vec2 uv = vTexCoord; // future: modify with sin/cos ripple based on uTime
    FragColor = texture(uTexture, uv);
}";

        /// <summary>
        /// Vertex shader for volumetric fog.
        /// Outputs world-space position for per-fragment fog calculations.
        /// </summary>
        /// <remarks>UNUSED FOR NOW — placeholder for volumetric fog implementation.</remarks>
        public const string VolumetricFogVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;

out vec3 vWorldPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    vec4 worldPos = model * vec4(aPosition, 1.0);
    vWorldPos = worldPos.xyz;
    gl_Position = projection * view * worldPos;
}";

        /// <summary>
        /// Fragment shader for volumetric fog.
        /// Computes fog factor based on world-space position (simple linear placeholder).
        /// </summary>
        /// <remarks>UNUSED FOR NOW — placeholder for volumetric fog implementation.</remarks>
        public const string VolumetricFogFragmentShader = @"#version 450 core
in vec3 vWorldPos;
out vec4 FragColor;

uniform vec3 uFogColor;
uniform float uFogDensity; // placeholder

void main() {
    // Simple linear fog based on height (Y coordinate)
    float fogFactor = clamp(vWorldPos.y * uFogDensity, 0.0, 1.0);
    FragColor = vec4(uFogColor * fogFactor, 1.0);
}";
    }
}
