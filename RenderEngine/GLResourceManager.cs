/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GlResourceManager.cs
 * PURPOSE:     Manager that holds all texture information and shaders on runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderEngine
{
    /// <summary>
    /// Texture and Shader manager for OpenGL.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class GlResourceManager : IDisposable
    {
        /// <summary>
        /// The texture cache
        /// </summary>
        private readonly Dictionary<string, int> _textureCache = new();

        /// <summary>
        /// The program cache
        /// </summary>
        private readonly Dictionary<ShaderTypeApp, int> _programCache = new();

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets or sets a value indicating whether [use matrices].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use matrices]; otherwise, <c>false</c>.
        /// </value>
        public bool UseMatrices { get; set; } = true;

        // --- Textures ---

        /// <summary>
        /// The fallback texture identifier
        /// </summary>
        private int _fallbackTextureId = -1;

        /// <summary>
        /// Gets the texture.
        /// Be careful, this can overwrite existing textures.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Id of Texture</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public int GetTexture(string filePath)
        {
            if (_textureCache.TryGetValue(filePath, out var texId))
                return texId;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            texId = OpenTkHelper.LoadTextureFromFile(filePath);

            _textureCache[filePath] = texId;
            return texId;
        }

        /// <summary>
        /// Gets the fallback texture.
        /// </summary>
        /// <returns>Id of Texture, fallback, checkerboard</returns>
        public int GetFallbackTexture()
        {
            if (_fallbackTextureId >= 0) return _fallbackTextureId;

            var pixels = new byte[] { 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 0, 255 };
            _fallbackTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fallbackTextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2, 2, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // 1. CLAMP TO EDGE: Stops the edges from wrapping around and bleeding colors at the borders
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // 2. MIPMAPS: Keep it sharp up close, but blend it in the distance to stop the diagonal zippering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // 3. ANISOTROPIC FILTERING: The ultimate fix for viewing textures at sharp "roof" angles.
            // (Using hex codes safely bypasses any OpenTK version differences for this extension)
            GL.GetFloat((GetPName)0x84FF, out float maxAniso); // GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT
            if (maxAniso > 0.0f)
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FE, maxAniso); // GL_TEXTURE_MAX_ANISOTROPY_EXT
            }

            // Generate the smaller mipmap versions so the MinFilter works
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return _fallbackTextureId;
        }

        /// <summary>
        /// Creates the master textures.
        /// </summary>
        /// <param name="textures">The textures.</param>
        /// <returns>Dictionary of Input id and Id of texture</returns>
        /// <exception cref="ArgumentNullException">Texture {id} is null.</exception>
        public static Dictionary<int, int> CreateMasterTextures(Dictionary<int, UnmanagedImageBuffer?> textures)
        {
            var result = new Dictionary<int, int>();

            foreach (var (id, tex) in textures)
            {
                if (tex == null)
                    throw new ArgumentNullException(nameof(textures), $"Texture {id} is null.");

                // Create GL texture from Bitmap
                var texId = OpenTkHelper.CreateTexture(tex, opaqueFastPath: false);

                // Store in result dictionary
                result[id] = texId;
            }

            return result;
        }

        /// <summary>
        /// Gets the texture.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="opaqueFastPath">if set to <c>true</c> [opaque fast path].</param>
        /// <returns>Id of Texture</returns>
        public int GetTexture(UnmanagedImageBuffer buffer, bool opaqueFastPath = false)
        {
            return OpenTkHelper.CreateTexture(buffer, opaqueFastPath);
        }

        // --- Shader Programs ---

        /// <summary>
        /// Gets the shader program.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="ArgumentOutOfRangeException">appType</exception>
        public int GetShaderProgram(ShaderTypeApp appType)
        {
            if (_programCache.TryGetValue(appType, out var programId))
                return programId;

            string vertexSrc, fragmentSrc;
            switch (appType)
            {
                case ShaderTypeApp.SolidColor:
                    vertexSrc = ShaderResource.SolidColorVertexShader;
                    fragmentSrc = ShaderResource.SolidColorFragmentShader;
                    break;
                case ShaderTypeApp.TexturedQuad:
                    vertexSrc = ShaderResource.TextureMappingVertexShader;
                    fragmentSrc = ShaderResource.TextureMappingFragmentShader;
                    break;
                case ShaderTypeApp.VertexColor:
                    vertexSrc = ShaderResource.VertexColorVertexShader;
                    fragmentSrc = ShaderResource.VertexColorFragmentShader;
                    break;

                case ShaderTypeApp.Wireframe:
                    vertexSrc = UseMatrices
                        ? ShaderResource.WireframeVertexShader
                        : ShaderResource.WireframeVertexShaderPassThrough;
                    fragmentSrc = UseMatrices
                        ? ShaderResource.WireframeFragmentShader
                        : ShaderResource.WireframeFragmentShaderPassThrough;
                    break;

                case ShaderTypeApp.TextureArrayTilemap:
                    vertexSrc = ShaderResource.TextureArrayTilemapVertexShader;
                    fragmentSrc = ShaderResource.TextureArrayTilemapFragmentShader;
                    break;
                // ----- 2D shaders -----
                case ShaderTypeApp.SolidColor2D:
                    vertexSrc = ShaderResource.SolidColor2DVertexShader;
                    fragmentSrc = ShaderResource.SolidColor2DFragmentShader;
                    break;
                case ShaderTypeApp.VertexColor2D:
                    vertexSrc = ShaderResource.VertexColor2DVertexShader;
                    fragmentSrc = ShaderResource.VertexColor2DFragmentShader;
                    break;
                case ShaderTypeApp.TexturedQuad2D:
                    vertexSrc = ShaderResource.TexturedQuad2DVertexShader;
                    fragmentSrc = ShaderResource.TexturedQuad2DFragmentShader;
                    break;
                case ShaderTypeApp.PhongLighting: // UNUSED for now
                    vertexSrc = ShaderResource.PhongLightingVertexShader;
                    fragmentSrc = ShaderResource.PhongLightingFragmentShader;
                    break;
                case ShaderTypeApp.Instancing: // UNUSED for now
                    vertexSrc = ShaderResource.InstancingVertexShader;
                    fragmentSrc = ShaderResource.InstancingFragmentShader;
                    break;
                case ShaderTypeApp.PostProcessing: // UNUSED for now
                    vertexSrc = ShaderResource.PostProcessingVertexShader;
                    fragmentSrc = ShaderResource.PostProcessingFragmentShader;
                    break;
                case ShaderTypeApp.WaterRipple: // UNUSED for now
                    vertexSrc = ShaderResource.WaterRippleVertexShader;
                    fragmentSrc = ShaderResource.WaterRippleFragmentShader;
                    break;

                case ShaderTypeApp.VolumetricFog: // UNUSED for now
                    vertexSrc = ShaderResource.VolumetricFogVertexShader;
                    fragmentSrc = ShaderResource.VolumetricFogFragmentShader;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(appType));
            }

            programId = CompileAndLinkShader(vertexSrc, fragmentSrc);
            _programCache[appType] = programId;
            return programId;
        }


        /// <summary>
        /// Uses the shader.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        public void UseShader(ShaderTypeApp appType)
        {
            GL.UseProgram(GetShaderProgram(appType));
        }

        // --- Compile & link ---

        /// <summary>
        /// Compiles the and link shader.
        /// </summary>
        /// <param name="vertexSource">The vertex source.</param>
        /// <param name="fragmentSource">The fragment source.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="Exception">Shader program link failed: " + info</exception>
        private static int CompileAndLinkShader(string vertexSource, string fragmentSource)
        {
            var vertex = CompileSingleShader(ShaderType.VertexShader, vertexSource);
            var fragment = CompileSingleShader(ShaderType.FragmentShader, fragmentSource);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
            if (status != (int)All.True)
            {
                var info = GL.GetProgramInfoLog(program);
                GL.DeleteProgram(program);
                throw new Exception("Shader program link failed: " + info);
            }

            // shaders can be detached and deleted after linking
            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        /// <summary>
        /// Compiles the single shader.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="source">The source.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="Exception">Error compiling {type}: {info}</exception>
        private static int CompileSingleShader(ShaderType type, string source)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);

            if (status == (int)All.True) return shader;

            var info = GL.GetShaderInfoLog(shader);
            GL.DeleteShader(shader);
            throw new Exception($"Error compiling {type}: {info}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            foreach (var texId in _textureCache.Values)
                GL.DeleteTexture(texId);

            foreach (var program in _programCache.Values)
                GL.DeleteProgram(program);

            _textureCache.Clear();
            _programCache.Clear();

            if (_fallbackTextureId >= 0)
            {
                GL.DeleteTexture(_fallbackTextureId);
                _fallbackTextureId = -1;
            }

            GC.SuppressFinalize(this);
        }
    }
}