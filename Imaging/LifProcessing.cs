using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DataFormatter;

namespace Imaging
{
    internal static class LifProcessing
    {
        // Load LIF layers from a file
        internal static List<Dictionary<Color, SortedSet<int>>> LoadLif(string path)
        {
            var cifLayers = new List<Dictionary<Color, SortedSet<int>>>();
            var lines = File.ReadAllLines(path);

            int currentLine = 0;

            while (currentLine < lines.Length)
            {
                var layerPath = lines[currentLine++];
                var cif = CifProcessing.CifFromFile(layerPath);

                if (cif != null && cif.CifImage != null)
                {
                    cifLayers.Add(cif.CifImage);
                }
            }

            return cifLayers;
        }

        // Generate a Bitmap image from LIF layers
        internal static Bitmap? GenerateImageFromLif(string path)
        {
            var layers = LoadLif(path);
            if (layers.Count == 0) return null;

            var baseLayer = layers.FirstOrDefault();

            if (baseLayer == null) return null;

            var width = layers.Max(layer => layer.Values.SelectMany(s => s).Max() % width + 1);
            var height = layers.Max(layer => layer.Values.SelectMany(s => s).Max() / width + 1);
            var image = new Bitmap(width, height);
            var dbm = DirectBitmap.GetInstance(image);

            // Apply base layer
            foreach (var (color, ids) in baseLayer)
            {
                dbm.SetArea(ids, color);
            }

            // Apply subsequent delta layers
            foreach (var layer in layers.Skip(1))
            {
                if (layer == null) continue;

                // Apply changes from delta layer
                foreach (var (color, ids) in layer)
                {
                    dbm.SetArea(ids, color);
                }
            }

            return dbm.Bitmap;
        }

        // Save an image as a LIF file
        internal static void SaveImageAsLif(Bitmap image, string path)
        {
            var cifLayers = new List<string>();
            var baseCifPath = Path.GetTempFileName();
            cifLayers.Add(baseCifPath);

            // Convert and save base CIF layer
            var baseCif = CifProcessing.ConvertToCifFromBitmap(image);
            var width = image.Width;
            var height = image.Height;
            SaveCifToFile(baseCif, baseCifPath, width, height);

            // Delta Layer: Example implementation
            var deltaCifPath = Path.GetTempFileName();
            cifLayers.Add(deltaCifPath);
            // Here you should create and save delta CIF file (not implemented in this example)
            // var deltaCif = GenerateDeltaCif(baseCif, ...);
            // SaveCifToFile(deltaCif, deltaCifPath);

            // Write LIF file with layer paths
            File.WriteAllLines(path, cifLayers);
        }

        // Save CIF data to a file
        private static void SaveCifToFile(Dictionary<Color, SortedSet<int>> cifData, string path, int width, int height)
        {
            var csvData = CifProcessing.GenerateCsv(height, width, cifData);
            CsvHandler.WriteCsv(path, csvData);
        }

        // Generate a delta CIF file (stub method)
        private static Dictionary<Color, SortedSet<int>> GenerateDeltaCif(Dictionary<Color, SortedSet<int>> baseCif, Bitmap image)
        {
            // Implement delta calculation logic here
            return new Dictionary<Color, SortedSet<int>>();
        }
    }
}
