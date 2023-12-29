using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        /// Generates the image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <param name="textures">The textures.</param>
        /// <param name="map">The map.</param>
        /// <returns>The generated Map</returns>
        internal static BitmapImage GenerateImage(int width, int height, int textureSize, Dictionary<int, Texture>textures,
            Dictionary<int, List<int>> map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var tiles = (from tile in map from layer in tile.Value select new Box {X = IdToX(tile.Key, width), Y = IdToY(tile.Key, width), Layer = textures[tile.Key].Layer, Image = Render.GetBitmapFile(textures[tile.Key].Path)}).ToList();


            background = tiles.OrderBy(layer => layer.Layer).ToList().Aggregate(background,
                (current, slice) => Render.CombineBitmap(current, slice.Image, slice.X, slice.Y));

            return background.ToBitmapImage();
        }

        /// <summary>
        /// Generates the grid.
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
        /// Generates the numbers.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="textureSize">Size of the texture.</param>
        /// <returns>A number overlay</returns>
        public static ImageSource GenerateNumbers(int width, int height, int textureSize)
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
