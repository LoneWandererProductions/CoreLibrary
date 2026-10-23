/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Viewport.cs
 * PURPOSE:     Manages camera transforms, coordinate projections, and spatial frustum culling bounds.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Solaris
{
    /// <summary>
    /// Represents a camera viewport managing pan offsets, zoom factors, projection styles, and visible frustum culling bounds.
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// Gets or sets the X camera offset in screen pixels.
        /// </summary>
        /// <value>
        /// The pan x.
        /// </value>
        public float PanX { get; set; }

        /// <summary>
        /// Gets or sets the Y camera offset in screen pixels.
        /// </summary>
        /// <value>
        /// The pan y.
        /// </value>
        public float PanY { get; set; }

        /// <summary>
        /// Gets or sets the zoom factor (1.0f = 100%).
        /// </summary>
        /// <value>
        /// The zoom.
        /// </value>
        public float Zoom { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the screen canvas width in pixels.
        /// </summary>
        /// <value>
        /// The width of the screen.
        /// </value>
        public int ScreenWidth { get; set; }

        /// <summary>
        /// Gets or sets the screen canvas height in pixels.
        /// </summary>
        /// <value>
        /// The height of the screen.
        /// </value>
        public int ScreenHeight { get; set; }

        /// <summary>
        /// Gets or sets the visual projection mode.
        /// </summary>
        public ProjectionMode Projection { get; set; } = ProjectionMode.Orthographic2D;

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public ProjectionMode Mode { get; set; } = ProjectionMode.Orthographic2D;

        /// <summary>
        /// Converts a world tile index into screen pixel coordinates based on projection mode, pan, and zoom.
        /// </summary>
        /// <param name="tileIndex">The 1D spatial tile index.</param>
        /// <param name="mapWidth">The map width in tile units.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <returns>A point representing destination screen pixel coordinates.</returns>
        public Point WorldToScreen(int tileIndex, int mapWidth, int textureSize, int elevation = 0, int stepHeight = 16)
        {
            int x = tileIndex % mapWidth;
            int y = tileIndex / mapWidth;

            if (Mode == ProjectionMode.Isometric)
            {
                int halfW = textureSize / 2;
                int halfH = textureSize / 4; // 2:1 dimetric ratio

                int originX = ScreenWidth > 0 ? ScreenWidth / 2 : (mapWidth * textureSize) / 2;
                int originY = ScreenHeight > 0 ? textureSize : 80;

                // Apply isometric projection minus vertical elevation
                int sx = originX + (int)((x - y) * halfW * Zoom) + (int)PanX;
                int sy = originY + (int)((x + y) * halfH * Zoom) - (int)(elevation * stepHeight * Zoom) + (int)PanY;

                return new Point(sx, sy);
            }

            // Default Orthographic 2D path (completely unmodified behavior)
            return new Point((int)(x * textureSize * Zoom + PanX), (int)(y * textureSize * Zoom + PanY));
        }

        /// <summary>
        /// Converts a screen pixel coordinate into a 1D spatial tile index.
        /// </summary>
        /// <param name="screenPt">The screen pt.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <param name="elevationMap">The elevation map.</param>
        /// <param name="stepHeight">Height of the step.</param>
        /// <returns>
        /// The resolved 1D tile index, or -1 if outside grid bounds.
        /// </returns>
        public int ScreenToWorld(
            PointF screenPt,
            int width,
            int height,
            int textureSize,
            Dictionary<int, int>? elevationMap = null,
            int stepHeight = 16)
        {
            if (Mode != ProjectionMode.Isometric)
            {
                // Default 2D picking (unmodified)
                var adjX = (screenPt.X - PanX) / Zoom;
                var adjY = (screenPt.Y - PanY) / Zoom;

                // Use Math.Floor so negative values correctly round down to -1
                int x = (int)Math.Floor(adjX / textureSize);
                int y = (int)Math.Floor(adjY / textureSize);

                return (x >= 0 && x < width && y >= 0 && y < height) ? y * width + x : -1;
            }

            int halfW = textureSize / 2;
            int halfH = textureSize / 4;
            int originX = ScreenWidth > 0 ? ScreenWidth / 2 : (width * textureSize) / 2;
            int originY = ScreenHeight > 0 ? textureSize : 80;

            // Scan top-down from highest elevation to floor (Z = 0)
            int maxElevation = elevationMap is { Count: > 0 }
                ? Math.Max(0, elevationMap.Values.Max())
                : 0;

            for (int z = maxElevation; z >= 0; z--)
            {
                float adjX = (screenPt.X - originX - PanX) / Zoom;
                float adjY = ((screenPt.Y + (z * stepHeight * Zoom)) - originY - PanY) / Zoom;

                int tileX = (int)Math.Floor((adjX / halfW + adjY / halfH) / 2.0f);
                int tileY = (int)Math.Floor((adjY / halfH - adjX / halfW) / 2.0f);

                if (tileX >= 0 && tileX < width && tileY >= 0 && tileY < height)
                {
                    int index = tileY * width + tileX;
                    int actualElev = elevationMap != null && elevationMap.TryGetValue(index, out int e) ? e : 0;

                    if (actualElev == z)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Calculates the visible tile rectangle in the current viewport frustum.
        /// Now supports Orthographic2D and Isometric, including elevation and height offset.
        /// </summary>
        /// <param name="mapWidth">The map width in tile units.</param>
        /// <param name="mapHeight">The map height in tile units.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <returns>A rectangle defining [MinTileX, MinTileY, WidthInTiles, HeightInTiles].</returns>
        public Rectangle GetVisibleTileBounds(int mapWidth, int mapHeight, int textureSize, int maxElevation = 8, int stepHeight = 16)
        {
            if (mapWidth <= 0 || mapHeight <= 0 || textureSize <= 0)
                return new Rectangle(0, 0, 0, 0);

            if (Mode != ProjectionMode.Isometric)
            {
                var scaledTileSize = textureSize * Zoom;
                if (scaledTileSize <= 0) return new Rectangle(0, 0, mapWidth, mapHeight);

                var minOrthX = Math.Max(0, (int)Math.Floor(-PanX / scaledTileSize));
                var minOrthY = Math.Max(0, (int)Math.Floor(-PanY / scaledTileSize));

                var visibleCols = (int)Math.Ceiling(ScreenWidth / scaledTileSize) + 1;
                var visibleRows = (int)Math.Ceiling(ScreenHeight / scaledTileSize) + 1;

                var maxOrthX = Math.Min(mapWidth, minOrthX + visibleCols);
                var maxOrthY = Math.Min(mapHeight, minOrthY + visibleRows);

                return new Rectangle(minOrthX, minOrthY, Math.Max(1, maxOrthX - minOrthX), Math.Max(1, maxOrthY - minOrthY));
            }

            // --- ISOMETRISCHES CULLING ---
            int halfW = textureSize / 2;
            int halfH = textureSize / 4;
            int originX = ScreenWidth > 0 ? ScreenWidth / 2 : (mapWidth * textureSize) / 2;
            int originY = ScreenHeight > 0 ? textureSize : 80;

            // Raised tiles or tall sprites extend beyond the top and bottom of the screen: extend the top and bottom edges of the screen
            float elevBufferPx = maxElevation * stepHeight * Zoom;
            float spriteBufferPx = textureSize * 2.0f * Zoom; // Puffer für hohe Bäume/Wände

            // The 4 corners of the visible screen (with buffer)
            var corners = new[]
            {
                new PointF(-spriteBufferPx, -spriteBufferPx),
                new PointF(ScreenWidth + spriteBufferPx, -spriteBufferPx),
                new PointF(ScreenWidth + spriteBufferPx, ScreenHeight + spriteBufferPx + elevBufferPx),
                new PointF(-spriteBufferPx, ScreenHeight + spriteBufferPx + elevBufferPx)
            };

            float minTileX = float.MaxValue;
            float maxTileX = float.MinValue;
            float minTileY = float.MaxValue;
            float maxTileY = float.MinValue;

            foreach (var pt in corners)
            {
                float adjX = (pt.X - originX - PanX) / (Zoom * halfW);
                float adjY = (pt.Y - originY - PanY) / (Zoom * halfH);

                // Inverse 2:1-Projection
                float tx = (adjX + adjY) / 2.0f;
                float ty = (adjY - adjX) / 2.0f;

                if (tx < minTileX) minTileX = tx;
                if (tx > maxTileX) maxTileX = tx;
                if (ty < minTileY) minTileY = ty;
                if (ty > maxTileY) maxTileY = ty;
            }

            // Clip to map size (Flat Map Bounds)
            int left = Math.Clamp((int)Math.Floor(minTileX) - 1, 0, mapWidth);
            int top = Math.Clamp((int)Math.Floor(minTileY) - 1, 0, mapHeight);
            int right = Math.Clamp((int)Math.Ceiling(maxTileX) + 1, 0, mapWidth);
            int bottom = Math.Clamp((int)Math.Ceiling(maxTileY) + 1, 0, mapHeight);

            return new Rectangle(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
        }
    }
}
