/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Helper.cs
 * PURPOSE:     Helper class for image processing, map rendering, and viewport frustum culling.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Extended.Extensions;
using RenderEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;

namespace Solaris
{
    /// <summary>
    /// Helper class that manages image generation and spatial rendering tasks.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Generates the final image based on map, textures, and active camera viewport frustum bounds.
        /// </summary>
        /// <param name="width">The map width in tile units.</param>
        /// <param name="height">The map height in tile units.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="map">The map.</param>
        /// <param name="viewport">The active rendering viewport for frustum culling and projection mapping.</param>
        /// <returns>Unmanaged image buffer representing rendered visible canvas.</returns>
        internal static UnmanagedImageBuffer GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>>? map,
            Viewport? viewport = null)
        {
            var canvasWidth = viewport is { ScreenWidth: > 0 } ? viewport.ScreenWidth : width * textureSize;
            var canvasHeight = viewport is { ScreenHeight: > 0 }
                ? viewport.ScreenHeight
                : height * textureSize;

            var canvas = new UnmanagedImageBuffer(canvasWidth, canvasHeight);
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

            // 2. Obtain viewport frustum bounds for spatial culling
            var bounds = viewport?.GetVisibleTileBounds(width, height, textureSize) ??
                         new Rectangle(0, 0, width, height);

            var tiles = new ConcurrentBag<UnmanagedTileBox>();

            // 3. Parallel spatial translation with frustum culling
            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is not { Count: > 0 }) return;

                var tileX = tile.Key % width;
                var tileY = tile.Key / width;

                // Frustum Culling Check: Skip tiles outside current camera view
                if (tileX < bounds.Left || tileX >= bounds.Right || tileY < bounds.Top || tileY >= bounds.Bottom)
                {
                    return;
                }

                var screenPt = viewport?.WorldToScreen(tile.Key, width, textureSize) ??
                               new Point(tileX * textureSize, tileY * textureSize);

                foreach (var textureId in tile.Value)
                {
                    var cachedBuffer = TextureManager.GetBufferById(textureId);

                    if (cachedBuffer != null && TextureManager.TryGetTexture(textureId, textures, out var texture))
                    {
                        tiles.Add(new UnmanagedTileBox
                        {
                            X = screenPt.X, Y = screenPt.Y, Layer = texture.Layer, Buffer = cachedBuffer
                        });
                    }
                }
            });

            var sortedTiles = tiles.ToList();
            sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // 4. High-performance memory alpha-blitting pass
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
        /// Re-blits a single modified tile sub-region into an existing unmanaged canvas buffer using viewport transformation.
        /// </summary>
        /// <param name="canvas">The destination unmanaged canvas layer buffer.</param>
        /// <param name="tileIndex">The 1D spatial tile index to repaint.</param>
        /// <param name="width">The map width in tile units.</param>
        /// <param name="textureSize">The pixel size of individual square tiles.</param>
        /// <param name="textures">The global texture mapping dictionary.</param>
        /// <param name="map">The active tile map data structure.</param>
        /// <param name="viewport">Optional active viewport camera.</param>
        internal static void RedrawTileRegion(
            UnmanagedImageBuffer canvas,
            int tileIndex,
            int width,
            int textureSize,
            Dictionary<int, Texture>? textures,
            Dictionary<int, List<int>>? map,
            Viewport? viewport = null)
        {
            if (canvas == null || width <= 0 || textureSize <= 0) return;

            var destPt = viewport?.WorldToScreen(tileIndex, width, textureSize) ??
                         new Point((tileIndex % width) * textureSize, (tileIndex / width) * textureSize);

            // 1. Clear only the sub-region bounding box
            ClearTileRegion(canvas, destPt.X, destPt.Y, textureSize);

            if (map == null || !map.TryGetValue(tileIndex, out var textureIds) || textureIds == null ||
                textureIds.Count == 0)
            {
                return;
            }

            // 2. Collect and sort layer slices
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
                    tileSlices.Add(new UnmanagedTileBox
                    {
                        X = destPt.X, Y = destPt.Y, Layer = texDef.Layer, Buffer = cachedBuffer
                    });
                }
            }

            tileSlices.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // 3. Re-blit updated region
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
        /// Clears a square pixel bounding box within an unmanaged buffer to full transparency safely clamped to canvas dimensions.
        /// </summary>
        /// <param name="buffer">The target unmanaged image buffer.</param>
        /// <param name="destX">The starting X coordinate.</param>
        /// <param name="destY">The starting Y coordinate.</param>
        /// <param name="size">The square size in pixels.</param>
        private static void ClearTileRegion(UnmanagedImageBuffer buffer, int destX, int destY, int size)
        {
            var startX = System.Math.Max(0, destX);
            var startY = System.Math.Max(0, destY);
            var endX = System.Math.Min(buffer.Width, destX + size);
            var endY = System.Math.Min(buffer.Height, destY + size);

            if (startX >= endX || startY >= endY) return;

            for (var row = startY; row < endY; row++)
            {
                for (var col = startX; col < endX; col++)
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
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            foreach (var kp in glyphMap)
            {
                var tileIndex = kp.Key;
                var glyph = kp.Value;

                if (string.IsNullOrEmpty(glyph.Symbol)) continue;

                var cellX = (tileIndex % width) * textureSize;
                var cellY = (tileIndex / width) * textureSize;

                var targetRect = new RectangleF(cellX, cellY, textureSize, textureSize);

                var fontStyle = glyph.IsBold ? FontStyle.Bold : FontStyle.Regular;
                using var font = new Font(glyph.FontName, glyph.FontSize, fontStyle);
                using var brush = new SolidBrush(glyph.Color);
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

            var tileBuffer = TextureManager.GetBufferById(tileId);

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
        /// Displays movement animation frame by frame and resets temporary frames when finished.
        /// </summary>
        /// <param name="aurora">The aurora control instance.</param>
        /// <param name="steps">The tile steps sequence.</param>
        /// <param name="avatar">The avatar bitmap.</param>
        /// <param name="width">The width in tile units.</param>
        /// <param name="height">The height in tile units.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        internal static async Task DisplayMovement(Aurora aurora, IEnumerable<int> steps, Bitmap? avatar,
            int width, int height, int textureSize)
        {
            if (avatar == null) return;

            aurora.IsEnabled = false;

            using var avatarBuffer = UnmanagedImageBuffer.FromBitmap(avatar);

            var frameWidth = width * textureSize;
            var frameHeight = height * textureSize;

            foreach (var step in steps)
            {
                var x = (step % width) * textureSize;
                var y = (step / width) * textureSize;

                using var frame = new UnmanagedImageBuffer(frameWidth, frameHeight);
                frame.Clear(0, 0, 0, 0);

                frame.BlitRegionBlend(avatarBuffer, 0, 0, avatarBuffer.Width, avatarBuffer.Height, x, y);

                using var tempBmp = frame.ToBitmap();
                aurora.LayerThree.Source = tempBmp.ToBitmapImage();

                await Task.Delay(100);
            }

            // Cleanly restore LayerThree to its static display buffer or null
            aurora.LayerThree.Source =
                aurora.BitmapLayerThree?.UpdateWriteableBitmap(aurora.LayerThree.Source as WriteableBitmap);

            aurora.IsEnabled = true;
        }

        /// <summary>
        /// Blits a region from source to destination using fast unsafe alpha blending with full boundary clipping guards.
        /// </summary>
        /// <param name="dest">The destination unmanaged image buffer.</param>
        /// <param name="src">The source unmanaged image buffer.</param>
        /// <param name="srcX">The source X offset.</param>
        /// <param name="srcY">The source Y offset.</param>
        /// <param name="width">The region width in pixels.</param>
        /// <param name="height">The region height in pixels.</param>
        /// <param name="destX">The destination X offset.</param>
        /// <param name="destY">The destination Y offset.</param>
        private static unsafe void BlitRegionBlend(
            this UnmanagedImageBuffer dest,
            UnmanagedImageBuffer src,
            int srcX, int srcY,
            int width, int height,
            int destX, int destY)
        {
            if (dest == null || src == null) return;

            // --- DESTINATION CLIPPING GUARDS ---
            if (destX < 0)
            {
                var shift = -destX;
                srcX += shift;
                width -= shift;
                destX = 0;
            }

            if (destY < 0)
            {
                var shift = -destY;
                srcY += shift;
                height -= shift;
                destY = 0;
            }

            if (destX + width > dest.Width)
            {
                width = dest.Width - destX;
            }

            if (destY + height > dest.Height)
            {
                height = dest.Height - destY;
            }

            // --- SOURCE CLIPPING GUARDS ---
            if (srcX < 0)
            {
                var shift = -srcX;
                destX += shift;
                width -= shift;
                srcX = 0;
            }

            if (srcY < 0)
            {
                var shift = -srcY;
                destY += shift;
                height -= shift;
                srcY = 0;
            }

            if (srcX + width > src.Width)
            {
                width = src.Width - srcX;
            }

            if (srcY + height > src.Height)
            {
                height = src.Height - srcY;
            }

            // Return immediately if clipped region is entirely off-screen or empty
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

                    if (alpha == 0) continue;

                    if (alpha == 255)
                    {
                        pDest[x] = srcPixel;
                        continue;
                    }

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
