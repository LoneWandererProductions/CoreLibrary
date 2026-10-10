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
        public const string SkyboxFragmentShader = @"#version 450 core
in vec3 vTexCoord;
out vec4 FragColor;
uniform samplerCube uSkybox;
void main() { FragColor = texture(uSkybox, vTexCoord); }";

        /// <summary>
        /// Defines cube vertex coordinates used to render the skybox.
        /// </summary>
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
        public const string SolidColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
void main() { gl_Position = projection * view * model * vec4(aPosition, 1.0); }";

        /// <summary>
        /// Fragment shader for solid color rendering.
        /// </summary>
        public const string SolidColorFragmentShader = @"#version 450 core
uniform vec4 uColor;
out vec4 FragColor;
void main() { FragColor = uColor; }";

        /// <summary>
        /// Vertex shader for basic texture mapping.
        /// </summary>
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
        public const string TextureMappingFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
void main() { FragColor = texture(uTexture, vTexCoord); }";

        /// <summary>
        /// Vertex shader for per-vertex coloring.
        /// </summary>
        public const string VertexColorVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;
out vec4 vColor;
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
        public const string VertexColorFragmentShader = @"#version 450 core
in vec4 vColor;
out vec4 FragColor;
void main() { FragColor = vColor; }";

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
        public const string WireframeFragmentShader = @"#version 450 core
out vec4 FragColor;
void main() { FragColor = vec4(0.0, 1.0, 0.0, 1.0); }";

        /// <summary>
        /// Wireframe vertex shader that ignores model/view/projection matrices.
        /// </summary>
        public const string WireframeVertexShaderPassThrough = @"#version 450 core
