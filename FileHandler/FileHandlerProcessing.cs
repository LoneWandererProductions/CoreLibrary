/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandlerProcessing.cs
 * PURPOSE:     Helper Methods for the FileHandler library
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileHandler
{
    /// <summary>
    /// Provides internal helper methods for processing file paths and extensions.
    /// </summary>
    internal static class FileHandlerProcessing
    {
        /// <summary>
        /// Cleans up a list of file extensions by removing any leading dots.
        /// </summary>
        /// <param name="fileExtList">The file extension list. Null elements are converted to empty strings.</param>
        /// <returns>A cleaned list of file extensions without dots.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileExtList"/> is null.</exception>
        internal static List<string> CleanUpExtensionList(IEnumerable<string> fileExtList)
        {
            if (fileExtList == null)
            {
                throw new ArgumentNullException(nameof(fileExtList), FileHandlerResources.ErrorFileExtension);
            }

            return fileExtList
                .Select(ext => ext?.Replace(FileHandlerResources.Dot, string.Empty) ?? string.Empty)
                .ToList();
        }

        /// <summary>
        /// Gets the path of a subfolder relative to a root directory and combines it with a target directory.
        /// </summary>
        /// <param name="element">The full path of the element.</param>
        /// <param name="root">The root directory path.</param>
        /// <param name="target">The target directory path to combine with.</param>
        /// <returns>The combined target folder path.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="element"/> is not under <paramref name="root"/>.</exception>
        internal static string GetSubFolder(string element, string root, string target)
        {
            if (string.IsNullOrEmpty(element) || string.IsNullOrEmpty(root) || string.IsNullOrEmpty(target))
                throw new ArgumentException(FileHandlerResources.ErrorInvalidPath);

            var elementDir = Path.GetFullPath(element);
            var rootDir = Path.GetFullPath(root);

            // Use Path.GetRelativePath for cross-platform correctness
            var relativePath = Path.GetRelativePath(rootDir, elementDir);

            // Ensure the element is actually under root
            if (relativePath.StartsWith("..", StringComparison.Ordinal))
            {
                throw new ArgumentException(FileHandlerResources.ErrorInvalidPath);
            }

            return Path.Combine(target, relativePath);
        }

        /// <summary>
        /// Collects all files with a specific extension from a folder.
        /// </summary>
        /// <param name="path">The folder path to search.</param>
        /// <param name="appendix">The file extension to filter by, e.g., "txt". Null or empty uses "*".</param>
        /// <param name="subdirectories">Indicates whether to include subdirectories.</param>
        /// <returns>A list of matching files. Returns an empty list if the folder does not exist.</returns>
        /// <exception cref="FileHandlerException">Thrown when <paramref name="path"/> is null or empty.</exception>
        internal static List<string>? GetFilesByExtension(string path, string? appendix, bool subdirectories)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            if (!Directory.Exists(path))
                return null;

            appendix = string.IsNullOrEmpty(appendix)
                ? FileHandlerResources.Star
                : appendix.Replace(FileHandlerResources.Dot, string.Empty);

            var option = subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory
                .EnumerateFiles(path, $"{FileHandlerResources.StarDot}{appendix}", option)
                .ToList();
        }

        /// <summary>
        /// Returns the shortest path from a collection of paths. Useful for determining a "root" path.
        /// </summary>
        /// <param name="source">A collection of file or folder paths.</param>
        /// <returns>The shortest path, or null if <paramref name="source"/> is empty.</returns>
        internal static string? SearchRoot(IReadOnlyCollection<string> source)
        {
            if (source == null || source.Count == 0)
                return null;

            return source.OrderBy(path => path.Length).First();
        }

        /// <summary>
        /// Validates source and target paths to ensure they are not empty and not equal.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <param name="target">The target path.</param>
        /// <exception cref="FileHandlerException">Thrown if paths are invalid.</exception>
        internal static void ValidatePaths(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
            }

            var normalizedSource = Path.GetFullPath(source)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var normalizedTarget = Path.GetFullPath(target)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.Equals(normalizedSource, normalizedTarget, StringComparison.OrdinalIgnoreCase))
            {
                throw new FileHandlerException(FileHandlerResources.ErrorEqualPath);
            }
        }
    }
}
