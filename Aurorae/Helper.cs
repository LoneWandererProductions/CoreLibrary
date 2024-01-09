using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtendedSystemObjects;
using Imaging;
using Brushes = System.Drawing.Brushes;

namespace Aurorae
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
        internal static BitmapImage GenerateImage(int width, int height, int textureSize,
            Dictionary<int, Texture> textures,
            Dictionary<int, List<int>> map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var tiles = (from tile in map
                from layer in tile.Value
                select new Box
                {
                    X = IdToX(tile.Key, width) * textureSize,
                    Y = IdToY(tile.Key, width) * textureSize,
                    Layer = textures[tile.Key].Layer,
                    Image = Render.GetBitmapFile(textures[tile.Key].Path)
                }).ToList();

            background = tiles.OrderBy(layer => layer.Layer).ToList().Aggregate(background,
                (current, slice) => Render.CombineBitmap(current, slice.Image, slice.X, slice.Y));

            return background.ToBitmapImage();
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
                    graphics.DrawString(count.ToString(), new Font("Tahoma", 8), Brushes.Black, rectangle);
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
        internal static bool AddTile(Dictionary<int, List<int>> map,
            KeyValuePair<int, int> idTexture)
        {
            var (id, texture) = idTexture;
            if (!map.ContainsKey(id))
            {
                return false;
            }

            var lst = map[id];
            var check = lst.AddDistinct(texture);

            if (check)
            {
                return false;
            }

            map[id] = lst;

            return true;
        }

        /// <summary>
        ///     Removes the tile.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="idLayer">The identifier layer.</param>
        /// <returns>If changes are needed</returns>
        internal static bool RemoveTile(Dictionary<int, List<int>> map,
            Dictionary<int, Texture> textures,
            KeyValuePair<int, int> idLayer)
        {
            var (id, layer) = idLayer;
            if (!map.ContainsKey(id))
            {
                return false;
            }

            var lst = map[id];

            var cache = new List<int>(lst);
            cache.AddRange(from tile in lst let compare = textures[tile] where compare.Layer != layer select tile);

            if (cache.Count == lst.Count)
            {
                return false;
            }

            map[id] = cache;

            return true;
        }

        /// <summary>
        ///     Adds the display.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="idTile">The id Position and the tile Id.</param>
        public static void AddDisplay(int width, int textureSize, Dictionary<int, Texture> textures, Bitmap layer,
            KeyValuePair<int, int> idTile)
        {
            var (position, tileId) = idTile;
            var x = IdToX(position, width) * textureSize;
            var y = IdToY(position, width) * textureSize;

            var image = Render.GetBitmapFile(textures[tileId].Path);

            //TODO Test, should work since it references the original Object
            _ = Render.CombineBitmap(layer, image, x, y);
        }

        /// <summary>
        ///     Removes the display.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="position">The position.</param>
        public static void RemoveDisplay(int width, int textureSize, Bitmap layer, int position)
        {
            var x = IdToX(position, width) * textureSize;
            var y = IdToY(position, width) * textureSize;

            //TODO Test, should work since it references the original Object
            _ = Render.EraseRectangle(layer, x, y, textureSize, textureSize);
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
                var x = IdToX(step, width) * textureSize;
                var y = IdToY(step, width) * textureSize;

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

        /// <summary>
        ///     Identifiers to x.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        /// <param name="width">The width.</param>
        /// <returns>x coordinate</returns>
        private static int IdToX(int masterId, int width)
        {
            return masterId % width;
        }

        /// <summary>
        ///     Identifiers to y.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        /// <param name="width">The width.</param>
        /// <returns>y coordinate</returns>
        private static int IdToY(int masterId, int width)
        {
            return masterId / width;
        }
    }
}
