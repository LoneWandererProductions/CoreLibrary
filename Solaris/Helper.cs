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
        /// <param name="avatarTile">The avatar tile.</param>
        /// <param name="avatarBuffer">The avatar buffer.</param>
        /// <param name="actors">The actors.</param>
        /// <param name="fogMap">The fog map.</param>
        /// <returns>
        /// Unmanaged image buffer representing rendered visible canvas.
        /// </returns>
        internal static UnmanagedImageBuffer GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>>? map,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16,
            int avatarTile = -1,
            UnmanagedImageBuffer? avatarBuffer = null,
            Dictionary<int, GameActor>? actors = null,
            byte[]? fogMap = null)
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

            var bounds = viewport?.GetVisibleTileBounds(width, height, textureSize, 8, stepHeight) ??
                         new Rectangle(0, 0, width, height);

            // Extra Frustum padding to prevent tall walls/sprites from culling too early when panning
            var safeLeft = bounds.Left - 4;
            var safeRight = bounds.Right + 4;
            var safeTop = bounds.Top - 8;
            var safeBottom = bounds.Bottom + 4;
            var safeBounds = new Rectangle(safeLeft, safeTop, safeRight - safeLeft, safeBottom - safeTop);

            var isIsometric = viewport?.Mode == ProjectionMode.Isometric;
            var tiles = new ConcurrentBag<UnmanagedTileBox>();

            // 1. Collect static tiles (with fog check)
            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is not { Count: > 0 }) return;

                var fog = (fogMap != null && tile.Key >= 0 && tile.Key < fogMap.Length)
                    ? fogMap[tile.Key]
                    : (byte)FogState.Visible;

                if (fog == (byte)FogState.Unexplored) return;

                var tileX = tile.Key % width;
                var tileY = tile.Key / width;

                if (tileX < safeLeft || tileX >= safeRight || tileY < safeTop || tileY >= safeBottom)
                    return;

                var elev = elevationMap != null && elevationMap.TryGetValue(tile.Key, out var z) ? z : 0;
                var screenPt = viewport?.WorldToScreen(tile.Key, width, textureSize, elev, stepHeight) ??
                               new Point(tileX * textureSize, tileY * textureSize);

                foreach (var textureId in tile.Value)
                {
                    var cachedBuffer = TextureManager.GetBufferById(textureId, isIsometric);

                    if (cachedBuffer != null && TextureManager.TryGetTexture(textureId, textures, out var texture))
                    {
                        var anchorX = texture.AnchorX ?? (cachedBuffer.Width / 2);
                        var anchorY = (texture.AnchorY ?? cachedBuffer.Height) + texture.HeightOffset;

                        tiles.Add(new UnmanagedTileBox
                        {
                            X = screenPt.X,
                            Y = screenPt.Y,
                            Layer = texture.Layer,
                            Buffer = cachedBuffer,
                            TileX = tileX,
                            TileY = tileY,
                            IsStanding = texture.IsStandingSprite,
                            AnchorX = anchorX,
                            AnchorY = anchorY
                        });
                    }
                }
            });

            // 2. Add avatar (visible only when the field is revealed)
            if (avatarBuffer != null && avatarTile >= 0 && avatarTile < width * height)
            {
                var avatarFog = (fogMap != null && avatarTile < fogMap.Length)
                    ? fogMap[avatarTile]
                    : (byte)FogState.Visible;
                if (avatarFog == (byte)FogState.Visible)
                {
                    InjectActorTile(tiles, avatarTile, avatarBuffer, safeBounds, width, textureSize, viewport,
                        elevationMap, stepHeight, 10, true);
                }
            }

            // 3. Load enemies and units (only in the visible area)
            if (actors != null)
            {
                foreach (var actor in actors.Values)
                {
                    if (actor.Buffer == null || actor.TileIndex < 0 || actor.TileIndex >= width * height)
                        continue;

                    var actorFog = (fogMap != null && actor.TileIndex < fogMap.Length)
                        ? fogMap[actor.TileIndex]
                        : (byte)FogState.Visible;
                    if (actorFog == (byte)FogState.Visible)
                    {
                        InjectActorTile(tiles, actor.TileIndex, actor.Buffer, safeBounds, width, textureSize, viewport,
                            elevationMap, stepHeight, actor.Layer, actor.IsStandingSprite);
                    }
                }
            }

            var sortedTiles = tiles.ToList();

            // 4. Painter Algorithm (Depth Sort)
            if (isIsometric)
            {
                sortedTiles.Sort((a, b) =>
                {
                    var depthA = a.TileX + a.TileY;
                    var depthB = b.TileX + b.TileY;
                    var cmp = depthA.CompareTo(depthB);
                    if (cmp != 0) return cmp;

                    var tileIdA = a.TileY * width + a.TileX;
                    var tileIdB = b.TileY * width + b.TileX;

                    var elevA = elevationMap != null && elevationMap.TryGetValue(tileIdA, out var zA) ? zA : 0;
                    var elevB = elevationMap != null && elevationMap.TryGetValue(tileIdB, out var zB) ? zB : 0;

                    var elevCmp = elevA.CompareTo(elevB);
                    if (elevCmp != 0) return elevCmp;

                    var layerCmp = a.Layer.CompareTo(b.Layer);
                    if (layerCmp != 0) return layerCmp;

                    // Tie-breaker: Draw flat surfaces (false) before standing sprites (true)
                    var standingCmp = a.IsStanding.CompareTo(b.IsStanding);
                    if (standingCmp != 0) return standingCmp;

                    return a.TileX.CompareTo(b.TileX);
                });
            }
            else
            {
                sortedTiles.Sort((a, b) =>
                {
                    var layerCmp = a.Layer.CompareTo(b.Layer);
                    if (layerCmp != 0) return layerCmp;

                    var standingCmp = a.IsStanding.CompareTo(b.IsStanding);
                    if (standingCmp != 0) return standingCmp;

                    var rowCmp = a.TileY.CompareTo(b.TileY);
                    return rowCmp != 0 ? rowCmp : a.TileX.CompareTo(b.TileX);
                });
            }

            var halfW = textureSize / 2;
            var halfH = textureSize / 4;
            var renderedWalls = new HashSet<int>();

            foreach (var slice in sortedTiles)
            {
                var tileId = slice.TileY * width + slice.TileX;
                var elev = elevationMap != null && elevationMap.TryGetValue(tileId, out var z) ? z : 0;

                if (isIsometric && elev > 0 && !slice.IsStanding && renderedWalls.Add(tileId))
                {
                    if (map.TryGetValue(tileId, out var texList) && texList.Count > 0)
                    {
                        var rawWallBuffer = TextureManager.GetBufferById(texList[0], isometric: false);
                        if (rawWallBuffer != null)
                        {
                            DrawIsometricWall(canvas, slice.X, slice.Y, textureSize, elev, stepHeight, rawWallBuffer);
                        }
                    }
                }

                int drawX;
                int drawY;

                if (isIsometric)
                {
                    if (slice.IsStanding)
                    {
                        drawX = slice.X - slice.AnchorX;
                        drawY = slice.Y - slice.AnchorY;
                    }
                    else
                    {
                        drawX = slice.X - halfW;
                        drawY = slice.Y - halfH;
                    }
                }
                else
                {
                    drawX = slice.IsStanding ? slice.X - slice.AnchorX : slice.X;
                    drawY = slice.IsStanding ? slice.Y - slice.AnchorY : slice.Y;
                }

                var currentFog = (fogMap != null && tileId >= 0 && tileId < fogMap.Length)
                    ? fogMap[tileId]
                    : (byte)FogState.Visible;
                var tintFactor = (currentFog == (byte)FogState.Explored) ? 0.45f : 1.0f;

                canvas.BlitRegionBlendTint(
                    slice.Buffer,
                    srcX: 0,
                    srcY: 0,
                    width: slice.Buffer.Width,
                    height: slice.Buffer.Height,
                    destX: drawX,
                    destY: drawY,
                    tintFactor: tintFactor);
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
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns></returns>
        internal static void RedrawTileRegion(
            UnmanagedImageBuffer? canvas,
            int tileIndex,
            int width,
            int textureSize,
            Dictionary<int, Texture>? textures,
            Dictionary<int, List<int>>? map,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            if (canvas == null || width <= 0 || textureSize <= 0) return;

            // ADDED: Apply elevation to the redraw coordinates so it doesn't snap to Z=0
            var elev = elevationMap != null && elevationMap.TryGetValue(tileIndex, out var z) ? z : 0;
            var destPt = viewport?.WorldToScreen(tileIndex, width, textureSize, elev, stepHeight) ??
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
            var drop = elevation * stepHeight;
            if (drop <= 0) return;

            var halfW = textureSize / 2;
            var halfH = textureSize / 4;

            var texW = rawTexture.Width;
            var texH = rawTexture.Height;

            // 1. Calculate the base color from the texture and create a deeply darkened crease tone (Sel-Out)
            var samplePixel = rawTexture.GetPixel(texW / 2, texH / 2);
            var edgeR = (byte)(samplePixel.r * 0.30f);
            var edgeG = (byte)(samplePixel.g * 0.30f);
            var edgeB = (byte)(samplePixel.b * 0.30f);
            var edgeA = samplePixel.a;

            // 2. LEFT WALL (South-West: 60% shadow side)
            for (var col = 0; col < halfW; col++)
            {
                var sx = (centerX - halfW) + col;
                if (sx < 0 || sx >= canvas.Width) continue;

                var topY = centerY + (int)(col * (float)halfH / halfW);
                var srcX = Math.Clamp((int)(col * (float)texW / halfW), 0, texW - 1);

                var isOuterEdge = col == 0;

                for (var dy = 0; dy < drop; dy++)
                {
                    var sy = topY + dy;
                    if (sy < 0 || sy >= canvas.Height) continue;

                    var isTopOrBottomEdge = dy == 0 || dy == drop - 1;

                    if (isOuterEdge || isTopOrBottomEdge)
                    {
                        // Draw darkened edge crease
                        canvas.SetPixelUnsafe(sx, sy, edgeR, edgeG, edgeB, edgeA);
                    }
                    else
                    {
                        var srcY = dy % texH;
                        var pixel = rawTexture.GetPixel(srcX, srcY);

                        var r = (byte)(pixel.r * 0.60f);
                        var g = (byte)(pixel.g * 0.60f);
                        var b = (byte)(pixel.b * 0.60f);

                        canvas.SetPixelUnsafe(sx, sy, r, g, b, pixel.a);
                    }
                }
            }

            // 3. RIGHT WALL (South-East: 80% light side)
            for (var col = 0; col < halfW; col++)
            {
                var sx = centerX + col;
                if (sx < 0 || sx >= canvas.Width) continue;

                var topY = (centerY + halfH) - (int)(col * (float)halfH / halfW);
                var srcX = Math.Clamp((int)(col * (float)texW / halfW), 0, texW - 1);

                // col == 0 is the front middle corner; col == halfW - 1 is the outer right silhouette
                var isCornerOrOuter = col == 0 || col == halfW - 1;

                for (var dy = 0; dy < drop; dy++)
                {
                    var sy = topY + dy;
                    if (sy < 0 || sy >= canvas.Height) continue;

                    var isTopOrBottomEdge = dy == 0 || dy == drop - 1;

                    if (isCornerOrOuter || isTopOrBottomEdge)
                    {
                        // Draw darkened edge crease
                        canvas.SetPixelUnsafe(sx, sy, edgeR, edgeG, edgeB, edgeA);
                    }
                    else
                    {
                        var srcY = dy % texH;
                        var pixel = rawTexture.GetPixel(srcX, srcY);

                        var r = (byte)(pixel.r * 0.80f);
                        var g = (byte)(pixel.g * 0.80f);
                        var b = (byte)(pixel.b * 0.80f);

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
        /// <param name="viewport">The viewport.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>
        /// Bitmap representing the glyph overlay.
        /// </returns>
        public static Bitmap GenerateGlyphOverlay(
            int width, int height, int textureSize,
            Dictionary<int, OverlayGlyph>? glyphMap,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            // The canvas must now be the same size as the viewport (otherwise icons will be cut off)
            var cWidth = viewport is { ScreenWidth: > 0 } ? viewport.ScreenWidth : width * textureSize;
            var cHeight = viewport is { ScreenHeight: > 0 } ? viewport.ScreenHeight : height * textureSize;
            var overlayFrame = new Bitmap(cWidth, cHeight);

            if (glyphMap == null || glyphMap.Count == 0) return overlayFrame;

            using var g = Graphics.FromImage(overlayFrame);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            var halfH = textureSize / 4;

            foreach (var kp in glyphMap)
            {
                var tileIndex = kp.Key;
                var glyph = kp.Value;

                if (string.IsNullOrEmpty(glyph.Symbol)) continue;

                // Echte Projektions-Koordinate inkl. Z-Verschub holen
                var elev = elevationMap != null && elevationMap.TryGetValue(tileIndex, out var z) ? z : 0;
                var pt = viewport?.WorldToScreen(tileIndex, width, textureSize, elev, stepHeight) ??
                         new Point((tileIndex % width) * textureSize, (tileIndex / width) * textureSize);

                // Das Ziel-Rechteck um den projizierten Punkt zentrieren
                var rectX = pt.X - (textureSize / 2f);
                var rectY = pt.Y - (textureSize / 2f);

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
        /// <param name="viewport">The viewport.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>
        /// Image on screen
        /// </returns>
        public static UnmanagedImageBuffer AddDisplay(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            UnmanagedImageBuffer? layer,
            KeyValuePair<int, int> idTile,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            var cWidth = viewport is { ScreenWidth: > 0 } ? viewport.ScreenWidth : width * textureSize;
            var cHeight = viewport is { ScreenHeight: > 0 } ? viewport.ScreenHeight : height * textureSize;

            if (layer == null || layer.Width != cWidth || layer.Height != cHeight)
            {
                layer?.Dispose();
                layer = new UnmanagedImageBuffer(cWidth, cHeight);
                layer.Clear(0, 0, 0, 0);
            }

            var (position, tileId) = idTile;
            var elev = elevationMap != null && elevationMap.TryGetValue(position, out var z) ? z : 0;
            var pt = viewport?.WorldToScreen(position, width, textureSize, elev, stepHeight) ??
                     new Point((position % width) * textureSize, (position / width) * textureSize);

            var isIso = viewport?.Mode == ProjectionMode.Isometric;
            var tileBuffer = TextureManager.GetBufferById(tileId, isIso);

            if (tileBuffer == null && TextureManager.TryGetTexture(tileId, textures, out var texture))
            {
                TextureManager.RegisterTexture(texture);
                tileBuffer = TextureManager.GetBufferById(tileId, isIso);
            }

            if (tileBuffer == null) return layer;

            int drawX;
            int drawY;

            if (isIso)
            {
                if (TextureManager.TryGetTexture(tileId, textures, out var texDef) && texDef.IsStandingSprite)
                {
                    drawX = pt.X - (tileBuffer.Width / 2);
                    drawY = pt.Y - tileBuffer.Height;
                }
                else
                {
                    drawX = pt.X - (textureSize / 2);
                    drawY = pt.Y - (textureSize / 4);
                }
            }
            else
            {
                drawX = pt.X;
                drawY = pt.Y;
            }

            layer.BlitRegionBlend(tileBuffer, 0, 0, tileBuffer.Width, tileBuffer.Height, drawX, drawY);
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
        /// Animates movement across tile steps.
        /// Supports both unified LayerOne depth sorting and LayerThree overlay projection.
        /// </summary>
        /// <param name="aurora">The aurora control instance.</param>
        /// <param name="steps">The tile steps sequence.</param>
        /// <param name="avatar">The avatar bitmap.</param>
        /// <param name="width">The width in tile units.</param>
        /// <param name="height">The height in tile units.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="useUnifiedLayerOne">if set to <c>true</c> [use unified layer one].</param>
        /// <param name="stepDelayMs">The step delay ms.</param>
        /// <returns>
        /// Task representing the asynchronous operation.
        /// </returns>
        internal static async Task DisplayMovement(
            Aurora aurora,
            IEnumerable<int> steps,
            Bitmap? avatar,
            int width,
            int height,
            int textureSize,
            bool useUnifiedLayerOne = true,
            int stepDelayMs = 100)
        {
            var avatarBmp = avatar ?? aurora.AuroraAvatar;
            if (avatarBmp == null) return;

            aurora.IsEnabled = false;

            try
            {
                if (useUnifiedLayerOne)
                {
                    // Mode A: Dynamic sorting in LayerOne (properly occludes behind foreground walls/trees)
                    foreach (var step in steps)
                    {
                        aurora.AvatarPosition = step;
                        aurora.MarkLayerDirty(DirtyFlags.FullRedraw);
                        aurora.RenderDirty();

                        await Task.Delay(stepDelayMs);
                    }
                }
                else
                {
                    // Mode B: Fast LayerThree blitting with viewport transforms and zero GC allocations
                    var canvasW = aurora.ActiveViewport.ScreenWidth > 0
                        ? aurora.ActiveViewport.ScreenWidth
                        : width * textureSize;
                    var canvasH = aurora.ActiveViewport.ScreenHeight > 0
                        ? aurora.ActiveViewport.ScreenHeight
                        : height * textureSize;

                    using var avatarBuffer = UnmanagedImageBuffer.FromBitmap(avatarBmp);
                    using var frameBuffer = new UnmanagedImageBuffer(canvasW, canvasH);

                    var isIso = aurora.ActiveViewport.Mode == ProjectionMode.Isometric;

                    foreach (var step in steps)
                    {
                        frameBuffer.Clear(0, 0, 0, 0);

                        var elev = aurora.AuroraElevation != null && aurora.AuroraElevation.TryGetValue(step, out var z)
                            ? z
                            : 0;
                        var pt = aurora.ActiveViewport.WorldToScreen(step, width, textureSize, elev, aurora.StepHeight);

                        var drawX = isIso
                            ? pt.X - (avatarBuffer.Width / 2)
                            : pt.X + (textureSize - avatarBuffer.Width) / 2;
                        var drawY = isIso ? pt.Y - avatarBuffer.Height : pt.Y + (textureSize - avatarBuffer.Height);

                        frameBuffer.BlitRegionBlend(avatarBuffer, 0, 0, avatarBuffer.Width, avatarBuffer.Height, drawX,
                            drawY);

                        aurora.LayerThree.Source =
                            frameBuffer.UpdateWriteableBitmap(aurora.LayerThree.Source as WriteableBitmap);

                        await Task.Delay(stepDelayMs);
                    }

                    // Restore LayerThree to static display buffer or clear
                    if (aurora.BitmapLayerThree != null)
                    {
                        aurora.LayerThree.Source =
                            aurora.BitmapLayerThree.UpdateWriteableBitmap(aurora.LayerThree.Source as WriteableBitmap);
                    }
                    else
                    {
                        frameBuffer.Clear(0, 0, 0, 0);
                        aurora.LayerThree.Source =
                            frameBuffer.UpdateWriteableBitmap(aurora.LayerThree.Source as WriteableBitmap);
                    }
                }
            }
            finally
            {
                aurora.IsEnabled = true;
            }
        }


        /// <summary>
        /// Removes the display.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="position">The position.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>The UnmanagedImageBuffer.</returns>
        public static UnmanagedImageBuffer? RemoveDisplay(
            int width, int textureSize, UnmanagedImageBuffer? layer,
            int position,
            Viewport? viewport = null,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            if (layer == null) return null;

            var elev = elevationMap != null && elevationMap.TryGetValue(position, out var z) ? z : 0;
            var pt = viewport?.WorldToScreen(position, width, textureSize, elev, stepHeight) ??
                     new Point((position % width) * textureSize, (position / width) * textureSize);

            var halfW = textureSize / 2;
            var halfH = textureSize / 4;

            var clearX = viewport?.Mode == ProjectionMode.Isometric ? pt.X - halfW : pt.X;

            // INCREASE bounding box height and shift Y up to fully erase tall standing sprites
            var clearY = viewport?.Mode == ProjectionMode.Isometric ? pt.Y - textureSize : pt.Y;
            var clearH = viewport?.Mode == ProjectionMode.Isometric ? textureSize + halfH : textureSize;

            ClearTileRegion(layer, clearX, clearY, Math.Max(textureSize, clearH));
            return layer;
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
            var cWidth = viewport.ScreenWidth > 0 ? viewport.ScreenWidth : width * textureSize;
            var cHeight = viewport.ScreenHeight > 0 ? viewport.ScreenHeight : height * textureSize;

            using var isoBitmap = new Bitmap(cWidth, cHeight);
            using var g = Graphics.FromImage(isoBitmap);
            using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(90, 0, 0, 0), 1f);

            var halfW = textureSize / 2;
            var halfH = textureSize / 4;

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var tileId = y * width + x;
                var elev = elevationMap != null && elevationMap.TryGetValue(tileId, out var z) ? z : 0;
                var pt = viewport.WorldToScreen(tileId, width, textureSize, elev, stepHeight);

                Point[] diamond = 

                [
                    new Point(pt.X, pt.Y

                -halfH),
                pt with { X = pt.X + halfW },
                pt with { Y = pt.Y + halfH },
                pt with { X = pt.X - halfW }
                    ];
                g.DrawPolygon(pen, diamond);
            }

            return isoBitmap.ToBitmapImage();
        }

        /// <summary>
        /// Injects the actor tile.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="tileIndex">Index of the tile.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="mapWidth">Width of the map.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="isStanding">if set to <c>true</c> [is standing].</param>
        private static void InjectActorTile(
            ConcurrentBag<UnmanagedTileBox> bag,
            int tileIndex,
            UnmanagedImageBuffer buffer,
            Rectangle bounds,
            int mapWidth,
            int textureSize,
            Viewport? viewport,
            Dictionary<int, int>? elevationMap,
            int stepHeight,
            int layer,
            bool isStanding)
        {
            var tx = tileIndex % mapWidth;
            var ty = tileIndex / mapWidth;

            if (tx < bounds.Left || tx >= bounds.Right || ty < bounds.Top || ty >= bounds.Bottom)
                return;

            var elev = elevationMap != null && elevationMap.TryGetValue(tileIndex, out var z) ? z : 0;
            var pt = viewport?.WorldToScreen(tileIndex, mapWidth, textureSize, elev, stepHeight) ??
                     new Point(tx * textureSize, ty * textureSize);

            bag.Add(new UnmanagedTileBox
            {
                X = pt.X,
                Y = pt.Y,
                Layer = layer,
                Buffer = buffer,
                TileX = tx,
                TileY = ty,
                IsStanding = isStanding
            });
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

        /// <summary>
        /// Blits the region blend tint.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="srcX">The source x.</param>
        /// <param name="srcY">The source y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="destX">The dest x.</param>
        /// <param name="destY">The dest y.</param>
        /// <param name="tintFactor">The tint factor.</param>
        private static unsafe void BlitRegionBlendTint(
            this UnmanagedImageBuffer? dest,
            UnmanagedImageBuffer? src,
            int srcX, int srcY,
            int width, int height,
            int destX, int destY,
            float tintFactor = 1.0f)
        {
            if (dest == null || src == null) return;

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

            if (destX + width > dest.Width) width = dest.Width - destX;
            if (destY + height > dest.Height) height = dest.Height - destY;

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

            if (srcX + width > src.Width) width = src.Width - srcX;
            if (srcY + height > src.Height) height = src.Height - srcY;

            if (width <= 0 || height <= 0) return;

            var srcStride = src.Width * UnmanagedImageBuffer.BytesPerPixel;
            var destStride = dest.Width * UnmanagedImageBuffer.BytesPerPixel;

            var pSrcBase = (byte*)src.Buffer.ToPointer() + (srcY * srcStride) +
                           (srcX * UnmanagedImageBuffer.BytesPerPixel);
            var pDestBase = (byte*)dest.Buffer.ToPointer() + (destY * destStride) +
                            (destX * UnmanagedImageBuffer.BytesPerPixel);

            var applyTint = tintFactor < 0.99f;

            for (var y = 0; y < height; y++)
            {
                var pSrc = (uint*)(pSrcBase + y * srcStride);
                var pDest = (uint*)(pDestBase + y * destStride);

                for (var x = 0; x < width; x++)
                {
                    var srcPixel = pSrc[x];
                    var alpha = (byte)(srcPixel >> 24);
                    if (alpha == 0) continue;

                    var srcB = (byte)(srcPixel & 0xFF);
                    var srcG = (byte)((srcPixel >> 8) & 0xFF);
                    var srcR = (byte)((srcPixel >> 16) & 0xFF);

                    if (applyTint)
                    {
                        srcB = (byte)(srcB * tintFactor);
                        srcG = (byte)(srcG * tintFactor);
                        srcR = (byte)(srcR * tintFactor);
                    }

                    if (alpha == 255)
                    {
                        pDest[x] = (255u << 24) | ((uint)srcR << 16) | ((uint)srcG << 8) | srcB;
                        continue;
                    }

                    var dstPixel = pDest[x];
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
