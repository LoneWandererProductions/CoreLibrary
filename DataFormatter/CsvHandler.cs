/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/CsvHandler.cs
 * PURPOSE:     SImple Csv reader/writer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataFormatter
{
    public static class CsvHandler
    {
        /// <summary>
        ///     Try to read a CSV file.
        /// </summary>
        /// <param name="filepath">Path of the CSV file.</param>
        /// <param name="separator">The separator in use.</param>
        /// <returns>IEnumerable if successful; otherwise, null.</returns>
        [return: MaybeNull]
        public static List<List<string>> ReadCsv(string filepath, char separator)
        {
            var lst = CsvHelper.ReadFileContent(filepath);
            return lst?.ConvertAll(item => CsvHelper.SplitLine(item, separator));
        }

        /// <summary>
        ///     Parses the entire CSV file as a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to parse.</typeparam>
        /// <param name="filepath">The path of the CSV file.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>A list of objects of type T.</returns>
        public static List<T> ReadCsv<T>(string filepath, char separator) where T : new()
        {
            return ReadCsvRange(filepath, separator, typeof(T), 0, int.MaxValue).Cast<T>().ToList();
        }

        /// <summary>
        ///     Parses a CSV file with specified types for different line ranges.
        /// </summary>
        /// <param name="filepath">The path of the CSV file.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="ranges">The list of line ranges and corresponding types.</param>
        /// <returns>A list of parsed objects for each range.</returns>
        public static List<object> ParseCsvWithRanges(string filepath, char separator, List<(Type type, int startLine, int endLine)> ranges)
        {
            var result = new List<object>();

            foreach (var (type, startLine, endLine) in ranges)
            {
                result.AddRange(ReadCsvRange(filepath, separator, type, startLine, endLine));
            }

            return result;
        }

        /// <summary>
        ///     Helper method that parses a CSV file within a specific line range as objects of a given type.
        /// </summary>
        /// <param name="filepath">The path of the CSV file.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="type">The type of objects to parse.</param>
        /// <param name="startLine">The starting line (inclusive).</param>
        /// <param name="endLine">The ending line (inclusive).</param>
        /// <returns>A list of parsed objects of the specified type.</returns>
        private static List<object> ReadCsvRange(string filepath, char separator, Type type, int startLine, int endLine)
        {
            var lst = ReadText.ReadFile(filepath);
            if (lst == null || lst.Count == 0 || startLine > endLine || startLine >= lst.Count)
            {
                return null;
            }

            var result = new List<object>();

            var properties = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(CsvColumnAttribute)))
                .ToArray();

            for (int i = startLine; i <= Math.Min(endLine, lst.Count - 1); i++)
            {
                var parts = DataHelper.GetParts(lst[i], separator).ConvertAll(s => s.Trim());
                var obj = Activator.CreateInstance(type);
                MapPartsToObject(parts, obj, properties);
                result.Add(obj);
            }

            return result;
        }

        /// <summary>
        /// Helper method to map CSV parts to object properties.
        /// </summary>
        private static void MapPartsToObject(List<string> parts, object obj, PropertyInfo[] properties)
        {
            foreach (var property in properties)
            {
                var attribute = (CsvColumnAttribute)Attribute.GetCustomAttribute(property, typeof(CsvColumnAttribute));
                if (attribute == null) continue;

                var converter = (IAttributeConverter)Activator.CreateInstance(attribute.ConverterType);
                if (converter == null) continue;

                var value = converter.Convert(parts[attribute.Index]);
                property.SetValue(obj, value);
            }
        }

        /// <summary>
        ///     Writes the CSV data into a file.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="csv">The CSV data.</param>
        /// <param name="separator">Possible file splitter, if not set;</param>
        public static void WriteCsv(string filepath, IEnumerable<List<string>> csv, string separator = ",")
        {
            var file = new StringBuilder();

            foreach (var line in csv.Select(row => string.Join(separator, row)))
            {
                file.AppendLine(line);
            }

            CsvHelper.WriteContentToFile(filepath, file);
        }
    }
}

