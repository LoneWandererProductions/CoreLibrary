/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        FileHandleRename.cs
* PURPOSE:     Utility to Rename Files and Folders
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable MemberCanBeInternal

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileHandler
{
    /// <summary>
    ///     The file handle create class.
    /// </summary>
    public static class FileHandleRename
    {
        /// <summary>
        ///     Rename a Directory.
        /// </summary>
        /// <param name="source">Full qualified location Path</param>
        /// <param name="target">Full qualified target Path</param>
        /// <returns>The <see cref="bool" />Was the Folder Renamed and all contents moved.</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static async Task<bool> RenameDirectory(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
            }

            if (source.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                throw new FileHandlerException(FileHandlerResources.ErrorEqualPath);
            }

            //if nothing exists we can return anyways
            if (!Directory.Exists(source))
            {
                return false;
            }

            try
            {
                await Task.Run(() => Directory.Move(source, target));
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                FileHandlerRegister.AddError(nameof(RenameFile), source, ex);
                Trace.WriteLine(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Rename a file.
        ///     It also overwrites the File if the target already exists, but only if the source file is not locked by another process.
        /// </summary>
        /// <param name="source">Full qualified location File Name</param>
        /// <param name="target">Full qualified target File Name</param>
        /// <returns>The <see cref="bool" />Was the File Renamed.</returns>
        /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
        public static async Task<bool> RenameFile(string source, string target, int maxRetries = 5)
        {
            // 1. Guard Clauses
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            if (source.Equals(target, StringComparison.InvariantCultureIgnoreCase))
                throw new FileHandlerException(FileHandlerResources.ErrorEqualPath);

            if (!File.Exists(source)) return false;

            // 2. Retry Logic Loop
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Move is more reliable inside Task.Run for UI apps
                    await Task.Run(() => File.Move(source, target, overwrite: true));
                    return true;
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    // If we've exhausted retries, log it and give up
                    if (i == maxRetries - 1)
                    {
                        FileHandlerRegister.AddError(nameof(RenameFile), source, new Exception("File remained locked after multiple attempts.", ex));
                        return false;
                    }

                    // Exponential backoff: Wait 100ms, then 200ms, 400ms...
                    int delay = (int)Math.Pow(2, i) * 100;
                    await Task.Delay(delay);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or NotSupportedException)
                {
                    // For non-lock errors (permissions, etc.), don't retry, just log and exit
                    FileHandlerRegister.AddError(nameof(RenameFile), source, ex);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is file locked] [the specified exception].
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <c>true</c> if [is file locked] [the specified exception]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFileLocked(IOException exception)
        {
            int errorCode = exception.HResult & 0xFFFF;
            return errorCode == 32 || errorCode == 33; // 32: Sharing violation, 33: Lock violation
        }
    }
}
