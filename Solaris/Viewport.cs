/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Viewport.cs
 * PURPOSE:     Manages camera transforms, coordinate projections, and spatial frustum culling bounds.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;

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
        /// Converts a world tile index into screen pixel coordinates based on projection mode, pan, and zoom.
        /// </summary>
        /// <param name="tileIndex">The 1D spatial tile index.</param>
        /// <param name="mapWidth">The map width in tile units.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <returns>A point representing destination screen pixel coordinates.</returns>
        public Point WorldToScreen(int tileIndex, int mapWidth, int textureSize)
        {
            var tileX = tileIndex % mapWidth;
            var tileY = tileIndex / mapWidth;

            float worldX = tileX * textureSize;
            float worldY = tileY * textureSize;

            if (Projection == ProjectionMode.Isometric)
            {
                var isoX = (worldX - worldY) * 0.5f;
                var isoY = (worldX + worldY) * 0.25f;
                worldX = isoX;
                worldY = isoY;
            }

            var screenX = (int)((worldX * Zoom) + PanX);
            var screenY = (int)((worldY * Zoom) + PanY);

            return new Point(screenX, screenY);
        }

        /// <summary>
        /// Converts a screen pixel coordinate into a 1D spatial tile index.
        /// </summary>
        /// <param name="screenPoint">The screen pixel point.</param>
        /// <param name="mapWidth">The map width in tile units.</param>
        /// <param name="mapHeight">The map height in tile units.</param>
        /// <param name="textureSize">The base tile pixel size.</param>
        /// <returns>The resolved 1D tile index, or -1 if outside grid bounds.</returns>
        public int ScreenToWorld(PointF screenPoint, int mapWidth, int mapHeight, int textureSize)
        {
            var unzoomedX = (screenPoint.X - PanX) / Zoom;
            var unzoomedY = (screenPoint.Y - PanY) / Zoom;

            int tileX;
            int tileY;

            if (Projection == ProjectionMode.Isometric)
            {
                tileX = (int)Math.Floor(((unzoomedX / 0.5f) + (unzoomedY / 0.25f)) / (2 * textureSize));
                tileY = (int)Math.Floor(((unzoomedY / 0.25f) - (unzoomedX / 0.5f)) / (2 * textureSize));
            }
            else
            {
                tileX = (int)Math.Floor(unzoomedX / textureSize);
                tileY = (int)Math.Floor(unzoomedY / textureSize);
            }

            if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
            {
                return -1;
            }

            return (tileY * mapWidth) + tileX;
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
