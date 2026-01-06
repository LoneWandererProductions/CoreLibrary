/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        FormatterApiFacade.cs
 * PURPOSE:     Api Façade for the DataFormatter library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataFormatter
{
    /// <summary>
    /// Central API Façade for the DataFormatter library
    /// </summary>
    public static class FormatterApiFacade
    {
        /// <summary>
        /// Reads the CSV.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sep">The sep.</param>
        /// <returns>Csv file as list.</returns>
        public static List<List<string>> ReadCsv(string file, char sep) => CsvHandler.ReadCsv(file, sep);

        /// <summary>
        /// Writes the CSV.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="sep">The sep.</param>
        public static void WriteCsv(string file, IEnumerable<List<string>> data, string sep) =>
            CsvHandler.WriteCsv(file, data, sep);

        /// <summary>
        /// Reads the CSV with layers.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="layerKeyword">The layer keyword.</param>
        /// <returns>Content of our special format file</returns>
        public static List<string> ReadCsvWithLayers(string file, string layerKeyword)
            => SegmentedCsvHandler.ReadCsvWithLayerKeywords(file, layerKeyword);

        /// <summary>
        /// Writes the CSV with layers.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="sep">The sep.</param>
        /// <param name="csvLayers">The CSV layers.</param>
        /// <param name="layerKeyword">The layer keyword.</param>
        public static void WriteCsvWithLayers(string file, char sep, List<List<string>> csvLayers, string layerKeyword)
            => SegmentedCsvHandler.WriteCsvWithLayerKeywords(file, sep, csvLayers, layerKeyword);

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>the values as String[]. Can return null.</returns>
        public static IEnumerable<string>? ReadFile(string path) => ReadText.ReadFile(path);

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="content">The content.</param>
        public static async Task WriteFile(string path, IEnumerable<string> content) => await ReadText.WriteFile(path, content);

        /// <summary>
        /// Reads the object.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Readable Obj File</returns>
        public static ObjFile? ReadObj(string path) => ReaderObj.ReadObj(path);
    }
}
