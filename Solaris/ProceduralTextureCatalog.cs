/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        ProceduralTextureCatalog.cs
 * PURPOSE:     Centralized catalog for built-in procedural textures with unique identifiers starting at 10000, allowing for efficient registration and rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using Imaging.Texture;
using RenderEngine;

namespace Solaris
{
    /// <summary>
    /// Static class ProceduralTextureCatalog provides a catalog of built-in procedural textures with unique identifiers starting at 10000.
    /// It allows for the initialization and registration of these textures into high-speed memory for efficient rendering.
    /// </summary>
    public static class ProceduralTextureCatalog
    {
        /// <summary>
        /// ProceduralTile enum defines the IDs for built-in procedural textures starting at 10000.
        /// </summary>
        public enum ProceduralTile
        {
            LavaPool = 10000,
            Cobblestone = 10001,
            MagicalEther = 10002,
            CrackedIce = 10003,
            MagicPortal = 10004,
            PlasmaArc = 10005,
            TreeBark = 10006,
            Foliage = 10007,
            WoodPlank = 10008,
            Stones = 10009
        }

        /// <summary>
        /// Precalculates built-in procedural textures into high-speed memory starting at ID 10000.
        /// </summary>
        /// <param name="textureSize">Size of the texture.</param>
        public static void InitializePrecalculatedTextures(int textureSize = 64)
        {
            var noise = new NoiseGenerator(textureSize, textureSize);

            Register(ProceduralTile.LavaPool, TextureFactory.GenerateLavaPool(textureSize, textureSize, noise), 0);
            Register(ProceduralTile.Cobblestone, TextureFactory.GenerateCobblestone(textureSize, textureSize), 0);
            Register(ProceduralTile.MagicalEther, TextureFactory.GenerateMagicalEther(textureSize, textureSize, noise),
                0);
            Register(ProceduralTile.CrackedIce, TextureFactory.GenerateCrackedIce(textureSize, textureSize), 0);
            Register(ProceduralTile.MagicPortal, TextureFactory.GenerateMagicPortal(textureSize, textureSize, noise),
                0);
            Register(ProceduralTile.PlasmaArc, TextureFactory.GeneratePlasmaArc(textureSize, textureSize, noise), 0);
            Register(ProceduralTile.TreeBark, TextureFactory.GenerateTreeBark(textureSize, textureSize, noise), 0);
            Register(ProceduralTile.Foliage, TextureFactory.GenerateFoliage(textureSize, textureSize, noise), 0);
            Register(ProceduralTile.WoodPlank, TextureFactory.GenerateWoodPlank(textureSize, textureSize, noise), 0);
            Register(ProceduralTile.Stones, TextureFactory.GenerateStoneTexture(textureSize, textureSize, noise), 0);
        }

        /// <summary>
        /// Registers the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <param name="rawBuffer">The raw buffer.</param>
        /// <param name="layer">The layer.</param>
        private static void Register(ProceduralTile tile, RawTextureBuffer? rawBuffer, int layer)
        {
            // Allocate unmanaged memory matching dimensions
            var buffer = new UnmanagedImageBuffer(rawBuffer.Width, rawBuffer.Height);

            // Copy BGRA bytes directly from managed RawTextureBuffer to unmanaged memory
            rawBuffer.AsSpan().CopyTo(buffer.BufferSpan);

            TextureManager.RegisterGeneratedTexture((int)tile, buffer, layer);
        }
    }
}
