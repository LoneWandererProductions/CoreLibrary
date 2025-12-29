/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        DirectoryInformation.cs
* PURPOSE:     Generic System Functions for Directories
* PROGRAMER:   Peter Geinitz (Wayfarer) 
*/

using System;
using System.Diagnostics;
using System.IO;

namespace FileHandler
{
    /// <summary>
    /// Directory helper class with testable parent retrieval
    /// </summary>
    public static class DirectoryInformation
    {
        /// <summary>
        /// Returns a parent directory at the specified level from the current working directory.
        /// </summary>
        /// <param name="level">Levels to go up</param>
        /// <returns>Parent directory path</returns>
        public static string GetParentDirectory(int level)
            => GetParentDirectoryFromPath(Directory.GetCurrentDirectory(), level);

        /// <summary>
        /// Returns a parent directory at the specified level from a given path.
        /// This method is safe for unit testing with arbitrary paths.
        /// </summary>
        /// <param name="startPath">The path to start from</param>
        /// <param name="level">Levels to go up</param>
        /// <returns>Parent directory path</returns>
        /// <exception cref="FileHandlerException"></exception>
        public static string GetParentDirectoryFromPath(string startPath, int level)
        {
            if (string.IsNullOrEmpty(startPath))
                throw new FileHandlerException($"{FileHandlerResources.ErrorGetParentDirectory}: startPath was empty");

            var path = startPath;

            try
            {
                for (var i = 0; i < level; i++)
                {
                    var parent = Directory.GetParent(path);
                    if (parent == null)
                        throw new FileHandlerException(
                            $"{FileHandlerResources.ErrorGetParentDirectory}: reached root at level {i}");
                    path = parent.FullName;
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException
                                           or FileHandlerException)
            {
                FileHandlerRegister.AddError(nameof(GetParentDirectoryFromPath), path, ex);
                Trace.WriteLine(ex);
                throw new FileHandlerException($"{FileHandlerResources.ErrorGetParentDirectory}: {ex.Message}");
            }

            return path;
        }
    }
}
