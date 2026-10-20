/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Helper.cs
 * PURPOSE:     Helper class for image processing, map rendering, and viewport frustum culling.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Extended.Extensions;
using Imaging.Objects;
using System;
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
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>
        /// Unmanaged image buffer representing rendered visible canvas.
        /// </returns>
        internal static UnmanagedImageBuffer GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>>? map,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            var canvasWidth = viewport is { ScreenWidth: > 0 } ? viewport.ScreenWidth : width * textureSize;
            var canvasHeight = viewport is { ScreenHeight: > 0 } ? viewport.ScreenHeight : height * textureSize;

            var canvas = new UnmanagedImageBuffer(canvasWidth, canvasHeight);
            canvas.Clear(0, 0, 0, 0);

            if (map == null) return canvas;

            if (textures != null)
            {
                foreach (var tex in textures.Values)
                {
                    TextureManager.RegisterTexture(tex);
                }
            }

            var bounds = viewport?.GetVisibleTileBounds(width, height, textureSize) ??
                         new Rectangle(0, 0, width, height);

            var isIsometric = viewport?.Mode == ProjectionMode.Isometric;
            var tiles = new ConcurrentBag<UnmanagedTileBox>();

            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is not { Count: > 0 }) return;

                var tileX = tile.Key % width;
                var tileY = tile.Key / width;

                if (tileX < bounds.Left || tileX >= bounds.Right || tileY < bounds.Top || tileY >= bounds.Bottom)
                {
                    return;
                }

                int elev = elevationMap != null && elevationMap.TryGetValue(tile.Key, out int z) ? z : 0;

                var screenPt = viewport?.WorldToScreen(tile.Key, width, textureSize, elev, stepHeight) ??
                               new Point(tileX * textureSize, tileY * textureSize);

                foreach (var textureId in tile.Value)
                {
                    // Automatically fetches the snugly masked diamond in isometric mode
                    var cachedBuffer = TextureManager.GetBufferById(textureId, isIsometric);

                    if (cachedBuffer != null && TextureManager.TryGetTexture(textureId, textures, out var texture))
                    {
                        tiles.Add(new UnmanagedTileBox
                        {
                            X = screenPt.X,
                            Y = screenPt.Y,
                            Layer = texture.Layer,
                            Buffer = cachedBuffer,
                            TileX = tileX,
                            TileY = tileY
                        });
                    }
                }
            });

            var sortedTiles = tiles.ToList();

            // Back-to-front depth sorting (Painter's Algorithm) for isometric views; otherwise, standard layer sorting
            if (isIsometric)
            {
                sortedTiles.Sort((a, b) =>
                {
                    int depthA = a.TileX + a.TileY;
                    int depthB = b.TileX + b.TileY;
                    int cmp = depthA.CompareTo(depthB);
                    return cmp != 0 ? cmp : a.Layer.CompareTo(b.Layer);
                });
            }
            else
            {
                sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));
            }

            int halfW = textureSize / 2;
            int halfH = textureSize / 4;
            var renderedWalls = new HashSet<int>();

            foreach (var slice in sortedTiles)
            {
                int tileId = slice.TileY * width + slice.TileX;
                int elev = elevationMap != null && elevationMap.TryGetValue(tileId, out int z) ? z : 0;

                // 1. Draw walls for elevated tiles below the top surface
                if (isIsometric && elev > 0 && renderedWalls.Add(tileId))
                {
                    if (map.TryGetValue(tileId, out var texList) && texList.Count > 0)
                    {
                        // Use the original, square texture for the vertical wall
                        var rawWallBuffer = TextureManager.GetBufferById(texList[0], isometric: false);
                        if (rawWallBuffer != null)
                        {
                            DrawIsometricWall(canvas, slice.X, slice.Y, textureSize, elev, stepHeight, rawWallBuffer);
                        }
                    }
                }

                // 2. Draw the top-cap diamond above
                int drawX = isIsometric ? slice.X - halfW : slice.X;
                int drawY = isIsometric ? slice.Y - halfH : slice.Y;

                canvas.BlitRegionBlend(
                    slice.Buffer,
                    srcX: 0,
                    srcY: 0,
                    width: slice.Buffer.Width,
                    height: slice.Buffer.Height,
                    destX: drawX,
                    destY: drawY);
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
            UnmanagedImageBuffer? canvas,
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
        internal static ImageSource? GenerateGrid(int width, int height, int textureSize)
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
        internal static ImageSource? GenerateNumbers(int width, int height, int textureSize, int padding = 2)
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
        /// Draws the vertical wall facets with tinted selective outlines and soft face shading.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="centerX">The center x.</param>
        /// <param name="centerY">The center y.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="elevation">The elevation.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <param name="rawTexture">The raw texture.</param>
        private static void DrawIsometricWall(
            UnmanagedImageBuffer canvas,
            int centerX,
            int centerY,
            int textureSize,
            int elevation,
            int stepHeight,
            UnmanagedImageBuffer rawTexture)
        {
            int drop = elevation * stepHeight;
            if (drop <= 0) return;

            int halfW = textureSize / 2;
            int halfH = textureSize / 4;

            int texW = rawTexture.Width;
            int texH = rawTexture.Height;

            // 1. Calculate the base color from the texture and create a deeply darkened crease tone (Sel-Out)
            var samplePixel = rawTexture.GetPixel(texW / 2, texH / 2);
            byte edgeR = (byte)(samplePixel.r * 0.30f);
            byte edgeG = (byte)(samplePixel.g * 0.30f);
            byte edgeB = (byte)(samplePixel.b * 0.30f);
            byte edgeA = samplePixel.a;

            // 2. LEFT WALL (South-West: 60% shadow side)
            for (int col = 0; col < halfW; col++)
            {
                int sx = (centerX - halfW) + col;
                if (sx < 0 || sx >= canvas.Width) continue;

                int topY = centerY + (int)(col * (float)halfH / halfW);
                int srcX = Math.Clamp((int)(col * (float)texW / halfW), 0, texW - 1);

                bool isOuterEdge = col == 0;

                for (int dy = 0; dy < drop; dy++)
                {
                    int sy = topY + dy;
                    if (sy < 0 || sy >= canvas.Height) continue;

                    bool isTopOrBottomEdge = dy == 0 || dy == drop - 1;

                    if (isOuterEdge || isTopOrBottomEdge)
                    {
                        // Draw darkened edge crease
                        canvas.SetPixelUnsafe(sx, sy, edgeR, edgeG, edgeB, edgeA);
                    }
                    else
                    {
                        int srcY = dy % texH;
                        var pixel = rawTexture.GetPixel(srcX, srcY);

                        byte r = (byte)(pixel.r * 0.60f);
                        byte g = (byte)(pixel.g * 0.60f);
                        byte b = (byte)(pixel.b * 0.60f);

                        canvas.SetPixelUnsafe(sx, sy, r, g, b, pixel.a);
                    }
                }
            }

            // 3. RIGHT WALL (South-East: 80% light side)
            for (int col = 0; col < halfW; col++)
            {
                int sx = centerX + col;
                if (sx < 0 || sx >= canvas.Width) continue;

                int topY = (centerY + halfH) - (int)(col * (float)halfH / halfW);
                int srcX = Math.Clamp((int)(col * (float)texW / halfW), 0, texW - 1);

                // col == 0 is the front middle corner; col == halfW - 1 is the outer right silhouette
                bool isCornerOrOuter = col == 0 || col == halfW - 1;

                for (int dy = 0; dy < drop; dy++)
                {
                    int sy = topY + dy;
                    if (sy < 0 || sy >= canvas.Height) continue;

                    bool isTopOrBottomEdge = dy == 0 || dy == drop - 1;

                    if (isCornerOrOuter || isTopOrBottomEdge)
                    {
                        // Draw darkened edge crease
                        canvas.SetPixelUnsafe(sx, sy, edgeR, edgeG, edgeB, edgeA);
                    }
                    else
                    {
                        int srcY = dy % texH;
                        var pixel = rawTexture.GetPixel(srcX, srcY);

                        byte r = (byte)(pixel.r * 0.80f);
                        byte g = (byte)(pixel.g * 0.80f);
                        byte b = (byte)(pixel.b * 0.80f);

                        canvas.SetPixelUnsafe(sx, sy, r, g, b, pixel.a);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a transparent vector overlay layer hosting crisp numbers or Unicode symbols.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="glyphMap">The glyph map.</param>
        /// <returns>Bitmap representing the glyph overlay.</returns>
        public static Bitmap GenerateGlyphOverlay(
            int width, int height, int textureSize,
            Dictionary<int, OverlayGlyph>? glyphMap,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            // Die Leinwand muss nun so groß sein wie der Viewport (sonst werden Symbole abgeschnitten)
            int cWidth = viewport is { ScreenWidth: > 0 } ? viewport.ScreenWidth : width * textureSize;
            int cHeight = viewport is { ScreenHeight: > 0 } ? viewport.ScreenHeight : height * textureSize;
            var overlayFrame = new Bitmap(cWidth, cHeight);

            if (glyphMap == null || glyphMap.Count == 0) return overlayFrame;

            using var g = Graphics.FromImage(overlayFrame);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            int halfH = textureSize / 4;

            foreach (var kp in glyphMap)
            {
                var tileIndex = kp.Key;
                var glyph = kp.Value;

                if (string.IsNullOrEmpty(glyph.Symbol)) continue;

                // Echte Projektions-Koordinate inkl. Z-Verschub holen
                int elev = elevationMap != null && elevationMap.TryGetValue(tileIndex, out int z) ? z : 0;
                var pt = viewport?.WorldToScreen(tileIndex, width, textureSize, elev, stepHeight) ??
                         new Point((tileIndex % width) * textureSize, (tileIndex / width) * textureSize);

                // Das Ziel-Rechteck um den projizierten Punkt zentrieren
                float rectX = pt.X - (textureSize / 2f);
                float rectY = pt.Y - (textureSize / 2f);

                // Im isometrischen Modus das Symbol etwas nach oben setzen, 
                // damit es optisch AUF dem Rauten-Boden "steht" und nicht zentriert IM Boden steckt.
                if (viewport?.Mode == ProjectionMode.Isometric)
                {
                    rectY -= halfH;
                }

                var targetRect = new RectangleF(rectX, rectY, textureSize, textureSize);

                var fontStyle = glyph.IsBold ? FontStyle.Bold : FontStyle.Regular;
                // Bei Emojis empfiehlt sich Segoe UI Emoji oder der System-Fallback
                using var font = new Font(glyph.FontName ?? "Segoe UI Emoji", glyph.FontSize, fontStyle);
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
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="idTile">The identifier tile.</param>
        /// <returns>Image on screen</returns>
        public static UnmanagedImageBuffer AddDisplay(
            int width, int height, int textureSize, Dictionary<int, Texture> textures, UnmanagedImageBuffer? layer,
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
                layer = new UnmanagedImageBuffer(width * textureSize, height * textureSize);
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
        /// Generates the grid.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>The generated ImageSource representing the grid.</returns>
        internal static ImageSource? GenerateGrid(
            int width, int height, int textureSize,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            if (viewport?.Mode != ProjectionMode.Isometric)
            {
                // Existing 2D orthogonal grid (exact original code)
                using var bitmap = new Bitmap(width * textureSize, height * textureSize);
                using var graphics = Graphics.FromImage(bitmap);
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        graphics.DrawRectangle(Pens.Black, x * textureSize, y * textureSize, textureSize, textureSize);
                    }
                return bitmap.ToBitmapImage();
            }

            // Isometric diamond grid
            int cWidth = viewport.ScreenWidth > 0 ? viewport.ScreenWidth : width * textureSize;
            int cHeight = viewport.ScreenHeight > 0 ? viewport.ScreenHeight : height * textureSize;

            using var isoBitmap = new Bitmap(cWidth, cHeight);
            using var g = Graphics.FromImage(isoBitmap);
            using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(90, 0, 0, 0), 1f);

            int halfW = textureSize / 2;
            int halfH = textureSize / 4;

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    int tileId = y * width + x;
                    int elev = elevationMap != null && elevationMap.TryGetValue(tileId, out int z) ? z : 0;
                    var pt = viewport.WorldToScreen(tileId, width, textureSize, elev, stepHeight);

                    Point[] diamond =
                    [
                        new Point(pt.X, pt.Y - halfH),
            new Point(pt.X + halfW, pt.Y),
            new Point(pt.X, pt.Y + halfH),
            new Point(pt.X - halfW, pt.Y)
                    ];
                    g.DrawPolygon(pen, diamond);
                }

            return isoBitmap.ToBitmapImage();
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
            this UnmanagedImageBuffer? dest,
            UnmanagedImageBuffer? src,
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