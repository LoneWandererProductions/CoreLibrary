/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GlResourceManager.cs
 * PURPOSE:     Manager that holds all texture information, shaders, and virtual procedural atlases on runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using Imaging.Texture;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderEngine
{
    /// <summary>
    /// Texture and Shader manager for OpenGL. Handles standard image streaming 
    /// and manages a high-range virtual procedural texture atlas catalog with lazy loading.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class GlResourceManager : IDisposable
    {
        // =================================================================================
        // STORAGE REGISTRIES
        // =================================================================================

        /// <summary>
        /// The texture cache for standard file textures.
        /// </summary>
        private readonly Dictionary<string, int> _textureCache = new();

        /// <summary>
        /// The program cache for compiled shader programs.
        /// </summary>
        private readonly Dictionary<ShaderTypeApp, int> _programCache = new();

        /// <summary>
        /// Maps Virtual IDs (e.g., 10002) to actual OpenGL Texture Handles allocated by the GPU.
        /// </summary>
        private readonly Dictionary<int, int> _bakedProceduralCache = new();

        /// <summary>
        /// The disposed state tracker.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The fallback texture identifier
        /// </summary>
        private int _fallbackTextureId = -1;

        // Configuration fields for dynamic on-demand lazy generation parameters
        private object _lazyNoiseGen;
        private int _procWidth = 256;
        private int _procHeight = 256;

        /// <summary>
        /// Gets or sets a value indicating whether [use matrices].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use matrices]; otherwise, <c>false</c>.
        /// </value>
        public bool UseMatrices { get; set; } = true;

        // =================================================================================
        // VIRTUAL PROCEDURAL CATALOGUE MENU
        // =================================================================================

        /// <summary>
        /// Immutable structural menu catalog outlining every procedural texture type 
        /// available for generation, mapped by its high-range safe identity key.
        /// </summary>
        public static IReadOnlyDictionary<int, (string Name, string Description)> ProceduralCatalogue { get; } =
            new Dictionary<int, (string Name, string Description)>
            {
                { 10000, ("Noise", "Procedural monochrome simplex noise distribution grain.") },
                { 10001, ("Clouds", "Fluid multi-octave cloud sky pattern spectrum.") },
                { 10002, ("Marble", "Sinusoidal soft marble stone vein matrix layout.") },
                { 10003, ("Wood", "Concentric organic natural wood grain ring texture layers.") },
                { 10004, ("Wave", "Continuous fluid phase wave spectrum running on a pure HSV scale.") },
                { 10005, ("Crosshatch", "Pixel-perfect grid intersecting alignment lines overlay.") },
                { 10006, ("Concrete", "Industrial high-contrast gritty stone concrete texture map.") },
                { 10007, ("Canvas", "Woven organic fiber cloth structural mesh with random fraying cutoffs.") },
                // WIRED: New nature engine assets added below
                { 10008, ("TreeBark", "Anisotropic domain-warped vertical furrowed bark grain.") },
                { 10009, ("Foliage", "Dense organic canopy composed of distance-pinched leaf profiles.") },
                { 10010, ("WoodPlank", "Longitudinal sawn wood board with sweeping grain and cathedral arches.") }
            };

        // =================================================================================
        // PROCEDURAL LAZY LOADING INTEGRATION PASS
        // =================================================================================

        /// <summary>
        /// Configures the base resolution and noise algorithm context used for dynamic on-demand baking.
        /// </summary>
        /// <param name="defaultWidth">Default pixel width for generated textures.</param>
        /// <param name="defaultHeight">Default pixel height for generated textures.</param>
        /// <param name="noiseGeneratorInstance">An initialized instance of your custom NoiseGenerator class.</param>
        public void ConfigureLazyBaking(int defaultWidth, int defaultHeight, object noiseGeneratorInstance)
        {
            _procWidth = defaultWidth;
            _procHeight = defaultHeight;
            _lazyNoiseGen = noiseGeneratorInstance;
        }

        /// <summary>
        /// Safely translates texture parameter pointers. If a virtual ID >= 10000 is requested 
        /// and has not been baked yet, it intercepts the sequence and builds it natively on demand.
        /// </summary>
        /// <param name="textureId">The structural raw source key identifier parameter passing through.</param>
        /// <returns>A validated OpenGL texture descriptor slot handle directly executable by the hardware driver.</returns>
        public int ResolveTextureId(int textureId)
        {
            if (textureId >= 10000)
            {
                if (_bakedProceduralCache.TryGetValue(textureId, out var activeGlId))
                {
                    return activeGlId;
                }

                return LazyBakeTexture(textureId);
            }

            return textureId <= 0 ? GetFallbackTexture() : textureId;
        }

        /// <summary>
        /// Internally intercepts execution to execute a clean math texture compilation on the fly.
        /// </summary>
        private int LazyBakeTexture(int id)
        {
            if (!ProceduralCatalogue.TryGetValue(id, out _))
                return GetFallbackTexture();

            if (_lazyNoiseGen == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType("Imaging.NoiseGenerator") ??
                               assembly.GetType("Imaging.Helpers.NoiseGenerator");
                    if (type != null)
                    {
                        _lazyNoiseGen = Activator.CreateInstance(type, _procWidth, _procHeight);
                        break;
                    }
                }

                if (_lazyNoiseGen == null)
                    throw new InvalidOperationException(
                        "Procedural layout execution halted: No NoiseGenerator instance was initialized or found.");
            }

            var rawBuffer = id switch
            {
                10000 => TextureMathEngine.GenerateNoise(_procWidth, _procHeight, _lazyNoiseGen),
                10001 => TextureMathEngine.GenerateClouds(_procWidth, _procHeight, _lazyNoiseGen),
                10002 => TextureMathEngine.GenerateMarble(_procWidth, _procHeight, _lazyNoiseGen),
                10003 => TextureMathEngine.GenerateWood(_procWidth, _procHeight, _lazyNoiseGen),
                10004 => TextureMathEngine.GenerateWave(_procWidth, _procHeight, _lazyNoiseGen),
                10005 => TextureMathEngine.GenerateCrosshatch(_procWidth, _procHeight, lineSpacing: 32,
                    lineThickness: 2),
                10006 => TextureMathEngine.GenerateConcrete(_procWidth, _procHeight, _lazyNoiseGen),
                10007 => TextureMathEngine.GenerateCanvas(_procWidth, _procHeight, lineSpacing: 8, lineThickness: 1),
                // WIRED: Divert requests safely into your custom factory configurations
                10008 => TextureFactory.GenerateTreeBark(_procWidth, _procHeight, _lazyNoiseGen),
                10009 => TextureFactory.GenerateFoliage(_procWidth, _procHeight),
                10010 => TextureFactory.GenerateWoodPlank(_procWidth, _procHeight, _lazyNoiseGen),
                _ => null
            };

            if (rawBuffer == null) return GetFallbackTexture();

            var glHandle = GetTexture(rawBuffer, linearFilter: true);
            _bakedProceduralCache[id] = glHandle;

            return glHandle;
        }

        // =================================================================================
        // CORE TEXTURE DRIVER PASSES
        // =================================================================================

        /// <summary>
        /// Gets the texture from file system pathing.
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
        /// Uploads a platform-agnostic <see cref="RawTextureBuffer"/> directly to OpenGL memory.
        /// </summary>
        /// <param name="rawBuffer">Source mathematical raw layout buffer.</param>
        /// <param name="linearFilter">Whether to use linear filtering (true) or nearest/point sampling (false).</param>
        /// <returns>OpenGL texture identifier index slot pointer.</returns>
        public int GetTexture(RawTextureBuffer rawBuffer, bool linearFilter = false)
        {
            var sampleId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, sampleId);

            // Reverted directly back to PixelInternalFormat.Rgba from Last Known Good state
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                rawBuffer.Width,
                rawBuffer.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                rawBuffer.PixelData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            if (linearFilter)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.NearestMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Nearest);
            }

            // 5. Anisotropic Filtering — checkerboards at grazing angles are the textbook
            // case FOR anisotropic filtering, not against it.
            GL.GetFloat((GetPName)0x84FF, out float maxAniso);
            if (maxAniso > 0.0f)
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FE, Math.Min(maxAniso, 4.0f));
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return sampleId;
        }

        /// <summary>
        /// Gets the fallback texture.
        /// </summary>
        /// <returns>Id of Texture, fallback, checkerboard</returns>
        public int GetFallbackTexture()
        {
            if (_fallbackTextureId >= 0) return _fallbackTextureId;

            // 1. Expand the 2x2 grid to 64x64 to create a deep, smooth Mipmap chain
            const int size = 64;
            var halfSize = size / 2;
            var pixels = new byte[size * size * 4];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    // Recreates your exact Black/White/White/Black 4-square pattern
                    var isWhite = (x >= halfSize) ^ (y >= halfSize);
                    var color = isWhite ? (byte)255 : (byte)0;

                    var idx = (y * size + x) * 4;
                    pixels[idx] = color; // R
                    pixels[idx + 1] = color; // G
                    pixels[idx + 2] = color; // B
                    pixels[idx + 3] = 255; // A
                }
            }

            _fallbackTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fallbackTextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size, size, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // 2. Wrap mode (Assuming your custom meshes require clamping instead of repeating)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            // 3. The filtering hybrid (Smooth distance, Sharp close-up)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);

            // 4. Mipmap generation (Now generates a 6-level deep chain instead of just 1)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // 5. Anisotropic Filtering (Disabled temporarily for the fallback)
            // High-contrast procedurals like checkerboards often fight with Anisotropic sampling. 
            // Since we created proper mipmaps, we can turn this off for the fallback texture.
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FE, 1.0f);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return _fallbackTextureId;
        }

        /// <summary>
        /// Creates a lookup collection of compiled GPU texture resources directly from raw math arrays.
        /// </summary>
        /// <param name="textures">The procedural textures data map.</param>
        /// <returns>Dictionary of Input id and Id of texture</returns>
        /// <exception cref="ArgumentNullException">proceduralTextures - Procedural texture item entry ID evaluates to null.</exception>
        public static Dictionary<int, int> CreateMasterTextures(Dictionary<int, UnmanagedImageBuffer?> textures)
        {
            var result = new Dictionary<int, int>();

            foreach (var (id, tex) in textures)
            {
                if (tex == null)
                    throw new ArgumentNullException(nameof(textures), $"Texture {id} is null.");

                var texId = OpenTkHelper.CreateTexture(tex, opaqueFastPath: false);
                result[id] = texId;
            }

            return result;
        }

        /// <summary>
        /// Gets the texture using legacy UnmanagedImageBuffers.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="opaqueFastPath">if set to <c>true</c> [opaque fast path].</param>
        /// <returns>Id of Texture</returns>
        public int GetTexture(UnmanagedImageBuffer buffer, bool opaqueFastPath = false)
        {
            return OpenTkHelper.CreateTexture(buffer, opaqueFastPath);
        }

        // =================================================================================
        // SHADER ROUTING INTERFACES
        // =================================================================================

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

        /// <summary>
        /// Compiles the and link shader.
        /// </summary>
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

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        /// <summary>
        /// Compiles the single shader.
        /// </summary>
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

        // =================================================================================
        // CLEANUP UNMANAGED ASSET STACK
        // =================================================================================

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            foreach (var texId in _textureCache.Values)
                GL.DeleteTexture(texId);

            foreach (var texId in _bakedProceduralCache.Values)
                GL.DeleteTexture(texId);

            foreach (var program in _programCache.Values)
                GL.DeleteProgram(program);

            _textureCache.Clear();
            _bakedProceduralCache.Clear();
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
