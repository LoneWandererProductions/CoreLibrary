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
        internal static BitmapImage GenerateImage(int width, int height, int textureSize, List<Texture> textures,
            IEnumerable<int> map)
        {
            var background = new Bitmap(width * textureSize, height * textureSize);

            var boxes = new List<Box>();

            var layers = map.ChunkBy(height);

            for (var y = 0; y < layers.Count; y++)
            {
                var slice = layers[y];

                for (var x = 0; x < slice.Count; x++)
                {
                    var id = slice[x];
                    if (id <= 0)
                    {
                        continue;
                    }

                    var texture = textures[id];
                    var image = Render.GetBitmapFile(texture.Path);

                    var box = new Box {X = x * textureSize, Y = y * textureSize, Image = image, Layer = texture.Layer};

                    boxes.Add(box);
                }
            }

            background = boxes.OrderBy(layer => layer.Layer).ToList().Aggregate(background,
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
    }
}
