/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Helper.cs
 * PURPOSE:     Helper class for image processing and map rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ExtendedSystemObjects;
using Imaging;
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
        ///     Generates the final image based on map and textures.
        /// </summary>
        internal static Bitmap GenerateImage(
            int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>> map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var tiles = new List<Box>();

            Parallel.ForEach(map, tile =>
            {
                if (tile.Value is { Count: > 0 })
                {
                    var x = tile.Key % width * textureSize;
                    var y = tile.Key / width * textureSize;

                    lock (tiles)
                    {
                        tiles.AddRange(tile.Value.Select(texture => new Box
                        {
                            X = x,
                            Y = y,
                            Layer = textures[texture].Layer,
                            Image = Render.GetBitmapFile(textures[texture].Path)
                        }));
                    }
                }
            });

            tiles.Sort((a, b) => a.Layer.CompareTo(b.Layer)); // Ensure layering order

            return tiles.Aggregate(background, (current, slice) =>
                Render.CombineBitmap(current, slice.Image, slice.X, slice.Y));
        }

        /// <summary>
        ///     Generates a grid overlay.
        /// </summary>
        internal static ImageSource GenerateGrid(int width, int height, int textureSize)
        {
            var bitmap = new Bitmap(width * textureSize, height * textureSize);
            using var graphics = Graphics.FromImage(bitmap);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                graphics.DrawRectangle(Pens.Black, x * textureSize, y * textureSize, textureSize, textureSize);
            }

            return bitmap.ToBitmapImage();
        }

        /// <summary>
        ///     Generates a number overlay.
        /// </summary>
        internal static ImageSource GenerateNumbers(int width, int height, int textureSize, int padding = 2)
        {
            var bitmap = new Bitmap(width * textureSize, height * textureSize);
            using var graphics = Graphics.FromImage(bitmap);

            var count = 0;
            var font = new Font(Resources.Font, 8);
            var brush = Brushes.Black;

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
        ///     Adds a tile to the map.
        /// </summary>
        internal static KeyValuePair<bool, Dictionary<int, List<int>>> AddTile(
            Dictionary<int, List<int>> map, KeyValuePair<int, int> idTexture)
        {
            map ??= new Dictionary<int, List<int>>();
            var added = map.AddDistinct(idTexture.Key, idTexture.Value);
            return new KeyValuePair<bool, Dictionary<int, List<int>>>(added, map);
        }

        /// <summary>
        ///     Removes a tile from the map.
        /// </summary>
        internal static KeyValuePair<bool, Dictionary<int, List<int>>> RemoveTile(
            Dictionary<int, List<int>> map, Dictionary<int, Texture> textures, KeyValuePair<int, int> idLayer)
        {
            if (map == null || !map.TryGetValue(idLayer.Key, out var tileList))
            {
                return new KeyValuePair<bool, Dictionary<int, List<int>>>(false, new Dictionary<int, List<int>>());
            }

            var updatedList = tileList.Where(tile => textures[tile].Layer != idLayer.Value).ToList();
            if (updatedList.Count == tileList.Count)
            {
                return new KeyValuePair<bool, Dictionary<int, List<int>>>(false, map);
            }

            map[idLayer.Key] = updatedList;
            return new KeyValuePair<bool, Dictionary<int, List<int>>>(true, map);
        }

        /// <summary>
        ///     Adds a tile image to the display layer.
        /// </summary>
        public static Bitmap AddDisplay(
            int width, int textureSize, Dictionary<int, Texture> textures, Bitmap layer,
            KeyValuePair<int, int> idTile)
        {
            var (position, tileId) = idTile;
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            var image = Render.GetBitmapFile(textures[tileId].Path);
            return Render.CombineBitmap(layer, image, x, y);
        }

        /// <summary>
        ///     Removes a tile image from the display layer.
        /// </summary>
        public static Bitmap RemoveDisplay(int width, int textureSize, Bitmap layer, int position)
        {
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            return Render.EraseRectangle(layer, x, y, textureSize, textureSize);
        }

        /// <summary>
        ///     Displays movement animation.
        /// </summary>
        internal static async Task DisplayMovement(Aurora aurora, IEnumerable<int> steps, Bitmap avatar,
            int width, int height, int textureSize)
        {
            aurora.IsEnabled = false;
            var background = new Bitmap(width * textureSize, height * textureSize);

            foreach (var step in steps)
            {
                var x = step % width * textureSize;
                var y = step / width * textureSize;

                await MoveAvatar(x, y, background, avatar, 100);
            }

            aurora.IsEnabled = true;
            aurora.LayerThree.Source = background.ToBitmapImage();
        }

        /// <summary>
        ///     Moves the avatar to a new position with animation.
        /// </summary>
        private static async Task<bool> MoveAvatar(int x, int y, Bitmap background, Bitmap avatar, int sleep)
        {
            Render.CombineBitmap(background, avatar, x, y);
            await Task.Delay(sleep);
            return true;
        }
    }
}
