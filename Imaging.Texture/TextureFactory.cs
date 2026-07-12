/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureFactory.cs
 * PURPOSE:     Some predifined texture generation recipes and their associated configuration objects.
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
                RgbRamp = [40, 10, 10, 180, 20, 0, 255, 120, 0, 255, 220, 50]
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
                CenterRgb = [140, 140, 145],
                EdgeRgb = [30, 30, 30]
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
                RgbRamp = [5, 5, 20, 0, 150, 200, 180, 50, 255, 255, 255, 255]
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
    }
}