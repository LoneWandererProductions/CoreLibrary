/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Helper.cs
 * PURPOSE:     Helper class, here we process everything. For the Coordinate Id it uses the same principles as Coordinate2D.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ExtendedSystemObjects;
using Imaging;
using Brushes = System.Drawing.Brushes;

namespace Solaris
{
    /// <summary>
    ///     Handle all the Image generation Tasks
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        ///     The padding for the numbers
        /// </summary>
        private const int Padding = 2;

        /// <summary>
        ///     The render Interface
        /// </summary>
        private static readonly ImageRender Render = new();

        /// <summary>
        ///     Generates the image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="map">The map.</param>
        /// <returns>The generated Map</returns>
        internal static Bitmap GenerateImage(int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>> map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var tiles = (from tile in map
                where !tile.Value.IsNullOrEmpty()
                from texture in tile.Value
                select new Box
                {
                    X = tile.Key % width * textureSize,
                    Y = tile.Key / width * textureSize,
                    Layer = textures[texture].Layer,
                    Image = Render.GetBitmapFile(textures[texture].Path)
                }).ToList();

            tiles = tiles.OrderBy(layer => layer.Layer).ToList();

            return tiles.Aggregate(background,
                (current, slice) => Render.CombineBitmap(current, slice.Image, slice.X, slice.Y));
        }

        /// <summary>
        ///     Generates the grid.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>A grid overlay</returns>
        internal static ImageSource GenerateGrid(int width, int height, int textureSize)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    using var graphics = Graphics.FromImage(background);
                    graphics.DrawRectangle(Pens.Black, x * textureSize, y * textureSize, textureSize, textureSize);
                }
            }

            return background.ToBitmapImage();
        }

        /// <summary>
        ///     Generates the numbers.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>A number overlay</returns>
        internal static ImageSource GenerateNumbers(int width, int height, int textureSize)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var count = -1;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    count++;

                    using var graphics = Graphics.FromImage(background);
                    var rectangle = new RectangleF((x * textureSize) + Padding, (y * textureSize) + Padding,
                        textureSize - Padding, textureSize - Padding);
                    graphics.DrawString(count.ToString(), new Font(Resources.Font, 8), Brushes.Black, rectangle);
                }
            }

            return background.ToBitmapImage();
        }

        /// <summary>
        ///     Adds the tile.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="idTexture">The identifier texture.</param>
        /// <returns>If changes are needed</returns>
        internal static KeyValuePair<bool, Dictionary<int, List<int>>> AddTile(Dictionary<int, List<int>> map,
            KeyValuePair<int, int> idTexture)
        {
            map ??= new Dictionary<int, List<int>>();

            var (id, texture) = idTexture;
            var check = map.AddDistinct(id, texture);

            return new KeyValuePair<bool, Dictionary<int, List<int>>>(check, map);
        }

        /// <summary>
        ///     Removes the tile.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="idLayer">The identifier layer.</param>
        /// <returns>If changes are needed</returns>
        internal static KeyValuePair<bool, Dictionary<int, List<int>>> RemoveTile(Dictionary<int, List<int>> map,
            Dictionary<int, Texture> textures,
            KeyValuePair<int, int> idLayer)
        {
            if (map == null)
            {
                return new KeyValuePair<bool, Dictionary<int, List<int>>>(false, null);
            }

            var (id, layer) = idLayer;

            if (!map.ContainsKey(id))
            {
                return new KeyValuePair<bool, Dictionary<int, List<int>>>(false, null);
            }

            var lst = map[id];

            var cache = lst.Where(tile => textures[tile].Layer != layer).ToList();

            if (cache.Count == lst.Count)
            {
                return new KeyValuePair<bool, Dictionary<int, List<int>>>(false, map);
            }

            map[id] = cache;

            return new KeyValuePair<bool, Dictionary<int, List<int>>>(true, map);
        }

        /// <summary>
        ///     Adds the display.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="idTile">The id Position and the tile Id.</param>
        /// <returns>Layer three Bitmap</returns>
        public static Bitmap AddDisplay(int width, int textureSize, Dictionary<int, Texture> textures, Bitmap layer,
            KeyValuePair<int, int> idTile)
        {
            var (position, tileId) = idTile;
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            var image = Render.GetBitmapFile(textures[tileId].Path);

            return Render.CombineBitmap(layer, image, x, y);
        }

        /// <summary>
        ///     Removes the display.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="position">The position.</param>
        /// <returns>Layer three Bitmap</returns>
        public static Bitmap RemoveDisplay(int width, int textureSize, Bitmap layer, int position)
        {
            var x = position % width * textureSize;
            var y = position / width * textureSize;

            return Render.EraseRectangle(layer, x, y, textureSize, textureSize);
        }

        /// <summary>
        ///     Displays the movement.
        /// </summary>
        /// <param name="aurora">The aurora.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="avatar">The avatar.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        internal static async void DisplayMovement(Aurora aurora, IEnumerable<int> steps, Bitmap avatar,
            int width,
            int height,
            int textureSize)
        {
            aurora.IsEnabled = false;

            var background = new Bitmap(width * textureSize, height * textureSize);

            foreach (var step in steps)
            {
                var x = step % width * textureSize;
                var y = step / width * textureSize;

                _ = await Task.Run(() => SwitchPosition(x, y, background, avatar, 100));
            }

            aurora.IsEnabled = true;
            aurora.LayerThree.Source = background.ToBitmapImage();
        }

        /// <summary>
        ///     Switches the position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="background">The background.</param>
        /// <param name="avatar">The avatar.</param>
        /// <param name="sleep">The sleep.</param>
        /// <returns>Generic return value needed for threading.</returns>
        private static bool SwitchPosition(int x, int y, Bitmap background, Bitmap avatar, int sleep)
        {
            Render.CombineBitmap(background, avatar, x, y);
            Thread.Sleep(sleep);
            return true;
        }
    }
}
