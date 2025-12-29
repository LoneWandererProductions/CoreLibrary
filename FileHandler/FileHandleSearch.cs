/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandleSearch.cs
 * PURPOSE:     Handles all types of file searches (refactored for clarity and safety)
 * PROGRAMER:   Peter Geinitz (Wayfarer) 
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileHandler
{
    /// <summary>
    ///     Handles most file searches.
    /// </summary>
    public static class FileHandleSearch
    {
        /// <summary>
        /// Collects all files with specific extensions (full path).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="appendix">The appendix.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <returns>File by criteria</returns>
        public static List<string> GetFilesByExtensionFullPath(string path, IEnumerable<string> appendix,
            bool subdirectories)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return new List<string>();

            var lst = new List<string>();

            foreach (var app in appendix)
            {
                var files = FileHandlerProcessing.GetFilesByExtension(path, app, subdirectories) ?? new List<string>();
                lst.AddRange(files);
            }

            return lst;
        }

        /// <summary>
        /// Collects all files with a specific extension (full path).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="appendix">The appendix.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <returns>File by criteria</returns>
        public static List<string> GetFilesByExtensionFullPath(string path, string appendix, bool subdirectories)
        {
            return FileHandlerProcessing.GetFilesByExtension(path, appendix, subdirectories) ?? new List<string>();
        }

        /// <summary>
        /// Collects file names with extension only.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="appendix">The appendix.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <returns>File by criteria</returns>
        public static List<string> GetFileByExtensionWithExtension(string path, string appendix, bool subdirectories)
        {
            var files = FileHandlerProcessing.GetFilesByExtension(path, appendix, subdirectories) ?? new List<string>();
            return files.Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Collects file names without path, with extension.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="appendix">The appendix.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <returns>File by criteria</returns>
        public static List<string> GetFileByExtensionWithoutExtension(string path, string appendix, bool subdirectories)
        {
            var files = FileHandlerProcessing.GetFilesByExtension(path, appendix, subdirectories) ?? new List<string>();
            return files.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        /// <summary>
        /// Get all files in a folder (optionally including subfolders).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <returns>File by criteria</returns>
        public static List<string> GetAllFiles(string path, bool subdirectories)
        {
            return FileHandlerProcessing.GetFilesByExtension(path, null, subdirectories) ?? new List<string>();
        }

        /// <summary>
        /// Get details of a single file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>File Details</returns>
        public static FileDetails? GetFileDetails(string path)
        {
            if (!File.Exists(path))
                return null;

            var fileInfo = FileVersionInfo.GetVersionInfo(path);
            var fi = new FileInfo(path);

            return new FileDetails
            {
                Path = path,
                FileName = fi.Name,
                OriginalFilename = fileInfo.OriginalFilename,
                Extension = fi.Extension,
                Size = fi.Length,
                Description = fileInfo.FileDescription,
                CompanyName = fileInfo.CompanyName,
                ProductName = fileInfo.ProductName,
                FileVersion = fileInfo.FileVersion,
                ProductVersion = fileInfo.ProductVersion
            };
        }

        /// <summary>
        /// Get details of multiple files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>File Details</returns>
        public static List<FileDetails> GetFilesDetails(List<string>? files)
        {
            if (files == null || files.Count == 0)
                return new List<FileDetails>();

            return files.Select(GetFileDetails).Where(f => f != null).ToList()!;
        }

        /// <summary>
        /// Get immediate subfolders.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Subfolders</returns>
        /// <exception cref="FileHandler.FileHandlerException"></exception>
        public static List<string> GetAllSubfolders(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            if (!Directory.Exists(path))
                return new List<string>();

            return Directory.GetDirectories(path).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Check if a folder contains any file or folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Check if folder is empty.</returns>
        /// <exception cref="FileHandler.FileHandlerException"></exception>
        public static bool CheckIfFolderContainsElement(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            if (!Directory.Exists(path))
                return false;

            return Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// Get files containing a specific substring (or inverse).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="appendix">The appendix.</param>
        /// <param name="subdirectories">if set to <c>true</c> [subdirectories].</param>
        /// <param name="subString">The sub string.</param>
        /// <param name="invert">if set to <c>true</c> [invert].</param>
        /// <returns>All files with a specific substring.</returns>
        public static List<string> GetFilesWithSubString(string path, IEnumerable<string> appendix, bool subdirectories,
            string subString, bool invert)
        {
            var lst = GetFilesByExtensionFullPath(path, appendix, subdirectories);

            if (lst.Count == 0)
                return new List<string>();

            return lst.Where(element =>
            {
                var file = Path.GetFileName(element);
                return invert ? !file.Contains(subString) : file.Contains(subString);
            }).ToList();
        }
    }
}
