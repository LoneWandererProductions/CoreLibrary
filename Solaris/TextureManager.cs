/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        TextureManager.cs
 * PURPOSE:     Helper class for managing textures and unmanaged image caching.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using Imaging.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Solaris
{
    /// <summary>
    /// Texture Manager using unmanaged memory buffers.
    /// </summary>
    public static class TextureManager
    {
        /// <summary>
        /// The file cache
        ///  Tier 1: Deduplicates hard drive reads (Path -> Bitmap)
        /// </summary>
        private static readonly ConcurrentDictionary<string, Bitmap> FileCache = new();

        /// <summary>
        /// The iso render buffer cache
        /// </summary>
        private static readonly ConcurrentDictionary<int, UnmanagedImageBuffer> IsoRenderBufferCache = new();

        // 
        /// <summary>
        /// The fast render buffer cache
        /// Tier 2: Unmanaged render cache for O(1) row blitting lookups (Id -> UnmanagedImageBuffer)
        /// </summary>
        private static readonly ConcurrentDictionary<int, UnmanagedImageBuffer> FastRenderBufferCache = new();

        /// <summary>
        /// The global textures mapping IDs to Texture definitions.
        /// </summary>
        private static readonly ConcurrentDictionary<int, Texture> GlobalTextures = new();

        /// <summary>
        /// Registers a texture from disk into the fast unmanaged memory cache.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public static void RegisterTexture(Texture texture)
        {
            if (string.IsNullOrWhiteSpace(texture.Path)) return;

            // 1. Load from disk only once
            var bmp = FileCache.GetOrAdd(texture.Path, LoadBitmapFromFile);

            // 2. Convert to UnmanagedImageBuffer for O(1) unmanaged blitting
            if (bmp != null && !FastRenderBufferCache.ContainsKey(texture.Id))
            {
                FastRenderBufferCache[texture.Id] = UnmanagedImageBuffer.FromBitmap(bmp);
            }
        }

        /// <summary>
        /// Registers a procedurally generated UnmanagedImageBuffer directly into the cache.
        /// </summary>
        /// <param name="id">The texture ID (e.g., 10000+).</param>
        /// <param name="buffer">The unmanaged buffer.</param>
        /// <param name="layer">The layer index.</param>
        /// <param name="isStandingSprite">if set to <c>true</c> [is standing sprite].</param>
        /// <param name="anchorX">The anchor x.</param>
        /// <param name="anchorY">The anchor y.</param>
        /// <param name="heightOffset">The height offset.</param>
        public static void RegisterGeneratedTexture(
            int id,
            UnmanagedImageBuffer buffer,
            int layer = 0,
            bool isStandingSprite = false,
            int? anchorX = null,
            int? anchorY = null,
            int heightOffset = 0)
        {
            var tex = new Texture
            {
                Id = id,
                Path = $"[Procedural:{id}]",
                Layer = layer,
                IsStandingSprite = isStandingSprite,
                AnchorX = anchorX,
                AnchorY = anchorY,
                HeightOffset = heightOffset
            };
            GlobalTextures[id] = tex;
            FastRenderBufferCache[id] = buffer;
        }

        /// <summary>
        /// Call this during application startup/loading screens to preload global disk assets.
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
        /// Safely gets an UnmanagedImageBuffer by ID for ultra-fast rendering loop lookups.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The unmanaged buffer if found; otherwise, null.</returns>
        public static UnmanagedImageBuffer? GetBufferById(int id, bool isometric = false)
        {
            if (!isometric)
            {
                return FastRenderBufferCache.TryGetValue(id, out var buffer) ? buffer : null;
            }

            // Upright sprites keep their original rectangular buffer and transparency
            if (GlobalTextures.TryGetValue(id, out var tex) && tex.IsStandingSprite)
            {
                return FastRenderBufferCache.TryGetValue(id, out var buffer) ? buffer : null;
            }

            return IsoRenderBufferCache.GetOrAdd(id, key =>
            {
                if (FastRenderBufferCache.TryGetValue(key, out var raw))
                {
                    return CutIsometricDiamond(raw, raw.Width, raw.Height);
                }
                return null!;
            });
        }

        /// <summary>
        /// Retrieves a Texture definition. Falls back to Global if not in map specifics.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mapTextures">The map textures.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>True if the texture was found; otherwise, false.</returns>
        public static bool TryGetTexture(int id, Dictionary<int, Texture>? mapTextures, out Texture texture)
        {
            if (mapTextures != null && mapTextures.TryGetValue(id, out texture))
                return true;

            return GlobalTextures.TryGetValue(id, out texture);
        }

        /// <summary>
        /// Cuts the isometric diamond.
        /// </summary>
        /// <param name="squareBuffer">The square buffer.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The isometric diamond buffer.</returns>
        public static UnmanagedImageBuffer CutIsometricDiamond(UnmanagedImageBuffer squareBuffer, int width, int height)
        {
            int isoHeight = height / 2;
            var diamondBuffer = new UnmanagedImageBuffer(width, isoHeight);
            diamondBuffer.Clear(0, 0, 0, 0);

            float halfW = width / 2f;
            float halfH = isoHeight / 2f;

            for (int y = 0; y < isoHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Math.Abs(x + 0.5f - halfW) / halfW;
                    float dy = Math.Abs(y + 0.5f - halfH) / halfH;

                    if (dx + dy <= 1.0f)
                    {
                        int srcY = Math.Min(height - 1, (int)((y / (float)isoHeight) * height));
                        int srcX = Math.Min(width - 1, x);

                        var (r, g, b, a) = squareBuffer.GetPixel(srcX, srcY);
                        diamondBuffer.SetPixelUnsafe(x, y, r, g, b, a);
                    }
                }
            }

            return diamondBuffer;
        }

        /// <summary>
        /// Flushes unmanaged texture buffers to prevent memory leaks.
        /// </summary>
        public static void FlushCaches()
        {
            foreach (var buffer in FastRenderBufferCache.Values)
            {
                buffer.Dispose();
            }

            FastRenderBufferCache.Clear();

            foreach (var bmp in FileCache.Values)
            {
                bmp.Dispose();
            }

            FileCache.Clear();
            GlobalTextures.Clear();
        }

        /// <summary>
        /// Safely loads a bitmap from disk using FileShare.ReadWrite to avoid locking files on disk.
        /// </summary>
        /// <param name="path">The image file path.</param>
        /// <returns>
        /// A new Bitmap instance or null if the file does not exist.
        /// </returns>
        private static Bitmap? LoadBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var temp = new Bitmap(stream);
            return new Bitmap(temp);
        }
    }
}
