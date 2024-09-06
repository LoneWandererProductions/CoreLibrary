/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/CvsLayeredHandler.cs
 * PURPOSE:     My custom format, it is a collection of csv files separated with an keyword. Mostly needed for my Lif file format
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataFormatter
{
    public static class CvsLayeredHandler
    {
        /// <summary>
        /// Writes the CSV with layer keywords.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="csvLayers">The CSV layers.</param>
        /// <param name="layerKeyword">The layer keyword.</param>
        public static void WriteCsvWithLayerKeywords(string filepath, char separator, List<List<string>> csvLayers, string layerKeyword)
        {
            var file = new StringBuilder();

            for (var layerIndex = 0; layerIndex < csvLayers.Count; layerIndex++)
            {
                foreach (var row in csvLayers[layerIndex])
                {
                    var line = string.Join(separator, row);
                    file.AppendLine(line);
                }

                // Add the layer keyword at the end
                file.AppendLine($"{layerKeyword}{layerIndex}");
            }

            CsvHelper.WriteContentToFile(filepath, file);
        }

        /// <summary>
        /// Reads the CSV with layer keywords.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="layerKeyword">The layer keyword.</param>
        /// <returns>Content of oure special format file</returns>
        public static List<List<List<string>>> ReadCsvWithLayerKeywords(string filepath, char separator, string layerKeyword)
        {
            var lst = CsvHelper.ReadFileContent(filepath);
            if (lst == null) return null;

            var layers = new List<List<List<string>>>();
            var currentLayer = new List<List<string>>();

            foreach (var line in lst)
            {
                if (line.StartsWith(layerKeyword))
                {
                    layers.Add(currentLayer);
                    currentLayer = new List<List<string>>();
                }
                else
                {
                    var parts = CsvHelper.SplitLine(line, separator);
                    currentLayer.Add(parts);
                }
            }

            if (currentLayer.Any()) layers.Add(currentLayer);

            return layers;
        }
    }
}
