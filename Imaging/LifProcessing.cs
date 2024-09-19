using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Imaging
{
    public static class LifProcessing
    {
        //TODO use for delta!

        /// <summary>
        ///     Ares the color counts similar.
        ///     A Color Histogram needed for delta calculation if it is even useful
        /// </summary>
        /// <param name="colorCount1">The color count1.</param>
        /// <param name="colorCount2">The color count2.</param>
        /// <param name="threshold">The threshold.</param>
        /// <returns>Similarity</returns>
        public static bool AreColorCountsSimilar(Dictionary<Color, int> colorCount1, Dictionary<Color, int> colorCount2,
            double threshold = 0.95)
        {
            var totalPixels1 = colorCount1.Values.Sum();
            var totalPixels2 = colorCount2.Values.Sum();

            var similarPixels = 0;

            foreach (var color in colorCount1.Keys)
            {
                if (colorCount2.ContainsKey(color))
                {
                    similarPixels += Math.Min(colorCount1[color], colorCount2[color]);
                }
            }

            var similarity = (double)similarPixels / Math.Min(totalPixels1, totalPixels2);
            return similarity >= threshold;
        }


        // Save the Lif object (layers and settings) to a binary file
        public static void SaveLif(Lif lif, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, lif);
            }
        }

        // Load the Lif object (layers and settings) from a binary file
        public static Lif LoadLif(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                return (Lif)formatter.Deserialize(fs);
            }
        }

        // Convert a Bitmap to a CIF (Compressed Image Format) dictionary
        public static Dictionary<Color, List<int>> ConvertToCifFromBitmap(Bitmap bitmap)
        {
            var cif = new Dictionary<Color, List<int>>();

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixelColor = bitmap.GetPixel(x, y);
                    var pixelIndex = (y * bitmap.Width) + x;

                    // Group pixels by color
                    if (!cif.ContainsKey(pixelColor))
                    {
                        cif[pixelColor] = new List<int>();
                    }

                    cif[pixelColor].Add(pixelIndex);
                }
            }

            return cif;
        }

        // Convert a CIF dictionary back into a Bitmap object
        public static Bitmap ConvertToBitmapFromCif(Dictionary<Color, List<int>> cif, int width, int height)
        {
            var bitmap = new Bitmap(width, height);

            foreach (var entry in cif)
            {
                var color = entry.Key;
                var pixels = entry.Value;

                foreach (var pixel in pixels)
                {
                    var x = pixel % width;
                    var y = pixel / width;
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }
    }
}
