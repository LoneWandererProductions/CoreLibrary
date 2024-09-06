/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/CsvHelper.cs
 * PURPOSE:     Shared Helper functions that will be inlined anyways
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DataFormatter
{
    internal static class CsvHelper
    {
        /// <summary>
        /// Shared method to read file content
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns>Content of File</returns>
        /// <exception cref="System.ArgumentException">File path is empty - filepath</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<string> ReadFileContent(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentException("File path is empty", nameof(filepath));
            }

            try
            {
                return ReadText.ReadFile(filepath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Writes the content of the file.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Split string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<List<string>> WriteFileContent(string input, char separator)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return lines
                .Select(line => SplitLine(line, separator))
                .ToList();
        }

        /// <summary>
        /// Shared method to split line by separator
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Split string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<string> SplitLine(string line, char separator)
        {
            return line.Split(separator).Select(part => part.Trim()).ToList();
        }

        /// <summary>
        /// Shared method to write content to file
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="content">The content.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteContentToFile(string filepath, StringBuilder content)
        {
            try
            {
                File.WriteAllText(filepath, content.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
    }
}
