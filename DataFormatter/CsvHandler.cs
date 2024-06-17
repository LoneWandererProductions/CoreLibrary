using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace DataFormatter
{
    /// <summary>
    ///     Basic CSV Reader/Writer
    /// </summary>
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
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentException(DataFormatterResources.ThrowFileEmpty, nameof(filepath));
            }

            try
            {
                var lst = ReadText.ReadFile(filepath);
                if (lst == null)
                {
                    return null;
                }

                var enums = new List<List<string>>(lst.Count);

                enums.AddRange(lst.Select(item => DataHelper.GetParts(item, separator))
                    .Select(subs => subs.ConvertAll(s => s.Trim())));

                return enums;
            }
            catch (Exception ex)
            {
                // Log exception or handle it as needed
                Trace.WriteLine(string.Concat(DataFormatterResources.ErrorFileEmpty, ex.Message));
                return null;
            }
        }

        /// <summary>
        ///     Reads a CSV file and maps it to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of objects to create.</typeparam>
        /// <param name="filepath">The path of the CSV file.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>A list of objects of type T.</returns>
        public static List<T> ReadCsv<T>(string filepath, char separator) where T : new()
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentException(DataFormatterResources.ThrowFileEmpty, nameof(filepath));
            }

            try
            {
                var lst = ReadText.ReadFile(filepath);
                if (lst == null)
                {
                    return null;
                }

                var result = new List<T>(lst.Count);
                var properties = typeof(T).GetProperties()
                    .Where(p => Attribute.IsDefined(p, typeof(CsvColumnAttribute)))
                    .ToArray();

                foreach (var item in lst)
                {
                    var parts = DataHelper.GetParts(item, separator);
                    var trimmedParts = parts.ConvertAll(s => s.Trim());
                    var obj = new T();

                    foreach (var property in properties)
                    {
                        var attribute = (CsvColumnAttribute)Attribute.GetCustomAttribute(property, typeof(CsvColumnAttribute));
                        if (attribute == null)
                        {
                            continue;
                        }

                        var converter = (IAttributeConverter)Activator.CreateInstance(attribute.ConverterType);

                        if (converter == null)
                        {
                            continue;
                        }

                        var value = converter.Convert(trimmedParts[attribute.Index]);
                        property.SetValue(obj, value);
                    }

                    result.Add(obj);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log exception or handle it as needed
                Trace.WriteLine(string.Concat(DataFormatterResources.ErrorFileEmpty, ex.Message));
                return null;
            }
        }

        /// <summary>
        ///     Writes the CSV data into a file.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="csv">The CSV data.</param>
        /// <param name="separator">Possible file splitter, if not set;</param>
        public static void WriteCsv(string filepath, List<List<string>> csv, string separator = DataFormatterResources.Splitter)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentException(DataFormatterResources.ThrowFileEmpty, nameof(filepath));
            }

            if (csv == null)
            {
                throw new ArgumentNullException(nameof(csv), DataFormatterResources.ErrorDataEmpty);
            }

            try
            {
                var file = new StringBuilder();

                for (var i = 0; i < csv.Count; i++)
                {
                    var row = csv[i];
                    var line = string.Empty;

                    for (var j = 0; j < row.Count; j++)
                    {
                        var cache = row[j];
                        line = j != row.Count - 1
                            ? string.Concat(line, cache, separator)
                            : string.Concat(line, cache);
                    }

                    _ = i != row.Count - 1 ? file.Append(line).Append(Environment.NewLine) : file.Append(line);
                }

                File.WriteAllText(filepath, file.ToString());
            }
            catch (Exception ex)
            {
                // Log exception or handle it as needed
                Trace.WriteLine($"Error writing CSV file: {ex.Message}");
            }
        }
    }
}
