/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        TextureManager.cs
 * PURPOSE:     Helper class for managing textures and image caching.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Imaging;

namespace Solaris
{
    /// <summary>
    /// Texture Manager.
    /// </summary>
    public static class TextureManager
    {
        // Tier 1: Deduplicates hard drive reads (Path -> Bitmap)
        /// <summary>
        /// The file cache.
        /// Maps a file path to the actual Bitmap in memory, preventing duplicate I/O.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Bitmap> FileCache = new();

        // Tier 2: Lightning fast rendering lookups (Id -> Bitmap)
        /// <summary>
        /// The fast render cache.
        /// Maps a Texture ID directly to the memory reference for O(1) rendering lookups.
        /// </summary>
        private static readonly ConcurrentDictionary<int, Bitmap> FastRenderCache = new();

        /// <summary>
        /// The global textures
        /// Maps our "High IDs" to their Texture definitions
        /// </summary>
        private static readonly ConcurrentDictionary<int, Texture> GlobalTextures = new();

        /// <summary>
        /// The render
        /// </summary>
        private static readonly ImageRender Render = new();

        /// <summary>
        /// Registers a texture (Global or Map-specific) into the high-speed caches.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public static void RegisterTexture(Texture texture)
        {
            if (string.IsNullOrWhiteSpace(texture.Path)) return;

            // 1. Load from disk only once
            var bmp = FileCache.GetOrAdd(texture.Path, p => Render.GetBitmapFile(p));

            // 2. Map the ID directly to the memory reference for O(1) int lookups
            if (bmp != null)
            {
                FastRenderCache[texture.Id] = bmp;
            }
        }

        /// <summary>
        /// Call this during application startup/loading screens to preload global assets.
        /// </summary>
        /// <param name="globalId">The global identifier.</param>
        /// <param name="path">The path.</param>
        /// <param name="layer">The layer.</param>
        public static void PreloadGlobalAsset(int globalId, string path, int layer)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            var tex = new Texture { Id = globalId, Path = path, Layer = layer };

            GlobalTextures[globalId] = tex;
            RegisterTexture(tex);
        }

        /// <summary>
        /// Retrieves a Texture definition. Falls back to Global if not in map specifics.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mapTextures">The map textures.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>True if the texture was found; otherwise, false.</returns>
        public static bool TryGetTexture(int id, Dictionary<int, Texture> mapTextures, out Texture texture)
        {
            // Check map-specific textures first
            if (mapTextures != null && mapTextures.TryGetValue(id, out texture))
                return true;

            // Fallback to preloaded global textures (your negative/high IDs)
            return GlobalTextures.TryGetValue(id, out texture);
        }

        /// <summary>
        /// Safely gets a bitmap by ID for ultra-fast rendering loop lookups.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The bitmap if found; otherwise, null.</returns>
        public static Bitmap? GetBitmapById(int id)
        {
            return FastRenderCache.TryGetValue(id, out var bmp) ? bmp : null;
        }

        /// <summary>
        /// Safely gets or loads a bitmap by path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The bitmap if found or loaded; otherwise, null.</returns>
        public static Bitmap? GetBitmapByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            return FileCache.GetOrAdd(path, p => Render.GetBitmapFile(p));
        }

        /// <summary>
        /// Optional: Clear map-specific caches if memory gets too high,
        /// but keep global ones alive.
        /// </summary>
        public static void FlushNonGlobalCaches()
        {
            // Logic to remove anything from caches that isn't referenced by GlobalTextures
        }
    }
}
