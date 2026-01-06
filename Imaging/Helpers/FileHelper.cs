/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Helpers
 * FILE:        FileHelper.cs
 * PURPOSE:     Some minimal helper classes needed for file handling in our Imaging library.
 *              Not nice, but avoids dependencies on other projects.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Imaging.Helpers
{
    /// <summary>
    /// ´Minimal file helper class for Imaging
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>
        /// The number regex
        /// </summary>
        private static readonly Regex NumberRegex = new(@"\d+", RegexOptions.Compiled);

        /// <summary>
        /// Gets the files by extension full path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extensions">The extensions.</param>
        /// <returns>List of files with certain extensions.</returns>
        internal static List<string> GetFilesByExtensionFullPath(
            string path,
            IEnumerable<string> extensions)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return new List<string>();

            var result = new List<string>();

            foreach (var ext in extensions)
            {
                var files = GetFilesByExtension(path, ext);
                if (files != null)
                    result.AddRange(files);
            }

            return result;
        }

        /// <summary>
        /// Gets the files by extension.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>List of files with certain extensions.</returns>
        /// <exception cref="System.ArgumentException">Path must not be empty. - path</exception>
        internal static List<string>? GetFilesByExtension(
            string path,
            string? extension)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must not be empty.", nameof(path));

            if (!Directory.Exists(path))
                return null;

            var searchOption = SearchOption.TopDirectoryOnly;

            // normalize extension
            extension = string.IsNullOrWhiteSpace(extension)
                ? "*"
                : extension.TrimStart('.');

            var pattern = extension == "*"
                ? "*"
                : $"*.{extension}";

            return Directory
                .EnumerateFiles(path, pattern, searchOption)
                .ToList();
        }

        /// <summary>
        /// Sorts the naturally.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>Files sorted naturally</returns>
        internal static List<string> SortNaturally(this IEnumerable<string> paths)
        {
            return paths
                .OrderBy(p => ExtractNumber(Path.GetFileNameWithoutExtension(p)))
                .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Extracts the number.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Extracted numbers.</returns>
        private static int ExtractNumber(string name)
        {
            var match = NumberRegex.Match(name);
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        }
    }
}
