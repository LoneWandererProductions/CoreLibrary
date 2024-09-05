using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Imaging
{
    public static class LifProcessing
    {
        // Save the Lif object (layers and settings) to a binary file
        public static void SaveLif(Lif lif, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, lif);
            }
        }

        // Load the Lif object (layers and settings) from a binary file
        public static Lif LoadLif(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Lif)formatter.Deserialize(fs);
            }
        }

        // Convert a Bitmap to a CIF (Compressed Image Format) dictionary
        public static Dictionary<Color, List<int>> ConvertToCifFromBitmap(Bitmap bitmap)
        {
            Dictionary<Color, List<int>> cif = new Dictionary<Color, List<int>>();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    int pixelIndex = y * bitmap.Width + x;

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
            Bitmap bitmap = new Bitmap(width, height);

            foreach (var entry in cif)
            {
                Color color = entry.Key;
                List<int> pixels = entry.Value;

                foreach (int pixel in pixels)
                {
                    int x = pixel % width;
                    int y = pixel / width;
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }
    }
}
