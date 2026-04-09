/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandleSafeDelete.cs
 * PURPOSE:     Variation of delete files, with Progress Bar and Deletion to Recycle Bin
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace FileHandler
{
    /// <summary>
    /// Save files to Recycle Bin instead of permanent deletion.
    /// Sadly Microsoft.VisualBasic.FileIO is the only easy way to do this in .NET.
    /// </summary>
    public static class FileHandleSafeDelete
    {
        /// <summary>
        ///     Deletes the specified file and moves it to the Recycle Bin.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        /// <returns>True if the file was successfully deleted; otherwise, false.</returns>
        /// <exception cref="FileHandlerException">Thrown when the path is empty or null.</exception>
        public static async Task<bool> DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

            if (!File.Exists(path)) return false;

            int maxTries = FileHandlerRegister.Tries;

            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    // Use SendToRecycleBin but consider UIOption.OnlyErrorDialogs 
                    // to prevent the "Are you sure?" popups from hanging your logic.
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    return true;
                }
                catch (IOException ex) when (i < maxTries - 1)
                {
                    // If it's a lock/sharing violation, wait and try again
                    // Exponential backoff is better than a flat 1-second wait
                    int delay = (i + 1) * 200;
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    // For UnauthorizedAccess or other fatal errors, log and stop
                    FileHandlerRegister.AddError(nameof(DeleteFile), path, ex);
                    return false;
                }
            }

            return false;
        }
    }
}
