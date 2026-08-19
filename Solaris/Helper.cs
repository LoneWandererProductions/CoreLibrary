/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Helper.cs
 * PURPOSE:     Helper class for image processing and map rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 *
 * 1. Viewport Frustum Culling & Scrolling Engine:
 *    - Implement camera clipping bounds for large maps.
 *    - Process only visible tile coordinates during parallel spatial mapping passes.
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Extended.Extensions;
using RenderEngine;
using Brushes = System.Drawing.Brushes;

namespace Solaris
{
    /// <summary>
    ///     Helper class that manages image generation tasks.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Generates the final image based on map and textures.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="map">The map.</param>
        /// <returns>Full Image.</returns>
        internal static UnmanagedImageBuffer GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>>? map)
        {
            var totalWidth = width * textureSize;
            var totalHeight = height * textureSize;

            var canvas = new UnmanagedImageBuffer(totalWidth, totalHeight);
            canvas.Clear(0, 0, 0, 0);

            if (map == null) return canvas;

            // 1. Pre-warm local map textures into the high-speed integer cache
            if (textures != null)
            {
                foreach (var tex in textures.Values)
                {
                    TextureManager.RegisterTexture(tex);
                }
            }

            // 2. High-Speed Memory Map Translation Pass
            var tiles = new ConcurrentBag<UnmanagedTileBox>();

            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is not { Count: > 0 }) return;

                var x = (tile.Key % width) * textureSize;
                var y = (tile.Key / width) * textureSize;

                foreach (var textureId in tile.Value)
                {
                    // --> THE MAGIC HAPPENS HERE <--
                    // Lightning fast integer lookup! No string hashing in the hot loop.
                    var cachedBuffer = TextureManager.GetBufferById(textureId);

                    if (cachedBuffer != null && TextureManager.TryGetTexture(textureId, textures, out var texture))
                    {
                        tiles.Add(new UnmanagedTileBox { X = x, Y = y, Layer = texture.Layer, Buffer = cachedBuffer });
                    }
                }
            });

            var sortedTiles = tiles.ToList();
            sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // 3. High-performance memory alpha-blitting pass
            foreach (var slice in sortedTiles)
            {
                canvas.BlitRegionBlend(
                    slice.Buffer,
                    srcX: 0,
                    srcY: 0,
                    width: slice.Buffer.Width,
                    height: slice.Buffer.Height,
                    destX: slice.X,
                    destY: slice.Y);
            }

            return canvas;
        }

        /// <summary>
        /// Re-blits a single modified tile sub-region into an existing unmanaged canvas buffer.
        /// Avoids full canvas rebuilds during single tile modifications.
        /// </summary>
        /// <param name="canvas">The destination unmanaged canvas layer buffer.</param>
        /// <param name="tileIndex">The 1D spatial tile index to repaint.</param>
        /// <param name="width">The map width in tile units.</param>
        /// <param name="textureSize">The pixel size of individual square tiles.</param>
        /// <param name="textures">The global texture mapping dictionary.</param>
        /// <param name="map">The active tile map data structure.</param>
        internal static void RedrawTileRegion(
            UnmanagedImageBuffer canvas,
            int tileIndex,
            int width,
            int textureSize,
            Dictionary<int, Texture>? textures,
            Dictionary<int, List<int>>? map)
        {
            if (canvas == null || width <= 0 || textureSize <= 0) return;

            var destX = (tileIndex % width) * textureSize;
            var destY = (tileIndex / width) * textureSize;

            // 1. Clear only the bounding box region for this specific tile
            ClearTileRegion(canvas, destX, destY, textureSize);

            if (map == null || !map.TryGetValue(tileIndex, out var textureIds) || textureIds == null || textureIds.Count == 0)
            {
                return;
            }

            // 2. Fetch and layer-sort the textures active at this coordinate
            var tileSlices = new List<UnmanagedTileBox>();
            foreach (var textureId in textureIds)
            {
                var cachedBuffer = TextureManager.GetBufferById(textureId);

                if (cachedBuffer == null && TextureManager.TryGetTexture(textureId, textures, out var texture))
                {
                    TextureManager.RegisterTexture(texture);
                    cachedBuffer = TextureManager.GetBufferById(textureId);
                }

                if (cachedBuffer != null && TextureManager.TryGetTexture(textureId, textures, out var texDef))
                {
                    tileSlices.Add(new UnmanagedTileBox { X = destX, Y = destY, Layer = texDef.Layer, Buffer = cachedBuffer });
                }
            }

            tileSlices.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // 3. Re-blit the stacked layers into the cleared sub-region
            foreach (var slice in tileSlices)
            {
                canvas.BlitRegionBlend(
                    slice.Buffer,
                    srcX: 0,
                    srcY: 0,
                    width: slice.Buffer.Width,
                    height: slice.Buffer.Height,
                    destX: slice.X,
                    destY: slice.Y);
            }
        }

        /// <summary>
        /// Clears a square pixel bounding box within an unmanaged buffer to full transparency.
        /// </summary>
        /// <param name="buffer">The target unmanaged image buffer.</param>
        /// <param name="destX">The starting X coordinate.</param>
        /// <param name="destY">The starting Y coordinate.</param>
        /// <param name="size">The square width and height in pixels.</param>
        private static void ClearTileRegion(UnmanagedImageBuffer buffer, int destX, int destY, int size)
        {
            for (var row = destY; row < destY + size; row++)
            {
                for (var col = destX; col < destX + size; col++)
                {
                    buffer.SetPixelUnsafe(col, row, 0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Generates a grid overlay.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>ImageSource representing the grid overlay.</returns>
        internal static ImageSource GenerateGrid(int width, int height, int textureSize)
        {
            using var bitmap = new Bitmap(width * textureSize, height * textureSize);
            using var graphics = Graphics.FromImage(bitmap);

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    graphics.DrawRectangle(Pens.Black, x * textureSize, y * textureSize, textureSize, textureSize);
                }

            return bitmap.ToBitmapImage();
        }

        /// <summary>
        /// Generates a number overlay.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="padding">The padding.</param>
        /// <returns>ImageSource representing the number overlay.</returns>
        internal static ImageSource GenerateNumbers(int width, int height, int textureSize, int padding = 2)
        {
            using var bitmap = new Bitmap(width * textureSize, height * textureSize);
            using var graphics = Graphics.FromImage(bitmap);
            using var font = new Font(Resources.Font, 8);
            var brush = Brushes.Black;

            var count = 0;

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++, count++)
                {
                    var rect = new RectangleF(
                        (x * textureSize) + padding,
                        (y * textureSize) + padding,
                        textureSize - padding,
                        textureSize - padding);

                    graphics.DrawString(count.ToString(), font, brush, rect);
                }

            return bitmap.ToBitmapImage();
        }

        /// <summary>
        /// Generates a transparent vector overlay layer hosting crisp numbers or Unicode symbols.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="glyphMap">The glyph map.</param>
        /// <returns>Bitmap representing the glyph overlay.</returns>
        internal static Bitmap GenerateGlyphOverlay(
            int width, int height, int textureSize,
            Dictionary<int, OverlayGlyph>? glyphMap)
        {
            var overlayFrame = new Bitmap(width * textureSize, height * textureSize);

            if (glyphMap == null || glyphMap.Count == 0) return overlayFrame;

            using var g = Graphics.FromImage(overlayFrame);
            // THE GRIP: Enable high-fidelity vector text rendering
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // Establish string formatting rules to ensure symbols center perfectly inside the cell block
            using var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            foreach (var kp in glyphMap)
            {
                var tileIndex = kp.Key;
                var glyph = kp.Value;

                if (string.IsNullOrEmpty(glyph.Symbol)) continue;

                // Map flat index location straight into 2D chessboard pixel boundaries
                var cellX = (tileIndex % width) * textureSize;
                var cellY = (tileIndex / width) * textureSize;

                // Calculate the exact destination boundaries of the chessboard cell space
                var targetRect = new RectangleF(cellX, cellY, textureSize, textureSize);

                // Build font family profile dynamically
                var fontStyle = glyph.IsBold ? FontStyle.Bold : FontStyle.Regular;
                using var font = new Font(glyph.FontName, glyph.FontSize, fontStyle);
                using var brush = new SolidBrush(glyph.Color);
                // Draw the crisp vector character straight onto the bitmap buffer plane
                g.DrawString(glyph.Symbol, font, brush, targetRect, sf);
            }

            return overlayFrame;
        }

        /// <summary>
        /// Adds a tile to the map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="idTexture">The identifier texture.</param>
        /// <returns>MapChangeResult representing the result of the operation.</returns>
        internal static MapChangeResult AddTile(
            Dictionary<int, List<int>>? map, KeyValuePair<int, int> idTexture)
        {
            map ??= new Dictionary<int, List<int>>();
            var (key, value) = idTexture;
            var added = map.AddDistinct(key, value);
            return new MapChangeResult(added, map);
        }

        /// <summary>
        /// Removes a tile from the map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="idLayer">The identifier layer.</param>
        /// <returns>MapChangeResult representing the result of the operation.</returns>
        internal static MapChangeResult RemoveTile(
            Dictionary<int, List<int>>? map, Dictionary<int, Texture> textures, KeyValuePair<int, int> idLayer)
        {
            if (map == null || !map.TryGetValue(idLayer.Key, out var tileList))
            {
                return new MapChangeResult(false, map);
            }

            var updatedList = tileList
                .Where(tile => textures[tile].Layer != idLayer.Value)
                .ToList();

            if (updatedList.Count == tileList.Count)
            {
                return new MapChangeResult(false, map);
            }

            if (updatedList.Count == 0)
            {
                map.Remove(idLayer.Key);
            }
            else
            {
                map[idLayer.Key] = updatedList;
            }

            return new MapChangeResult(true, map);
        }

        /// <summary>
        /// Adds a tile image to the display layer.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="idTile">The identifier tile.</param>
        /// <returns>Image on screen</returns>
        public static UnmanagedImageBuffer AddDisplay(
            int width, int textureSize, Dictionary<int, Texture> textures, UnmanagedImageBuffer? layer,
            KeyValuePair<int, int> idTile)
        {
            var (position, tileId) = idTile;
            var x = (position % width) * textureSize;
            var y = (position / width) * textureSize;

            // Attempt fast ID lookup first
            var tileBuffer = TextureManager.GetBufferById(tileId);

            // If it's not cached yet, register it and grab it
            if (tileBuffer == null && TextureManager.TryGetTexture(tileId, textures, out var texture))
            {
                TextureManager.RegisterTexture(texture);
                tileBuffer = TextureManager.GetBufferById(tileId);
            }

            if (layer == null)
            {
                layer = new UnmanagedImageBuffer(width * textureSize, textureSize);
                layer.Clear(0, 0, 0, 0);
            }

            if (tileBuffer == null) return layer;

            layer.BlitRegionBlend(tileBuffer, 0, 0, tileBuffer.Width, tileBuffer.Height, x, y);

            return layer;
        }

        /// <summary>
        /// Removes a tile image from the display layer.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="position">The position.</param>
        /// <returns>Cleaned Image.</returns>
        public static UnmanagedImageBuffer? RemoveDisplay(int width, int textureSize, UnmanagedImageBuffer? layer,
            int position)
        {
            if (layer == null) return null;

            var x = (position % width) * textureSize;
            var y = (position / width) * textureSize;

            for (var row = y; row < y + textureSize; row++)
            {
                for (var col = x; col < x + textureSize; col++)
                {
                    layer.SetPixelUnsafe(col, row, 0, 0, 0, 0);
                }
            }

            return layer;
        }

        /// <summary>
        /// Displays movement animation frame by frame.
        /// </summary>
        /// <param name="aurora">The aurora.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="avatar">The avatar.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        internal static async Task DisplayMovement(Aurora aurora, IEnumerable<int> steps, Bitmap? avatar,
            int width, int height, int textureSize)
        {
            if (avatar == null) return;

            aurora.IsEnabled = false;

            // Pre-convert avatar once to avoid per-frame conversion overhead
            using var avatarBuffer = UnmanagedImageBuffer.FromBitmap(avatar);

            var frameWidth = width * textureSize;
            var frameHeight = height * textureSize;

            foreach (var step in steps)
            {
                var x = (step % width) * textureSize;
                var y = (step / width) * textureSize;

                // 1. Create a fresh, transparent frame for this specific step to prevent "ghost trails"
                using var frame = new UnmanagedImageBuffer(frameWidth, frameHeight);
                frame.Clear(0, 0, 0, 0);

                // 2. Draw the avatar at the current step using unmanaged alpha blending
                frame.BlitRegionBlend(avatarBuffer, 0, 0, avatarBuffer.Width, avatarBuffer.Height, x, y);

                // 3. Push this frame immediately to the UI so we actually see the animation
                using var tempBmp = frame.ToBitmap();
                aurora.LayerThree.Source = tempBmp.ToBitmapImage();

                // 4. Wait before drawing the next frame
                await Task.Delay(100);
            }

            // Optional: Clear the avatar after the animation is entirely done
            // aurora.LayerThree.Source = null;

            aurora.IsEnabled = true;
        }

        /// <summary>
        /// Blits a region from source to destination using fast unsafe alpha blending.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="srcX">The source x.</param>
        /// <param name="srcY">The source y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="destX">The dest x.</param>
        /// <param name="destY">The dest y.</param>
        private static unsafe void BlitRegionBlend(
            this UnmanagedImageBuffer dest,
            UnmanagedImageBuffer src,
            int srcX, int srcY,
            int width, int height,
            int destX, int destY)
        {
            if (width <= 0 || height <= 0) return;

            var srcStride = src.Width * UnmanagedImageBuffer.BytesPerPixel;
            var destStride = dest.Width * UnmanagedImageBuffer.BytesPerPixel;

            var pSrcBase = (byte*)src.Buffer.ToPointer() + (srcY * srcStride) +
                           (srcX * UnmanagedImageBuffer.BytesPerPixel);
            var pDestBase = (byte*)dest.Buffer.ToPointer() + (destY * destStride) +
                            (destX * UnmanagedImageBuffer.BytesPerPixel);

            for (var y = 0; y < height; y++)
            {
                var pSrc = (uint*)(pSrcBase + y * srcStride);
                var pDest = (uint*)(pDestBase + y * destStride);

                for (var x = 0; x < width; x++)
                {
                    var srcPixel = pSrc[x];
                    var alpha = (byte)(srcPixel >> 24);

                    if (alpha == 0) continue; // Skip transparent pixels completely

                    if (alpha == 255)
                    {
                        pDest[x] = srcPixel; // Direct copy for fully opaque pixels
                        continue;
                    }

                    // Perform alpha blending for semi-transparent pixels
                    var dstPixel = pDest[x];

                    var srcB = (byte)(srcPixel & 0xFF);
                    var srcG = (byte)((srcPixel >> 8) & 0xFF);
                    var srcR = (byte)((srcPixel >> 16) & 0xFF);

                    var dstB = (byte)(dstPixel & 0xFF);
                    var dstG = (byte)((dstPixel >> 8) & 0xFF);
                    var dstR = (byte)((dstPixel >> 16) & 0xFF);
                    var dstA = (byte)((dstPixel >> 24) & 0xFF);

                    var invAlpha = 255 - alpha;

                    var outB = (byte)((srcB * alpha + dstB * invAlpha) / 255);
                    var outG = (byte)((srcG * alpha + dstG * invAlpha) / 255);
                    var outR = (byte)((srcR * alpha + dstR * invAlpha) / 255);
                    var outA = (byte)(alpha + (dstA * invAlpha) / 255);

                    pDest[x] = ((uint)outA << 24) | ((uint)outR << 16) | ((uint)outG << 8) | outB;
                }
            }
        }
    }
}