layout(location = 0) in vec3 aPosition;
void main() {
    gl_Position = vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Wireframe fragment shader with uniform color.
        /// </summary>
        public const string WireframeFragmentShaderPassThroughUniform = @"#version 450 core
out vec4 FragColor;
uniform vec4 uColor;
void main() {
    FragColor = uColor;
}";

        /// <summary>
        /// Wireframe fragment shader for pass-through vertex shader.
        /// </summary>
        public const string WireframeFragmentShaderPassThrough = @"#version 450 core
out vec4 FragColor;
void main() {
    FragColor = vec4(0.0, 1.0, 0.0, 1.0);
}";

        /// <summary>
        /// Vertex shader for rendering tilemaps using 2D texture arrays.
        /// </summary>
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
        public const string TextureArrayTilemapFragmentShader = @"#version 450 core
in vec2 vTexCoord;
flat in int vTileIndex;
out vec4 FragColor;
uniform sampler2DArray uTextureArray;
void main() { FragColor = texture(uTextureArray, vec3(vTexCoord, vTileIndex)); }";

        /// <summary>
        /// General-purpose vertex shader for basic colored geometry.
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

        /// <summary>
        /// General-purpose fragment shader for colored geometry.
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
        /// Vertex shader for solid 2D rendering.
        /// </summary>
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
        public const string SolidColor2DFragmentShader = @"#version 450 core
uniform vec4 uColor;
out vec4 FragColor;
void main() { FragColor = uColor; }";

        /// <summary>
        /// Vertex shader for 2D geometry with per-vertex color attributes.
        /// </summary>
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
        /// </summary>
        public const string VertexColor2DFragmentShader = @"#version 450 core
in vec4 vColor;
out vec4 FragColor;
void main() {
    FragColor = vColor;
}";

        /// <summary>
        /// Vertex shader for textured 2D quads.
        /// </summary>
        public const string TexturedQuad2DVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTex;
out vec2 vTex;
uniform vec2 uViewport;
void main() {
    vec2 pos = aPos / uViewport * 2.0 - 1.0;
    pos.y = -pos.y;
    gl_Position = vec4(pos, 0.0, 1.0);
    vTex = vec2(aTex.x, 1.0 - aTex.y);
}";

        /// <summary>
        /// Fragment shader for textured 2D quads.
        /// </summary>
        public const string TexturedQuad2DFragmentShader = @"#version 450 core
in vec2 vTex;
out vec4 FragColor;
uniform sampler2D uTexture;
void main() {
    FragColor = texture(uTexture, vTex);
}";

        /// <summary>
        /// Vertex shader for Phong lighting.
        /// </summary>
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
        /// </summary>
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
        /// </summary>
        public const string InstancingVertexShader = @"#version 450 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in mat4 aInstanceMatrix;
uniform mat4 view;
uniform mat4 projection;

void main() {
    gl_Position = projection * view * aInstanceMatrix * vec4(aPosition, 1.0);
}";

        /// <summary>
        /// Fragment shader for instanced rendering.
        /// </summary>
        public const string InstancingFragmentShader = @"#version 450 core
out vec4 FragColor;
uniform vec4 uColor;
void main() {
    FragColor = uColor;
}";

        /// <summary>
        /// Vertex shader for full-screen post-processing effects.
        /// Passes normalized texture coordinates for screen-space sampling.
        /// </summary>
        public const string PostProcessingVertexShader = @"#version 450 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
void main() {
    vTexCoord = aTexCoord;
    gl_Position = vec4(aPos.xy, 0.0, 1.0);
}";

        /// <summary>
        /// Fragment shader for full-screen post-processing effects.
        /// Supports dynamic filter modes: Passthrough, Kuwahara Painterly, Posterize & Edge Outlines, Impasto Canvas, and Grayscale Mono.
        /// </summary>
        public const string PostProcessingFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uScene;
uniform vec2 uScreenSize;
uniform int uFilterMode;

// Kuwahara Filter for Oil-Paint effect
vec3 KuwaharaFilter(vec2 uv) {
    int radius = 3;
    vec2 invSize = 1.0 / uScreenSize;
    float n = float((radius + 1) * (radius + 1));

    vec3 m[4];
    vec3 s[4];
    for (int k = 0; k < 4; ++k) {
        m[k] = vec3(0.0);
        s[k] = vec3(0.0);
    }

    for (int j = -radius; j <= 0; ++j) {
        for (int i = -radius; i <= 0; ++i) {
            vec3 c = texture(uScene, uv + vec2(i, j) * invSize).rgb;
            m[0] += c;
            s[0] += c * c;
        }
    }

    for (int j = -radius; j <= 0; ++j) {
        for (int i = 0; i <= radius; ++i) {
            vec3 c = texture(uScene, uv + vec2(i, j) * invSize).rgb;
            m[1] += c;
            s[1] += c * c;
        }
    }

    for (int j = 0; j <= radius; ++j) {
        for (int i = -radius; i <= 0; ++i) {
            vec3 c = texture(uScene, uv + vec2(i, j) * invSize).rgb;
            m[2] += c;
            s[2] += c * c;
        }
    }

    for (int j = 0; j <= radius; ++j) {
        for (int i = 0; i <= radius; ++i) {
            vec3 c = texture(uScene, uv + vec2(i, j) * invSize).rgb;
            m[3] += c;
            s[3] += c * c;
        }
    }

    float minVar = 1e+20;
    vec3 finalColor = vec3(0.0);

    for (int k = 0; k < 4; ++k) {
        m[k] /= n;
        s[k] = abs(s[k] / n - m[k] * m[k]);
        float sigma2 = s[k].r + s[k].g + s[k].b;
        if (sigma2 < minVar) {
            minVar = sigma2;
            finalColor = m[k];
        }
    }

    return finalColor;
}

// Sobel/Delta Edge Detection
float DetectEdge(vec2 uv) {
    vec2 invSize = 1.0 / uScreenSize;
    vec3 c = texture(uScene, uv).rgb;
    vec3 cUp = texture(uScene, uv + vec2(0.0, invSize.y)).rgb;
    vec3 cRight = texture(uScene, uv + vec2(invSize.x, 0.0)).rgb;

    float diff = length(c - cUp) + length(c - cRight);
    return diff > 0.12 ? 0.0 : 1.0;
}

// Posterization
vec3 Posterize(vec3 color, float steps) {
    return floor(color * steps) / steps;
}

void main() {
    vec3 baseColor = texture(uScene, vTexCoord).rgb;

    if (uFilterMode == 1) {
        // Mode 1: Painterly (Kuwahara)
        FragColor = vec4(KuwaharaFilter(vTexCoord), 1.0);
    } else if (uFilterMode == 2) {
        // Mode 2: Posterize & Edge Outlines
        vec3 pColor = Posterize(baseColor, 6.0);
        float edge = DetectEdge(vTexCoord);
        FragColor = vec4(pColor * edge, 1.0);
    } else if (uFilterMode == 3) {
        // Mode 3: Painterly Impasto Canvas
        vec3 kColor = KuwaharaFilter(vTexCoord);
        vec3 pColor = Posterize(kColor, 8.0);
        float edge = DetectEdge(vTexCoord);

        // Procedural canvas weave texture pattern
        vec2 st = vTexCoord * uScreenSize * 0.25;
        float grain = sin(st.x * 12.566) * cos(st.y * 12.566) * 0.05;
        pColor = clamp(pColor + vec3(grain), 0.0, 1.0);

        FragColor = vec4(pColor * edge, 1.0);
    } else if (uFilterMode == 4) {
        // Mode 4: Grayscale Mono
        float gray = dot(baseColor, vec3(0.299, 0.587, 0.114));
        FragColor = vec4(vec3(gray), 1.0);
    } else {
        // Mode 0: Passthrough / None
        FragColor = vec4(baseColor, 1.0);
    }
}";

        /// <summary>
        /// Vertex shader for water ripple effect.
        /// </summary>
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
        /// </summary>
        public const string WaterRippleFragmentShader = @"#version 450 core
in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform float uTime;

void main() {
    vec2 uv = vTexCoord;
    FragColor = texture(uTexture, uv);
}";

        /// <summary>
        /// Vertex shader for volumetric fog.
        /// </summary>
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
        /// </summary>
        public const string VolumetricFogFragmentShader = @"#version 450 core
in vec3 vWorldPos;
out vec4 FragColor;

uniform vec3 uFogColor;
uniform float uFogDensity;

void main() {
    float fogFactor = clamp(vWorldPos.y * uFogDensity, 0.0, 1.0);
    FragColor = vec4(uFogColor * fogFactor, 1.0);
}";
    }
}
