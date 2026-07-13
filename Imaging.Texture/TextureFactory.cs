/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureFactory.cs
 * PURPOSE:     Some predifined texture generation recipes and their associated configuration objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Runtime.CompilerServices;

namespace Imaging.Texture
{
    /// <summary>
    /// Owns the definitions and generation of specific textures.
    /// Fully isolated from the rest of the application.
    /// </summary>
    public static class TextureFactory
    {
        // --- RECIPES (CONFIGURATIONS) ---

        /// <summary>
        /// Gets the lava pool configuration.
        /// </summary>
        /// <returns>The lava pool configuration.</returns>
        public static TextureConfig GetLavaPoolConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0,
                // Stored purely as flat [R, G, B] bytes for the math engine
                RgbRamp =  [40, 10, 10, 180, 20, 0, 255, 120, 0, 255, 220, 50]
            };
        }

        /// <summary>
        /// Gets the cobblestone configuration.
        /// </summary>
        /// <returns>The cobblestone configuration.</returns>
        public static TextureConfig GetCobblestoneConfig()
        {
            return new TextureConfig
            {
                CellSize = 48,
                CenterRgb =  [140, 140, 145],
                EdgeRgb =  [30, 30, 30]
            };
        }

        /// <summary>
        /// Gets the magical ether configuration.
        /// </summary>
        /// <returns>The magical ether configuration.</returns>
        public static TextureConfig GetMagicalEtherConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 128.0,
                // Flat [R, G, B] bytes for: Dark Navy -> Deep Blue -> Bright Purple -> White
                RgbRamp =  [5, 5, 20, 0, 150, 200, 180, 50, 255, 255, 255, 255]
            };
        }

        /// <summary>
        /// Gets the cracked ice configuration.
        /// </summary>
        /// <returns>The cracked ice configuration.</returns>
        public static TextureConfig GetCrackedIceConfig()
        {
            return new TextureConfig
            {
                CellSize = 64,
                CenterRgb =  [230, 245, 255], // Bright icy white for the sharp ridges
                EdgeRgb =  [10, 40, 80] // Deep water blue for the flat cells
            };
        }

        /// <summary>
        /// Gets the magic portal configuration.
        /// </summary>
        /// <returns>The magic portal configuration.</returns>
        public static TextureConfig GetMagicPortalConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 64.0,
                WarpScale = 128.0,
                WarpStrength = 4.0, // High strength creates deep liquid swirls
                RgbRamp =  [0, 0, 10, 40, 10, 120, 150, 40, 255, 255, 200, 255]
            };
        }

        /// <summary>
        /// Gets the plasma arc configuration.
        /// </summary>
        /// <returns>The plasma arc configuration.</returns>
        public static TextureConfig GetPlasmaArcConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 128.0,
                Octaves = 5,
                Persistence = 0.5,
                // Black background -> dark purple -> bright cyan -> white hot core
                RgbRamp =  [0, 0, 0, 40, 0, 80, 0, 200, 255, 255, 255, 255]
            };
        }

        /// <summary>
        /// Gets the furrowed tree bark configuration.
        /// </summary>
        /// <returns>The tree bark configuration.</returns>
        public static TextureConfig GetTreeBarkConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 16.0,       // Maps to horizontal frequency scale
                WarpStrength = 12.0,         // Grain twisting displacement power
                CenterRgb = [140, 95, 55],   // Bright ridge wood highlight color
                EdgeRgb = [75, 45, 25]       // Dark deep furrow crease color
            };
        }

        /// <summary>
        /// Gets the leaf foliage configuration.
        /// </summary>
        /// <returns>The leaf foliage configuration.</returns>
        public static TextureConfig GetFoliageConfig()
        {
            return new TextureConfig
            {
                CellSize = 40,
                CenterRgb = [34, 110, 24],   // Primary outer leaf green color
                EdgeRgb = [12, 35, 10]       // Deep background ambient shadow drop color
            };
        }

        /// <summary>
        /// Gets the wooden plank board configuration.
        /// </summary>
        /// <returns>The wooden plank board configuration.</returns>
        public static TextureConfig GetWoodPlankConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0,
                WarpStrength = 0.15,         // Reusing WarpStrength for internal Engine TurbulencePower
                CenterRgb = [130, 85, 45],   // Base board brown
                EdgeRgb = [70, 40, 20]       // Dark grain accent lines
            };
        }

        // --- GENERATORS ---

        /// <summary>
        /// Generates the lava pool.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <returns>The generated raw texture buffer. Here lava pool.</returns>
        public static RawTextureBuffer GenerateLavaPool(int width, int height, object noiseGenInstance)
        {
            var config = GetLavaPoolConfig();

            return TextureMathEngine.GenerateColorMapped(
                width,
                height,
                noiseGenInstance,
                config.RgbRamp,
                config.TurbulenceSize,
                255);
        }

        /// <summary>
        /// Generates the cobblestone.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The generated raw texture buffer. Here cobblestone.</returns>
        public static RawTextureBuffer GenerateCobblestone(int width, int height)
        {
            var config = GetCobblestoneConfig();

            return TextureMathEngine.GenerateCellular(
                width,
                height,
                config.CellSize,
                255,
                config.CenterRgb[0], config.CenterRgb[1], config.CenterRgb[2],
                config.EdgeRgb[0], config.EdgeRgb[1], config.EdgeRgb[2]
            );
        }

        /// <summary>
        /// Generates the magical ether.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <returns>The generated raw texture buffer. Here magical ether.</returns>
        public static RawTextureBuffer GenerateMagicalEther(int width, int height, object noiseGenInstance)
        {
            var config = GetMagicalEtherConfig();

            return TextureMathEngine.GenerateColorMapped(
                width,
                height,
                noiseGenInstance,
                config.RgbRamp,
                config.TurbulenceSize,
                255);
        }

        /// <summary>
        /// Generates the cracked ice.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The generated raw texture buffer. Here cracked ice.</returns>
        public static RawTextureBuffer GenerateCrackedIce(int width, int height)
        {
            var config = GetCrackedIceConfig();
            // Calls the engine method containing the F2-F1 math
            return TextureMathEngine.GenerateAdvancedCellular(
                width, height, config.CellSize, 255,
                config.CenterRgb, config.EdgeRgb);
        }

        /// <summary>
        /// Generates the magic portal.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen.</param>
        /// <returns>The generated raw texture buffer. Here magic portal.</returns>
        public static RawTextureBuffer GenerateMagicPortal(int width, int height, object noiseGen)
        {
            var config = GetMagicPortalConfig();
            // Calls the engine method containing the warping math
            return TextureMathEngine.GenerateWarpedMapped(
                width, height, noiseGen, config.RgbRamp,
                config.TurbulenceSize, config.WarpScale, config.WarpStrength, 255);
        }

        /// <summary>
        /// Generates the plasma arc.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen.</param>
        /// <returns>The generated raw texture buffer. Here plasma arc.</returns>
        public static RawTextureBuffer GeneratePlasmaArc(int width, int height, object noiseGen)
        {
            var config = GetPlasmaArcConfig();
            // Calls the engine method containing the ridged math
            return TextureMathEngine.GenerateRidgedMapped(
                width, height, noiseGen, config.RgbRamp,
                config.TurbulenceSize, config.Octaves, config.Persistence, 255);
        }

        /// <summary>
        /// Generates the furrowed organic tree bark.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen instance wrapper.</param>
        /// <returns>The generated raw texture buffer containing tree bark.</returns>
        public static RawTextureBuffer GenerateTreeBark(int width, int height, object noiseGen)
        {
            var config = GetTreeBarkConfig();

            return TextureMathEngine.GenerateTreeBark(
                width,
                height,
                noiseGen,
                255,
                config.TurbulenceSize,
                70.0, // Stretched macro Y-scaling baseline footprint
                config.WarpStrength,
                config.EdgeRgb[0], config.EdgeRgb[1], config.EdgeRgb[2],
                config.CenterRgb[0], config.CenterRgb[1], config.CenterRgb[2]
            );
        }

        /// <summary>
        /// Generates the interlocking leaf foliage canopy.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The generated raw texture buffer containing leaf foliage.</returns>
        public static RawTextureBuffer GenerateFoliage(int width, int height)
        {
            var config = GetFoliageConfig();

            return TextureMathEngine.GenerateFoliage(
                width,
                height,
                config.CellSize,
                255,
                config.CenterRgb[0], config.CenterRgb[1], config.CenterRgb[2],
                config.EdgeRgb[0], config.EdgeRgb[1], config.EdgeRgb[2]
            );
        }

        /// <summary>
        /// Generates a longitudinal sawn wood board texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen instance wrapper.</param>
        /// <returns>The generated raw texture buffer containing a wood plank.</returns>
        public static RawTextureBuffer GenerateWoodPlank(int width, int height, object noiseGen)
        {
            var config = GetWoodPlankConfig();

            return TextureMathEngine.GenerateWoodPlank(
                width,
                height,
                noiseGen,
                255,
                6.0,
                config.WarpStrength,
                config.TurbulenceSize,
                config.CenterRgb[0], config.CenterRgb[1], config.CenterRgb[2],
                config.EdgeRgb[0], config.EdgeRgb[1], config.EdgeRgb[2]
            );
        }
    }
}
