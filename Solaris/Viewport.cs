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
        public float PanX { get; set; }

        /// <summary>
        /// Gets or sets the Y camera offset in screen pixels.
        /// </summary>
        public float PanY { get; set; }

        /// <summary>
        /// Gets or sets the zoom factor (1.0f = 100%).
        /// </summary>
        public float Zoom { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the screen canvas width in pixels.
        /// </summary>
        public int ScreenWidth { get; set; }

        /// <summary>
        /// Gets or sets the screen canvas height in pixels.
        /// </summary>
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
                // Default 2D picking (with Math.Floor instead of Truncate)
                var adjX = (screenPt.X - PanX) / Zoom;
                var adjY = (screenPt.Y - PanY) / Zoom;
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
        /// Calculates the visible bounding box of tile coordinates within the current viewport frustum.
        /// </summary>
        /// <param name="mapWidth">The map width in tile units.</param>
        /// <param name="mapHeight">The map height in tile units.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <returns>A rectangle defining [MinTileX, MinTileY, WidthInTiles, HeightInTiles].</returns>
        public Rectangle GetVisibleTileBounds(int mapWidth, int mapHeight, int textureSize)
        {
            // Do not apply 2D orthogonal culling to isometric projections
            if (Mode == ProjectionMode.Isometric)
            {
                return new Rectangle(0, 0, mapWidth, mapHeight);
            }

            var scaledTileSize = textureSize * Zoom;
            if (scaledTileSize <= 0) return new Rectangle(0, 0, mapWidth, mapHeight);

            var minX = Math.Max(0, (int)Math.Floor(-PanX / scaledTileSize));
            var minY = Math.Max(0, (int)Math.Floor(-PanY / scaledTileSize));

            var visibleCols = (int)Math.Ceiling(ScreenWidth / scaledTileSize) + 1;
            var visibleRows = (int)Math.Ceiling(ScreenHeight / scaledTileSize) + 1;

            var maxX = Math.Min(mapWidth, minX + visibleCols);
            var maxY = Math.Min(mapHeight, minY + visibleRows);

            return new Rectangle(minX, minY, Math.Max(1, maxX - minX), Math.Max(1, maxY - minY));
        }
    }
}
