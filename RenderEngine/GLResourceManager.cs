/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GlResourceManager.cs
 * PURPOSE:     Manager that holds all texture information, shaders, and virtual procedural atlases on runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using Imaging.Objects;
using Imaging.Texture;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderEngine
{
    /// <summary>
    /// Texture and Shader manager for OpenGL. Handles standard image streaming
    /// and manages a high-range virtual procedural texture atlas catalog with lazy loading.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class GlResourceManager : IDisposable
    {
        // --- STORAGE REGISTRIES ---

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

        /// <summary>
        /// Texture ids (same ids as everywhere else, virtual ids allowed) of the four terrain splat layers.
        /// Layer 0..2 are the RGB weights, layer 3 is the remainder. Set this once where textures are registered.
        /// </summary>
        public int[]? TerrainLayerTextureIds { get; set; }

        /// <summary>
        /// Terrain splat texture repeats per world unit.
        /// </summary>
        public float TerrainTiling { get; set; } = 0.25f;

        /// <summary>
        /// Terrain splat transition sharpness (1 = linear, higher = narrower).
        /// </summary>
        public float TerrainSharpness { get; set; } = 2.0f;

        /// <summary>
        /// Terrain splat edge noise amount (0 = straight fade).
        /// </summary>
        public float TerrainEdgeNoise { get; set; } = 0.3f;

        /// <summary>
        /// All tracked handles
        /// </summary>
        private readonly HashSet<int> _allTrackedHandles = new();

        // --- Configuration fields for dynamic on-demand lazy generation parameters ---

        /// <summary>
        /// The lazy noise gen
        /// </summary>
        private NoiseGenerator? _lazyNoiseGen;

        /// <summary>
        /// The proc width
        /// </summary>
        private int _procWidth = 256;

        /// <summary>
        /// The proc height
        /// </summary>
        private int _procHeight = 256;

        /// <summary>
        /// Gets or sets a value indicating whether [use matrices].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use matrices]; otherwise, <c>false</c>.
        /// </value>
        public bool UseMatrices { get; set; } = true;

        // --- VIRTUAL PROCEDURAL CATALOGUE MENU ---

        /// <summary>
        /// Immutable structural menu catalog outlining every procedural texture type
        /// available for generation, mapped by its high-range safe identity key.
        /// </summary>
        private static readonly Dictionary<int, ProceduralEntry> ProceduralRegistry = new()
        {
            {
                10000,
                new("Noise", "Procedural monochrome simplex noise distribution grain.",
                    (w, h, n) => TextureMathEngine.GenerateNoise(w, h, n))
            },
            {
                10001,
                new("Clouds", "Fluid multi-octave cloud sky pattern spectrum.",
                    (w, h, n) => TextureMathEngine.GenerateClouds(w, h, n))
            },
            {
                10002,
                new("Marble", "Sinusoidal soft marble stone vein matrix layout.",
                    (w, h, n) => TextureMathEngine.GenerateMarble(w, h, n))
            },
            {
                10003,
                new("Wood", "Concentric organic natural wood grain ring texture layers.",
                    (w, h, n) => TextureMathEngine.GenerateWood(w, h, n))
            },
            {
                10004,
                new("Wave", "Continuous fluid phase wave spectrum running on a pure HSV scale.",
                    (w, h, n) => TextureMathEngine.GenerateWave(w, h, n))
            },
            {
                10005,
                new("Crosshatch", "Pixel-perfect grid intersecting alignment lines overlay.",
                    (w, h, _) =>
                        TextureMathEngine.GenerateCrosshatch(w, h, 32, 2, bgR: 140, bgG: 40, bgB: 40, bgA: 255))
            },
            {
                10006,
                new("Concrete", "Industrial high-contrast gritty stone concrete texture map.",
                    (w, h, n) => TextureMathEngine.GenerateConcrete(w, h, n))
            },
            {
                10007,
                new("Canvas", "Woven organic fiber cloth structural mesh with random fraying cutoffs.",
                    (w, h, _) => TextureMathEngine.GenerateCanvas(w, h, 8, 1))
            },
            {
                10008,
                new("TreeBark", "Anisotropic domain-warped vertical furrowed bark grain.",
                    (w, h, n) => TextureFactory.GenerateTreeBark(w, h, n))
            },
            {
                10009,
                new("Foliage", "Dense organic canopy composed of distance-pinched leaf profiles.",
                    (w, h, n) => TextureFactory.GenerateFoliage(w, h, n))
            },
            {
                10010,
                new("WoodPlank", "Longitudinal sawn wood board with sweeping grain and cathedral arches.",
                    (w, h, n) => TextureFactory.GenerateWoodPlank(w, h, n))
            },
            {
                10011,
                new("Cobblestone", "Voronoi cellular paving stone grid texture.",
                    (w, h, _) => TextureFactory.GenerateCobblestone(w, h))
            },
            {
                10012,
                new("Stone", "Directional shaded stone surface with mortar joints.",
                    (w, h, n) => TextureFactory.GenerateStoneTexture(w, h, n))
            },
            {
                10013,
                new("LavaPool", "Molten lava pool with turbulent color ramp mapping.",
                    (w, h, n) => TextureFactory.GenerateLavaPool(w, h, n))
            },
            {
                10014,
                new("MagicalEther", "Ethereal magical aura flow texture.",
                    (w, h, n) => TextureFactory.GenerateMagicalEther(w, h, n))
            },
            {
                10015,
                new("CrackedIce", "Cellular cracked ice crystal structure.",
                    (w, h, _) => TextureFactory.GenerateCrackedIce(w, h))
            },
            {
                10016,
                new("MagicPortal", "Domain-warped swirling portal energy texture.",
                    (w, h, n) => TextureFactory.GenerateMagicPortal(w, h, n))
            },
            {
                10017,
                new("PlasmaArc", "Ridged multifractal plasma energy arc.",
                    (w, h, n) => TextureFactory.GeneratePlasmaArc(w, h, n))
            },
            {
                10018,
                new("Steel", "Directional brushed steel metal surface with specular grit.",
                    (w, h, n) => TextureFactory.GenerateSteel(w, h, n))
            },
            {
                10019,
                new("Latex", "Glossy latex surface with steep specular falloff.",
                    (w, h, n) => TextureFactory.GenerateLatex(w, h, n))
            },
            {
                10020,
                new("Leather", "Pebbled organic skin pores and warped wrinkles.",
                    (w, h, n) => TextureFactory.GenerateLeather(w, h, n))
            },
            {
                10021,
                new("PolishedSteel", "High-gloss chrome reflective steel surface.",
                    (w, h, n) => TextureFactory.GeneratePolishedSteel(w, h, n))
            },
            {
                10022,
                new("LeafCloud", "Volumetric leaf cloud texture for bushes and tree canopies.",
                    (w, h, n) => TextureFactory.GenerateLeafCloud(w, h, n))
            },
            {
                10023,
                new("VolumetricLeafCloud",
                    "Smooth volumetric leaf cloud texture with rounded leaf puffs and distinct canopy gaps.",
                    (w, h, n) => TextureFactory.GenerateVolumetricLeafCloud(w, h, n))
            },
            {
                10024,
                new("TerrainGrass", "Smooth low-noise terrain grass texture with macro color transitions.",
                    (w, h, n) => TextureFactory.GenerateTerrainGrass(w, h, n))
            },
            {
                10025,
                new("TerrainDirt", "Soft muted terrain dirt texture with smooth soil transitions.",
                    (w, h, n) => TextureFactory.GenerateTerrainDirt(w, h, n))
            },
            {
                10026,
                new("MountainRock", "Mountain rock texture with macro height contours and directional slope shading.",
                    (w, h, n) => TextureFactory.GenerateMountainRock(w, h, n))
            },
            {
                10027,
                new("DungeonSandstone", "Layered dungeon sandstone texture with domain-warped sedimentary strata.",
                    (w, h, n) => TextureFactory.GenerateDungeonSandstone(w, h, n))
            },
            {
                10028,
                new("DungeonSandstoneFlat", "Flat soft-blended dungeon sandstone terrain texture.",
                    (w, h, n) => TextureFactory.GenerateDungeonSandstoneFlat(w, h, n))
            },
            {
                10029,
                new("RawIron", "Raw cast iron texture with fine surface pitting and micro-grit.",
                    (w, h, n) => TextureFactory.GenerateRawIron(w, h, n))
            },
            {
                10030,
                new("WroughtIron", "Wrought iron texture with subtle hammer marks and forged cellular facets.",
                    (w, h, n) => TextureFactory.GenerateWroughtIron(w, h, n))
            },
            {
                10031,
                new("RustedIron", "Rusted corroded iron texture with domain-warped oxidation layer.",
                    (w, h, n) => TextureFactory.GenerateRustedIron(w, h, n))
            },
            {
                10032,
                new("DesertSand", "Desert sand texture with gentle directional wind ripples.",
                    (w, h, n) => TextureFactory.GenerateDesertSand(w, h, n))
            },
            {
                10033,
                new("DesertSandFlat", "Flat soft-blended desert sand terrain texture for beds and background areas.",
                    (w, h, n) => TextureFactory.GenerateDesertSandFlat(w, h, n))
            },
            {
                10034,
                new("DarkLeafCloud", "Dark volumetric leaf cloud texture matched to floor tile shading.",
                    (w, h, n) => TextureFactory.GenerateLeafCloud(w, h, n, TextureConstants.GetDarkLeafCloudConfig()))
            },
            {
                10035,
                new("Gold", "Polished warm gold metal with soft highlights.",
                    (w, h, n) =>
                    {
                        var cfg = TextureConstants.GetGoldConfig();
                        return TextureMathEngine.GenerateColorMapped(w, h, n, cfg.RgbRamp!, cfg.TurbulenceSize);
                    })
            },
            {
                10036,
                new("Copper", "Polished reddish copper metal.",
                    (w, h, n) =>
                    {
                        var cfg = TextureConstants.GetCopperConfig();
                        return TextureMathEngine.GenerateColorMapped(w, h, n, cfg.RgbRamp!, cfg.TurbulenceSize);
                    })
            },
            {
                10037,
                new("Clay", "Fired terracotta clay with soft sandy grain.",
                    (w, h, n) =>
                    {
                        var cfg = TextureConstants.GetClayConfig();
                        return TextureMathEngine.GenerateColorMapped(w, h, n, cfg.RgbRamp!, cfg.TurbulenceSize);
                    })
            },
            {
                10038,
                new("StylizedFoliage", "Clean stylized leaf clumps with smooth canopy gradients for terrain.",
                    (w, h, n) => TextureMathEngine.GenerateStylizedFoliage(w, h, n, clusterSize: 32, transparentGaps: false))
            },
            {
                10039,
                new("StylizedFoliageCutout", "Stylized leaf clumps with alpha cutouts for low-poly tree meshes and bushes.",
                    (w, h, n) => TextureMathEngine.GenerateStylizedFoliage(w, h, n, clusterSize: 32, transparentGaps: true))
            },
            {
                10040,
                new("CobblestoneBeveled", "Flat-top cobblestones with narrow defined grout seams instead of bubbles.",
                    (w, h, _) => TextureMathEngine.GenerateCobblestoneBeveled(w, h, cellSize: 28))
            },
            {
                10041,
                new("ConcretePorous", "Architectural matte concrete with fine aggregate pores and no sine-ring artifacts.",
                    (w, h, n) => TextureMathEngine.GenerateConcretePorous(w, h, n, baseTone: 140, gritSpread: 22))
            },
{
                10042,
                new("GnarledTreeBark", "Twisted, corded tree bark with deep longitudinal furrows and cel-shading.",
                    (w, h, n) => TextureFactory.GenerateGnarledTreeBark(w, h, n))
            },
            {
                10043,
                new("VolumetricLeafCloudHardEdges", "Pixel-art leaf puff clusters with scalloped contours, dark outlines, and hard cel-shading.",
                    (w, h, n) => TextureFactory.GenerateVolumetricLeafCloudHardEdges(w, h, n))
            }
        };

        /// <summary>
        /// Public metadata catalog exposed for UI / Menu bindings (Backwards Compatible).
        /// </summary>
        public static IReadOnlyDictionary<int, (string Name, string Description)> ProceduralCatalogue { get; } =
            ProceduralRegistry.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.Name, kvp.Value.Description));

        // --- PROCEDURAL LAZY LOADING INTEGRATION PASS ---

        /// <summary>
        /// Initializes a new instance of the <see cref="GlResourceManager"/> class.
        /// </summary>
        public GlResourceManager()
        {
            _lazyNoiseGen = new NoiseGenerator(256, 256);
        }

        /// <summary>
        /// Sets the base resolution for dynamic procedural texture baking.
        /// </summary>
        /// <param name="width">Pixel width (e.g., 256, 512, 1024).</param>
        /// <param name="height">Pixel height (e.g., 256, 512, 1024).</param>
        /// <param name="clearExistingCache">If set to <c>true</c>, purges existing GL handles so textures re-bake at the new resolution.</param>
        public void SetProceduralResolution(int width, int height, bool clearExistingCache = false)
        {
            _procWidth = width;
            _procHeight = height;
            _lazyNoiseGen = new NoiseGenerator(width, height);

            if (clearExistingCache)
            {
                foreach (var handle in _bakedProceduralCache.Values)
                {
                    GL.DeleteTexture(handle);
                    _allTrackedHandles.Remove(handle);
                }

                _bakedProceduralCache.Clear();
            }
        }

        /// <summary>
        /// Configures the base resolution and noise algorithm context used for dynamic on-demand baking.
        /// </summary>
        /// <param name="defaultWidth">Default pixel width for generated textures.</param>
        /// <param name="defaultHeight">Default pixel height for generated textures.</param>
        /// <param name="noiseGeneratorInstance">An initialized instance of your custom NoiseGenerator class.</param>
        public void ConfigureLazyBaking(int defaultWidth, int defaultHeight, NoiseGenerator? noiseGeneratorInstance)
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

            if (textureId <= 0)
            {
                return GetFallbackTexture();
            }

            if (!_allTrackedHandles.Contains(textureId))
            {
                return GetFallbackTexture();
            }

            return textureId;
        }

        /// <summary>
        /// Internally intercepts execution to execute a clean math texture compilation on the fly.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Id of texture.</returns>
        /// <exception cref="System.InvalidOperationException">Procedural layout execution halted: No NoiseGenerator instance was initialized or found.</exception>
        private int LazyBakeTexture(int id)
        {
            if (!ProceduralRegistry.TryGetValue(id, out var entry))
                return GetFallbackTexture();

            EnsureNoiseGenerator();

            var rawBuffer = entry.Generator(_procWidth, _procHeight, _lazyNoiseGen!);
            if (rawBuffer == null) return GetFallbackTexture();

            var glHandle = GetTexture(rawBuffer, linearFilter: true);
            _bakedProceduralCache[id] = glHandle;

            return glHandle;
        }

        // --- CORE TEXTURE DRIVER PASSES ---

        /// <summary>
        /// Gets the texture from file system pathing.
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

            _allTrackedHandles.Add(texId);
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

            GL.GetFloat((GetPName)0x84FF, out float maxAniso);
            if (maxAniso > 0.0f)
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FE, Math.Min(maxAniso, 4.0f));
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            _allTrackedHandles.Add(sampleId);
            return sampleId;
        }

        /// <summary>
        /// Gets the fallback texture.
        /// </summary>
        /// <returns>Id of Texture, fallback, checkerboard</returns>
        public int GetFallbackTexture()
        {
            if (_fallbackTextureId > 0) return _fallbackTextureId;

            const int size = 64;
            const int tileSize = 8;
            var pixels = new byte[size * size * 4];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var fx = (x % tileSize) / (float)tileSize;
                    var fy = (y % tileSize) / (float)tileSize;

                    var edgeX = Math.Min(fx, 1.0f - fx) * tileSize;
                    var edgeY = Math.Min(fy, 1.0f - fy) * tileSize;
                    var minEdge = Math.Min(edgeX, edgeY);

                    var smooth = Math.Clamp(minEdge, 0.0f, 1.0f);

                    var isWhiteTile = (((x / tileSize) % 2) ^ ((y / tileSize) % 2)) == 0;

                    var targetColor = isWhiteTile ? 225f : 30f;
                    var centerColor = isWhiteTile ? 30f : 225f;

                    var color =
                        (byte)Math.Clamp(Math.Min(targetColor, centerColor + smooth * (targetColor - centerColor)), 0,
                            255);

                    var idx = (y * size + x) * 4;
                    pixels[idx] = color;
                    pixels[idx + 1] = color;
                    pixels[idx + 2] = color;
                    pixels[idx + 3] = 255;
                }
            }

            _fallbackTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fallbackTextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size, size, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.GetFloat((GetPName)0x84FF, out float maxAniso);
            if (maxAniso > 0.0f)
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)0x84FE, Math.Min(maxAniso, 16.0f));
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            _allTrackedHandles.Add(_fallbackTextureId);
            return _fallbackTextureId;
        }

        /// <summary>
        /// Creates a lookup collection of compiled GPU texture resources directly from raw math arrays.
        /// </summary>
        /// <param name="textures">The procedural textures data map.</param>
        /// <returns>Dictionary of Input id and Id of texture</returns>
        public Dictionary<int, int> CreateMasterTextures(Dictionary<int, UnmanagedImageBuffer?> textures)
        {
            var result = new Dictionary<int, int>();

            foreach (var (id, tex) in textures)
            {
                if (tex == null)
                    throw new ArgumentNullException(nameof(textures), $"Texture {id} is null.");

                var texId = GetTexture(tex, opaqueFastPath: false);
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
            var texId = OpenTkHelper.CreateTexture(buffer, opaqueFastPath);
            _allTrackedHandles.Add(texId);
            return texId;
        }

        // --- SHADER ROUTING INTERFACES ---

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
                case ShaderTypeApp.PhongLighting:
                    vertexSrc = ShaderResource.PhongLightingVertexShader;
                    fragmentSrc = ShaderResource.PhongLightingFragmentShader;
                    break;
                case ShaderTypeApp.Instancing:
                    vertexSrc = ShaderResource.InstancingVertexShader;
                    fragmentSrc = ShaderResource.InstancingFragmentShader;
                    break;
                case ShaderTypeApp.PostProcessing: // Now active for Painterly & Stylized Filters
                    vertexSrc = ShaderResource.PostProcessingVertexShader;
                    fragmentSrc = ShaderResource.PostProcessingFragmentShader;
                    break;
                case ShaderTypeApp.WaterRipple:
                    vertexSrc = ShaderResource.WaterRippleVertexShader;
                    fragmentSrc = ShaderResource.WaterRippleFragmentShader;
                    break;

                case ShaderTypeApp.VolumetricFog:
                    vertexSrc = ShaderResource.VolumetricFogVertexShader;
                    fragmentSrc = ShaderResource.VolumetricFogFragmentShader;
                    break;

                case ShaderTypeApp.TerrainSplat:
                    vertexSrc = ShaderResource.TerrainSplatVertexShader;
                    fragmentSrc = ShaderResource.TerrainSplatFragmentShader;
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
        /// <param name="vertexSource">The vertex source.</param>
        /// <param name="fragmentSource">The fragment source.</param>
        /// <returns>Id of Shader.</returns>
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
        /// <param name="type">The type.</param>
        /// <param name="source">The source.</param>
        /// <returns>Id of Shader.</returns>
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

        // --- Helper methods ---

        /// <summary>
        /// Ensures the noise generator.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Procedural layout execution halted: No NoiseGenerator instance was initialized or found.</exception>
        private void EnsureNoiseGenerator()
        {
            _lazyNoiseGen ??= new NoiseGenerator(_procWidth, _procHeight);
        }

        // --- CLEANUP UNMANAGED ASSET STACK ---

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            foreach (var texId in _allTrackedHandles)
            {
                GL.DeleteTexture(texId);
            }

            foreach (var program in _programCache.Values)
            {
                GL.DeleteProgram(program);
            }

            _textureCache.Clear();
            _bakedProceduralCache.Clear();
            _programCache.Clear();
            _allTrackedHandles.Clear();

            _fallbackTextureId = -1;

            GC.SuppressFinalize(this);
        }
    }
}
