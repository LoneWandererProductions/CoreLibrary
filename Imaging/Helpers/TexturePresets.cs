/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        TexturePresets.cs
 * PURPOSE:     Some prepared textures that can be used for textures.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging.Enums;
using System.Drawing;

namespace Imaging.Helpers
{
    /// <summary>
    /// Provides ready-to-use procedural texture configurations specifically tailored for RPG environments.
    /// </summary>
    public static class TexturePresets
    {
        /// <summary>
        /// Registers all presets.
        /// </summary>
        public static void RegisterAllPresets()
        {
            ImageRegister.TextureSetting[TextureType.MagicalEther] = GetMagicalEtherSettings();
            ImageRegister.TextureSetting[TextureType.LavaPool] = GetLavaPoolSettings();
            ImageRegister.TextureSetting[TextureType.Cobblestone] = GetCobblestoneSettings();
            ImageRegister.TextureSetting[TextureType.DragonScales] = GetDragonScalesSettings();
        }

        // --- DIRECT USAGE: Instant Bitmaps ---

        /// <summary>
        /// Generates the magical ether.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Bitmap texture, magical ether.</returns>
        public static Bitmap? GenerateMagicalEther(int width, int height)
        {
            var s = GetMagicalEtherSettings();
            return TextureStream.GenerateColorMappedBitmap(width, height, s.ColorRamp, s.TurbulenceSize);
        }

        /// <summary>
        /// Generates the lava pool.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Bitmap texture, lava.</returns>
        public static Bitmap? GenerateLavaPool(int width, int height)
        {
            var s = GetLavaPoolSettings();
            return TextureStream.GenerateColorMappedBitmap(width, height, s.ColorRamp, s.TurbulenceSize);
        }

        /// <summary>
        /// Generates the cobblestone.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Bitmap texture, cobblestone.</returns>
        public static Bitmap? GenerateCobblestone(int width, int height)
        {
            var s = GetCobblestoneSettings();
            return TextureStream.GenerateCellularBitmap(width, height, s.CellSize, 255, s.BaseColor, s.SecondaryColor);
        }

        /// <summary>
        /// Generates the dragon scales.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Bitmap texture, dragonscale.</returns>
        public static Bitmap? GenerateDragonScales(int width, int height)
        {
            var s = GetDragonScalesSettings();
            return TextureStream.GenerateCellularBitmap(width, height, s.CellSize, 255, s.BaseColor, s.SecondaryColor);
        }

        /// <summary>
        /// Gets the magical ether settings.
        /// </summary>
        /// <returns>Texture preset for Magic ether texture.</returns>
        public static TextureConfiguration GetMagicalEtherSettings()
        {
            return new TextureConfiguration
            {
                TurbulenceSize = 128.0,
                // Color mapping: Deep Void -> Cyan -> Bright Magic Purple -> White
                ColorRamp = new Color[]
                {
                    Color.FromArgb(5, 5, 20), Color.FromArgb(0, 150, 200), Color.FromArgb(180, 50, 255), Color.White
                }
            };
        }

        /// <summary>
        /// Gets the lava pool settings.
        /// </summary>
        /// <returns>Texture preset for Lava texture.</returns>
        public static TextureConfiguration GetLavaPoolSettings()
        {
            return new TextureConfiguration
            {
                TurbulenceSize = 32.0,
                // Color mapping: Hardened Magma -> Deep Red -> Bright Orange -> Yellow Hot
                ColorRamp = new Color[]
                {
                    Color.FromArgb(40, 10, 10), Color.FromArgb(180, 20, 0), Color.FromArgb(255, 120, 0),
                    Color.FromArgb(255, 220, 50)
                }
            };
        }

        /// <summary>
        /// Gets the cobblestone settings.
        /// </summary>
        /// <returns>Texture preset for Cobblestone texture.</returns>
        public static TextureConfiguration GetCobblestoneSettings()
        {
            return new TextureConfiguration
            {
                CellSize = 48,
                BaseColor = Color.FromArgb(140, 140, 145), // Stone center
                SecondaryColor = Color.FromArgb(30, 30, 30) // Deep mortar crevices
            };
        }

        /// <summary>
        /// Gets the dragon scales settings.
        /// </summary>
        /// <returns>Texture preset for Dragonscale texture.</returns>
        public static TextureConfiguration GetDragonScalesSettings()
        {
            return new TextureConfiguration
            {
                CellSize = 24,
                BaseColor = Color.FromArgb(20, 80, 40), // Deep green scale body
                SecondaryColor = Color.FromArgb(10, 30, 15) // Dark overlapping edges
            };
        }
    }
}
