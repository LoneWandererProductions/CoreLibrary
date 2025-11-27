/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        FileHandler/FileHandleCompress.cs
* PURPOSE:     File Compression Utilities
* PROGRAMER:   Peter Geinitz (Wayfarer)
* Sources:     https://docs.microsoft.com/de-de/dotnet/api/system.io.compression.zipfile?view=net-5.0
*              https://docs.microsoft.com/de-de/dotnet/api/system.io.compression.ziparchive.entries?view=net-5.0
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FileHandler;

/// <summary>
///     Included Compression Library
/// </summary>
public static class FileHandleCompress
{
    /// <summary>
    ///     Saves the specified files into a zip archive.
    /// </summary>
    /// <param name="zipPath">The path of the zip file to create or update.</param>
    /// <param name="fileToAdd">The file(s) to add to the zip.</param>
    /// <param name="delete">If <c>true</c>, deletes the source files after adding. Default is true.</param>
    /// <param name="compressionLevel">Optional compression level (default Optimal).</param>
    /// <returns>Operation success as <c>true</c> or <c>false</c>.</returns>
    public static async Task<bool> SaveZip(
        string zipPath,
        List<string> fileToAdd,
        bool delete = true,
        CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath) ?? throw new InvalidOperationException("Invalid zip path"));

            // Wrap in Task.Run for non-blocking large file operations
            await Task.Run(() =>
            {
                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

                foreach (var file in fileToAdd)
                {
                    if (!File.Exists(file))
                        continue;

                    var fileInfo = new FileInfo(file);
                    _ = archive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name, compressionLevel);

                    FileHandlerRegister.SendStatus?.Invoke(nameof(SaveZip), file);
                }
            });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                       or NotSupportedException)
        {
            FileHandlerRegister.AddError(nameof(SaveZip), zipPath, ex);
            Trace.WriteLine(ex);
            return false;
        }

        if (!delete)
            return true;

        var deleteTasks = fileToAdd.Select(file => FileHandleDelete.DeleteFile(file));
        var results = await Task.WhenAll(deleteTasks);

        return results.All(r => r);
    }

    /// <summary>
    ///     Extracts the contents of a zip archive to a specified folder.
    /// </summary>
    /// <param name="zipPath">The zip file path to extract.</param>
    /// <param name="extractPath">The folder to extract files into.</param>
    /// <param name="delete">If <c>true</c>, deletes the zip file after extraction. Default is true.</param>
    /// <returns>Operation success as <c>true</c> or <c>false</c>.</returns>
    /// <exception cref="FileHandlerException">Thrown if the zip file does not exist.</exception>
    public static async Task<bool> OpenZip(string zipPath, string extractPath, bool delete = true)
    {
        if (!File.Exists(zipPath))
            throw new FileHandlerException(string.Concat(FileHandlerResources.ErrorFileNotFound, zipPath));

        try
        {
            Directory.CreateDirectory(extractPath);

            await Task.Run(() =>
            {
                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
                archive.ExtractToDirectory(extractPath);
            });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                       or NotSupportedException)
        {
            FileHandlerRegister.AddError(nameof(OpenZip), zipPath, ex);
            Trace.WriteLine(ex);
            return false;
        }

        return !delete || await FileHandleDelete.DeleteFile(zipPath);
    }

    /// <summary>
    /// Saves the zip transactional.
    /// </summary>
    /// <param name="zipPath">The zip path.</param>
    /// <param name="filesToAdd">The files to add.</param>
    /// <param name="delete">if set to <c>true</c> [delete].</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>Operation success as <c>true</c> or <c>false</c>.</returns>
    public static async Task<bool> SaveZipTransactional(
    string zipPath,
    List<string> filesToAdd,
    bool delete = true,
    CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var addedEntries = new List<ZipArchiveEntry>();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath) ?? throw new InvalidOperationException("Invalid zip path"));

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

            foreach (var file in filesToAdd)
            {
                if (!File.Exists(file)) continue;

                var fileInfo = new FileInfo(file);
                var entry = archive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name, compressionLevel);
                addedEntries.Add(entry);

                FileHandlerRegister.SendStatus?.Invoke(nameof(SaveZipTransactional), file);
            }
        }
        catch (Exception ex)
        {
            // Rollback added entries
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
            foreach (var entry in addedEntries)
            {
                entry.Delete();
            }

            FileHandlerRegister.AddError(nameof(SaveZipTransactional), zipPath, ex);
            Trace.WriteLine(ex);
            return false;
        }

        if (!delete) return true;

        var deleteTasks = filesToAdd.Select(file => FileHandleDelete.DeleteFile(file));
        var results = await Task.WhenAll(deleteTasks);

        return results.All(r => r);
    }

}
