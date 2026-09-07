/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileUtility.cs
 * PURPOSE:     Some Basic helper Functions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System.IO;

namespace FileHandler
{
    public static class FileUtility
    {
        /// <summary>
        /// Returns a new file path if the given file already exists by appending a numeric suffix.
        /// Example: file.txt → file(0).txt, file(1).txt, etc.
        /// </summary>
        /// <param name="path">The full file path.</param>
        /// <returns>New unique file path, or null if path is invalid.</returns>
        public static string? GetNewFileName(string path)
        {
            if (!File.Exists(path))
                return null;

            var directory = Path.GetDirectoryName(path);
            var fileNameOnly = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var newPath = path;
            var count = 0;

            while (File.Exists(newPath))
            {
                newPath = Path.Combine(directory, fileNameOnly + "(" + count + ")" + extension);
                count++;
            }

            return newPath;
        }
    }
}