/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Helper.cs
 * PURPOSE:     Helper class for image processing and map rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using ExtendedSystemObjects;
using Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;

namespace Solaris
{
    /// <summary>
    ///     Helper class that manages image generation tasks.
    /// </summary>
    internal static class Helper
    {
        private static readonly ImageRender Render = new();

        /// <summary>
        ///     Cache for thread-safe bitmap loading
        /// </summary>
        private static readonly ConcurrentDictionary<string, Bitmap> ImageCache = new();

        /// <summary>
        /// Generates the final image based on map and textures.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="map">The map.</param>
        /// <returns>Full Image.</returns>
        internal static Bitmap GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>>? map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            if (map == null || textures == null)
            {
                return background;
            }

            // 1. Safe Sequential Cache Hydration Pass (File I/O)
            foreach (var texture in textures.Values)
            {
                if (string.IsNullOrWhiteSpace(texture.Path)) continue;

                if (!ImageCache.ContainsKey(texture.Path))
                {
                    try
                    {
                        var loadedBmp = Render.GetBitmapFile(texture.Path);
                        if (loadedBmp != null)
                        {
                            ImageCache[texture.Path] = loadedBmp;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"[CRITICAL] Failed to decode asset {texture.Path}: {ex.Message}");
                    }
                }
            }

            // 2. High-Speed Memory Map Translation Pass
            var tiles = new ConcurrentBag<Box>();

            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is not { Count: > 0 }) return;

                var x = (tile.Key % width) * textureSize;
                var y = (tile.Key / width) * textureSize;

                foreach (var textureId in tile.Value)
                {
                    if (!textures.TryGetValue(textureId, out var texture)) continue;

                    if (ImageCache.TryGetValue(texture.Path, out var cachedImage))
                    {
                        tiles.Add(new Box { X = x, Y = y, Layer = texture.Layer, Image = cachedImage });
                    }
                }
            });

            var sortedTiles = tiles.ToList();
            sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // 3. THE GRAPHICS FIX: Open the graphics envelope ONCE for the whole map sheet
            using (var graph = Graphics.FromImage(background))
            {
                // Optional: Ensure a clean alpha base line across the canvas space
                graph.Clear(System.Drawing.Color.Transparent);

                // Blit all 100 tiles rapidly into the open graphics pipeline context
                foreach (var slice in sortedTiles)
                {
                    if (slice.Image != null)
                    {
                        graph.DrawImage(slice.Image,
                            new Rectangle(slice.X, slice.Y, slice.Image.Width, slice.Image.Height));
                    }
                }
            } // GDI+ flushes all operations to system memory and closes cleanly here ONCE

            return background;
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
        public static Bitmap AddDisplay(
            int width, int textureSize, Dictionary<int, Texture> textures, Bitmap? layer,
            KeyValuePair<int, int> idTile)
        {
            var (position, tileId) = idTile;
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            var image = ImageCache.GetOrAdd(textures[tileId].Path, path => Render.GetBitmapFile(path));
            return Render.CombineBitmap(layer, image, x, y);
        }

        /// <summary>
        /// Removes a tile image from the display layer.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="position">The position.</param>
        /// <returns>Cleaned Image.</returns>
        public static Bitmap RemoveDisplay(int width, int textureSize, Bitmap? layer, int position)
        {
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            return Render.EraseRectangle(layer, x, y, textureSize, textureSize);
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
            aurora.IsEnabled = false;

            foreach (var step in steps)
            {
                var x = step % width * textureSize;
                var y = step / width * textureSize;

                // 1. Create a fresh, transparent frame for this specific step to prevent "ghost trails"
                using var frame = new Bitmap(width * textureSize, height * textureSize);

                // 2. Draw the avatar at the current step
                Render.CombineBitmap(frame, avatar, x, y);

                // 3. Push this frame immediately to the UI so we actually see the animation
                aurora.LayerThree.Source = frame.ToBitmapImage();

                // 4. Wait before drawing the next frame
                await Task.Delay(100);
            }

            // Optional: Clear the avatar after the animation is entirely done
            // aurora.LayerThree.Source = null;

            aurora.IsEnabled = true;
        }
    }
}
