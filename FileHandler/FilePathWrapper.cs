/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FilePathWrapper.cs
 * PURPOSE:     Represents a file path with natural sorting functionality
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * SOURCE:      Inspired by https://www.codeproject.com/Articles/22517/Natural-Sort-Comparer
 */

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FileHandler
{
    /// <summary>
    /// Represents a file path and provides natural sorting functionality.
    /// </summary>
    internal class FilePathWrapper : IComparable<FilePathWrapper>
    {
        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the directory path of the file.
        /// </summary>
        public string? Directory { get; }

        /// <summary>
        /// Precomputed split components of the file name for natural sorting.
        /// </summary>
        private readonly string[] _splitNameParts;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathWrapper"/> class.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public FilePathWrapper(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            FullPath = path;
            FileName = Path.GetFileName(path) ?? string.Empty;
            Directory = Path.GetDirectoryName(path);

            // Precompute split parts for natural sorting
            _splitNameParts = Regex.Split(
                FileName.Replace(FileHandlerResources.Space, string.Empty),
                "([0-9]+)");
        }

        /// <summary>
        /// Compares this instance with another <see cref="FilePathWrapper"/> for natural sorting.
        /// </summary>
        /// <param name="other">The other instance to compare to.</param>
        /// <returns>
        /// A negative number if this instance precedes <paramref name="other"/>,
        /// zero if they are equal,
        /// and a positive number if this instance follows <paramref name="other"/>.
        /// </returns>
        public int CompareTo(FilePathWrapper? other)
        {
            if (other == null) return 1;

            // Quick check for exact equality
            if (string.Equals(FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                return 0;

            var otherParts = other._splitNameParts;
            var minLength = Math.Min(_splitNameParts.Length, otherParts.Length);

            for (var i = 0; i < minLength; i++)
            {
                var part1 = _splitNameParts[i];
                var part2 = otherParts[i];

                if (part1 == part2) continue;

                if (int.TryParse(part1, out var num1) && int.TryParse(part2, out var num2))
                {
                    var numCompare = num1.CompareTo(num2);
                    if (numCompare != 0) return numCompare;
                }
                else
                {
                    var strCompare = string.Compare(part1, part2, StringComparison.OrdinalIgnoreCase);
                    if (strCompare != 0) return strCompare;
                }
            }

            // If all parts matched up to the shorter length, the longer one comes last
            if (_splitNameParts.Length == otherParts.Length)
                return 0;

            return _splitNameParts.Length < otherParts.Length ? -1 : 1;
        }
    }
}
