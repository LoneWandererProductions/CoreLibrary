/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureFactory.cs
 * PURPOSE:     Some predefined texture generation recipes and their associated configuration objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    /// Owns the definitions and generation of specific textures.
    /// Fully isolated from the rest of the application.
    /// </summary>
    public static class TextureFactory
    {
        // --- GENERATORS ---

        /// <summary>
        /// Generates the lava pool.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here lava pool.
        /// </returns>
        public static RawTextureBuffer? GenerateLavaPool(int width, int height, object noiseGenInstance,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetLavaPoolConfig();

            return TextureMathEngine.GenerateColorMapped(
                width,
                height,
                noiseGenInstance,
                activeConfig.RgbRamp,
                activeConfig.TurbulenceSize,
                255);
        }

        /// <summary>
        /// Generates the cobblestone.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here cobblestone.
        /// </returns>
        public static RawTextureBuffer? GenerateCobblestone(int width, int height, TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetCobblestoneConfig();

            return TextureMathEngine.GenerateCellular(
                width,
                height,
                activeConfig.CellSize,
                255,
                activeConfig.CenterRgb[0], activeConfig.CenterRgb[1], activeConfig.CenterRgb[2],
                activeConfig.EdgeRgb[0], activeConfig.EdgeRgb[1], activeConfig.EdgeRgb[2]
            );
        }

        /// <summary>
        /// Generates the magical ether.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here magical ether.
        /// </returns>
        public static RawTextureBuffer? GenerateMagicalEther(int width, int height, object noiseGenInstance,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetMagicalEtherConfig();

            return TextureMathEngine.GenerateColorMapped(
                width,
                height,
                noiseGenInstance,
                activeConfig.RgbRamp,
                activeConfig.TurbulenceSize,
                255);
        }

        /// <summary>
        /// Generates the cracked ice.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here cracked ice.
        /// </returns>
        public static RawTextureBuffer? GenerateCrackedIce(int width, int height, TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetCrackedIceConfig();
            // Calls the engine method containing the F2-F1 math
            return TextureMathEngine.GenerateAdvancedCellular(
                width, height, activeConfig.CellSize, 255,
                activeConfig.CenterRgb, activeConfig.EdgeRgb);
        }

        /// <summary>
        /// Generates the magic portal.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here magic portal.
        /// </returns>
        public static RawTextureBuffer? GenerateMagicPortal(int width, int height, object noiseGen,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetMagicPortalConfig();
            // Calls the engine method containing the warping math
            return TextureMathEngine.GenerateWarpedMapped(
                width, height, noiseGen, activeConfig.RgbRamp,
                activeConfig.TurbulenceSize, activeConfig.WarpScale, activeConfig.WarpStrength, 255);
        }

        /// <summary>
        /// Generates the plasma arc.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer. Here plasma arc.
        /// </returns>
        public static RawTextureBuffer? GeneratePlasmaArc(int width, int height, object noiseGen,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetPlasmaArcConfig();
            // Calls the engine method containing the ridged math
            return TextureMathEngine.GenerateRidgedMapped(
                width, height, noiseGen, activeConfig.RgbRamp,
                activeConfig.TurbulenceSize, activeConfig.Octaves, activeConfig.Persistence, 255);
        }

        /// <summary>
        /// Generates the furrowed organic tree bark.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen instance wrapper.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer containing tree bark.
        /// </returns>
        public static RawTextureBuffer? GenerateTreeBark(int width, int height, object noiseGen,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetTreeBarkConfig();

            return TextureMathEngine.GenerateTreeBark(
                width,
                height,
                noiseGen,
                255,
                activeConfig.TurbulenceSize,
                70.0, // Stretched macro Y-scaling baseline footprint
                activeConfig.WarpStrength,
                activeConfig.EdgeRgb[0], activeConfig.EdgeRgb[1], activeConfig.EdgeRgb[2],
                activeConfig.CenterRgb[0], activeConfig.CenterRgb[1], activeConfig.CenterRgb[2]
            );
        }

        /// <summary>
        /// Generates the interlocking leaf foliage canopy.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer containing leaf foliage.
        /// </returns>
        public static RawTextureBuffer? GenerateFoliage(int width, int height, object noiseGen,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetFoliageConfig();

            return TextureMathEngine.GenerateFoliage(
                width,
                height,
                noiseGen,
                activeConfig.CellSize,
                255,
                activeConfig.CenterRgb[0], activeConfig.CenterRgb[1], activeConfig.CenterRgb[2],
                activeConfig.EdgeRgb[0], activeConfig.EdgeRgb[1], activeConfig.EdgeRgb[2]
            );
        }

        /// <summary>
        /// Generates a longitudinal sawn wood board texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGen">The noise gen instance wrapper.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The generated raw texture buffer containing a wood plank.
        /// </returns>
        public static RawTextureBuffer? GenerateWoodPlank(int width, int height, object noiseGen,
            TextureConfig? config = null)
        {
            var activeConfig = config ?? TextureConstants.GetWoodPlankConfig();

            return TextureMathEngine.GenerateWoodPlank(
                width,
                height,
                noiseGen,
                255,
                6.0,
                activeConfig.WarpStrength,
                activeConfig.TurbulenceSize,
                activeConfig.CenterRgb[0], activeConfig.CenterRgb[1], activeConfig.CenterRgb[2],
                activeConfig.EdgeRgb[0], activeConfig.EdgeRgb[1], activeConfig.EdgeRgb[2]
            );
        }

        /// <summary>
        /// Generates a directional shaded stone texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="fillArea">If false, the deep mortar lines become fully transparent.</param>
        /// <returns>
        /// The generated raw texture buffer.
        /// </returns>
        public static RawTextureBuffer? GenerateStoneTexture(int width, int height, object noiseGenInstance,
            TextureConfig? config = null, bool fillArea = false)
        {
            var activeConfig = config ?? TextureConstants.GetStoneConfig();

            return TextureMathEngine.GenerateDirectionalStone(
                width,
                height,
                noiseGenInstance,
                activeConfig.RgbRamp,
                activeConfig.VoronoiGridSize,
                fillArea);
        }
    }
}