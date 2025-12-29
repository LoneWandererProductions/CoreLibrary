/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileNameConverter.cs
 * PURPOSE:     Helps to perform some generic renaming operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

//TODO Rollback for all Features

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileHandler
{
    /// <summary>
    /// Provides batch file renaming utilities for files within a given folder.
    /// </summary>
    public static class FileNameConverter
    {
        /// <summary>
        /// Removes a predefined appendage from all files in the folder that start with it.
        /// </summary>
        /// <param name="appendage">The prefix to remove from the files.</param>
        /// <param name="folder">The root folder to search in.</param>
        /// <param name="subFolder">If <c>true</c>, search recursively in subfolders.</param>
        /// <returns>The number of files successfully renamed.</returns>
        public static Task<int> RemoveAppendage(string appendage, string folder, bool subFolder) =>
            RenameFiles(folder, subFolder, name => name.RemoveAppendage(appendage));

        /// <summary>
        /// Adds a predefined appendage to all files in the folder that do not already start with it.
        /// </summary>
        /// <param name="appendage">The prefix to add to the files.</param>
        /// <param name="folder">The root folder to search in.</param>
        /// <param name="subFolder">If <c>true</c>, search recursively in subfolders.</param>
        /// <returns>The number of files successfully renamed.</returns>
        public static Task<int> AddAppendage(string appendage, string folder, bool subFolder) =>
            RenameFiles(folder, subFolder, name => name.AddAppendage(appendage));

        /// <summary>
        /// Replaces part of a file name with a new value.
        /// </summary>
        /// <param name="targetStr">The portion of the file name that should be replaced.</param>
        /// <param name="update">The replacement text.</param>
        /// <param name="folder">The root folder to search in.</param>
        /// <param name="subFolder">If <c>true</c>, search recursively in subfolders.</param>
        /// <returns>The number of files successfully renamed.</returns>
        public static Task<int> ReplacePart(string targetStr, string update, string folder, bool subFolder) =>
            RenameFiles(folder, subFolder, name => name.ReplacePart(targetStr, update));

        /// <summary>
        /// Removes all numbers from file names and re-appends them at the end,
        /// separated by an underscore. This preserves order based on extracted numbers.
        /// Intended for specific use cases where numeric sequences matter.
        /// </summary>
        /// <param name="folder">The root folder to search in.</param>
        /// <param name="subFolder">If <c>true</c>, search recursively in subfolders.</param>
        /// <returns>The number of files successfully renamed.</returns>
        public static Task<int> ReOrderNumbers(string folder, bool subFolder) =>
            RenameFiles(folder, subFolder, name =>
            {
                var ext = Path.GetExtension(name);
                var core = Path.GetFileNameWithoutExtension(name)?.ReOrderNumbers();
                return string.IsNullOrEmpty(core) ? name : string.Concat(core, ext);
            });

        /// <summary>
        /// Internal helper that applies a rename transformation to each file found in the folder.
        /// </summary>
        /// <param name="folder">The root folder to search in.</param>
        /// <param name="subFolder">If <c>true</c>, search recursively in subfolders.</param>
        /// <param name="renameSelector">
        /// A rename selector that takes the original file name (without path) and
        /// returns the updated file name. Returning <c>null</c>, empty string,
        /// or the same name means the file will be skipped.
        /// </param>
        /// <returns>The number of files successfully renamed.</returns>
        /// <remarks>
        /// This method skips renaming if:
        /// <list type="bullet">
        /// <item><description>The selector returns <c>null</c> or empty.</description></item>
        /// <item><description>The new name matches the original (case-insensitive).</description></item>
        /// <item><description>The destination file already exists.</description></item>
        /// </list>
        /// If a rename fails, the process stops, and the finished count is returned.
        /// </remarks>
        private static async Task<int> RenameFiles(
            string folder,
            bool subFolder,
            Func<string, string> renameSelector)
        {
            var lst = FileHandleSearch.GetAllFiles(folder, subFolder);

            if (lst == null || lst.Count == 0)
                return 0;

            var count = 0;

            foreach (var path in lst)
            {
                var name = Path.GetFileName(path);
                var directory = Path.GetDirectoryName(path);

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(directory))
                    continue;

                var updated = renameSelector(name);

                // ignore if the selector returns null, same, or empty
                if (string.IsNullOrEmpty(updated) ||
                    string.Equals(name, updated, StringComparison.OrdinalIgnoreCase))
                    continue;

                var target = Path.Combine(directory, updated);

                // Check if the target file already exists
                if (File.Exists(target))
                {
                    Trace.WriteLine($"{FileHandlerResources.ErrorFileAlreadyExists} {target}");
                    continue;
                }

                var ok = await FileHandleRename.RenameFile(path, target);
                if (!ok)
                {
                    Trace.WriteLine(path);
                    return count;
                }

                count++;
            }

            return count;
        }
    }
}
