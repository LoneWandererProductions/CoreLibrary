/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        FileHandleCopy.cs
* PURPOSE:     Does all types of File Operations, Copy Files
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeBraces_foreach
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileHandler
{
    /// <summary>
    ///     The file handle copy class.
    /// </summary>
    public static class FileHandleCopy
    {
        /// <summary>
        ///     Copies all Files to another Location, includes subdirectories
        ///     Example: https://msdn.microsoft.com/de-de/library/bb762914%28v=vs.110%29.aspx
        /// </summary>
        /// <param name="source">Full qualified location Path</param>
        /// <param name="target">Full qualified target Path</param>
        /// <param name="overwrite">Is overwrite allowed, optional, default true</param>
        /// <returns>Status if we encountered any problems</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static bool CopyFiles(string source, string target, bool overwrite = true)
        {
            FileHandlerProcessing.ValidatePaths(source, target);

            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);

            if (!Directory.Exists(source))
                return false;

            var srcDir = new DirectoryInfo(source);
            var tgtDir = new DirectoryInfo(target);

            if (!tgtDir.Exists)
                tgtDir.Create();

            bool success = true;
            CopyDirectoryRecursive(srcDir, tgtDir, overwrite, ref success);
            return success;
        }

        /// <summary>
        ///     Copies all Files to another Location, includes subdirectories
        /// </summary>
        /// <param name="source">Full qualified location Path</param>
        /// <param name="target">List of Files</param>
        /// <param name="overwrite">Is overwrite allowed, optional, default true.</param>
        /// <returns>Status if we encountered any problems</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static bool CopyFiles(List<string> source, string target, bool overwrite = true)
        {
            if (source == null || source.Count == 0 || string.IsNullOrEmpty(target))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            // Normalize paths
            target = Path.GetFullPath(target);
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            // Overview event
            var overview = new FileItems
            {
                Elements = new List<string>(source), Message = FileHandlerResources.InformationFileDeletion
            };
            FileHandlerRegister.SendOverview?.Invoke(nameof(CopyFiles), overview);

            // Determine root directory automatically
            var root = FileHandlerProcessing.SearchRoot(source);
            root = Path.GetFullPath(new FileInfo(root).Directory!.FullName);

            bool success = true;

            foreach (var filePath in source)
            {
                var file = new FileInfo(filePath);
                var relative = Path.GetRelativePath(root, file.Directory!.FullName);
                var destDir = Path.Combine(target, relative);

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                var dest = Path.Combine(destDir, file.Name);

                if (!TryCopyFile(file, dest, overwrite))
                    success = false;
            }

            return success;
        }


        /// <summary>
        ///     Copies all Files to another Location, includes subdirectories, does not Replace the files but returns a List of
        ///     Files that might be overwritten.
        /// </summary>
        /// <param name="source">Full qualified location Path</param>
        /// <param name="target">Full qualified target Path</param>
        /// <returns>List of Files that were not copied, can be null.</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static IList<string>? CopyFiles(string source, string target)
        {
            FileHandlerProcessing.ValidatePaths(source, target);

            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);

            if (!Directory.Exists(source))
                return null;

            var sourceFiles = FileHandlerProcessing.GetFilesByExtension(source, FileHandlerResources.AllFiles,
                FileHandlerResources.SubFolders);
            if (sourceFiles == null)
                return null;

            var targetFiles = FileHandlerProcessing.GetFilesByExtension(target, FileHandlerResources.AllFiles,
                FileHandlerResources.SubFolders);
            if (targetFiles == null)
                return null;

            var intersect = sourceFiles.Intersect(targetFiles).ToList();
            var except = sourceFiles.Except(targetFiles).ToList();

            if (intersect.Count == 0)
                return except.Count == 0 ? null : except;

            // Perform copy without overwrite
            if (!CopyFiles(intersect, target, false))
                return null;

            return except.Count == 0 ? null : except;
        }

        /// <summary>
        ///     Copies all Files to another Location, includes subdirectories, but only replace if newer
        /// </summary>
        /// <param name="source">Full qualified location Path</param>
        /// <param name="target">Full qualified target Path</param>
        /// <returns>Status if we encountered any problems</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static bool CopyFilesReplaceIfNewer(string source, string target)
        {
            FileHandlerProcessing.ValidatePaths(source, target);

            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);

            if (!Directory.Exists(source))
                return false;

            bool success = true;

            var dir = new DirectoryInfo(source);
            var files = dir.GetFiles();

            // Overview callback
            var overview = new FileItems
            {
                Elements = files.Select(f => f.Name).ToList(),
                Message = FileHandlerResources.InformationFileDeletion
            };
            FileHandlerRegister.SendOverview?.Invoke(nameof(CopyFiles), overview);

            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            foreach (var file in files)
            {
                var dest = Path.Combine(target, file.Name);

                // Replace only if newer
                try
                {
                    if (File.Exists(dest))
                    {
                        var destTime = File.GetLastWriteTime(dest);
                        if (file.LastWriteTime <= destTime)
                            continue;
                    }

                    if (!TryCopyFile(file, dest, true))
                        success = false;
                }
                catch (Exception ex)
                {
                    FileHandlerRegister.AddError(nameof(CopyFilesReplaceIfNewer), file.FullName, ex);
                    Trace.WriteLine(ex);
                    success = false;
                }
            }

            // Recurse into subdirs
            foreach (var sub in dir.GetDirectories())
            {
                var newTarget = Path.Combine(target, sub.Name);
                if (!CopyFilesReplaceIfNewer(sub.FullName, newTarget))
                    success = false;
            }

            return success;
        }

        /// <summary>
        /// Copies the directory recursive.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <returns>Success Status per operation.</returns>
        private static void CopyDirectoryRecursive(
            DirectoryInfo source,
            DirectoryInfo target,
            bool overwrite,
            ref bool success)
        {
            // Copy files in this directory
            foreach (var file in source.GetFiles())
            {
                try
                {
                    var dest = Path.Combine(target.FullName, file.Name);
                    file.CopyTo(dest, overwrite);
                    FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                               or NotSupportedException)
                {
                    success = false;
                    FileHandlerRegister.AddError(nameof(CopyFiles), file.Name, ex);
                    Trace.WriteLine(ex);
                }
            }

            // Recurse subdirectories
            foreach (var subDir in source.GetDirectories())
            {
                var newTarget = new DirectoryInfo(Path.Combine(target.FullName, subDir.Name));
                if (!newTarget.Exists)
                    newTarget.Create();

                CopyDirectoryRecursive(subDir, newTarget, overwrite, ref success);
            }
        }

        /// <summary>
        /// Tries the copy file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns>Status of copy process.</returns>
        private static bool TryCopyFile(FileInfo file, string destination, bool overwrite)
        {
            try
            {
                file.CopyTo(destination, overwrite);
                FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                           or NotSupportedException)
            {
                FileHandlerRegister.AddError(nameof(CopyFiles), file.FullName, ex);
                Trace.WriteLine(ex);
                return false;
            }
        }
    }
}
