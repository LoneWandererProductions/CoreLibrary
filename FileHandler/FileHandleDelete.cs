/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        FileHandler/FileHandleDelete.cs
* PURPOSE:     Handles all types of file operations, including deletion
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileHandler;

/// <summary>
///     Handles all kinds of file deletions.
/// </summary>
public static class FileHandleDelete
{
    /// <summary>
    ///     Deletes a single file asynchronously with retry for locked files.
    /// </summary>
    /// <param name="path">Target file path.</param>
    /// <returns>True if deletion succeeded, false otherwise.</returns>
    /// <exception cref="FileHandlerException">Thrown when path is null or empty.</exception>
    public static async Task<bool> DeleteFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        if (!File.Exists(path))
            return false;

        int attempt = 0;
        while (IsFileLocked(path) && attempt < FileHandlerRegister.Tries)
        {
            attempt++;
            Trace.WriteLine($"{FileHandlerResources.Tries} {attempt} {path}");
            await Task.Delay(1000);
        }

        if (attempt == FileHandlerRegister.Tries)
        {
            var ex = new Exception($"{FileHandlerResources.ErrorLock} {path}");
            Trace.WriteLine(ex);
            FileHandlerRegister.AddError(nameof(DeleteFile), path, ex);
            return false;
        }

        try
        {
            await Task.Run(() => File.Delete(path)).ConfigureAwait(false);
            FileHandlerRegister.SendStatus?.Invoke(nameof(DeleteFile), path);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            FileHandlerRegister.AddError(nameof(DeleteFile), path, ex);
            Trace.WriteLine(ex);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Deletes multiple files asynchronously.
    /// </summary>
    /// <param name="paths">List of file paths.</param>
    /// <returns>True if all deletions succeeded.</returns>
    /// <exception cref="FileHandlerException">Thrown when paths are null or empty.</exception>
    public static async Task<bool> DeleteFiles(IEnumerable<string> paths)
    {
        if (paths == null || !paths.Any())
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyList);

        var tasks = paths.Select(DeleteFile);
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    /// <summary>
    ///     Deletes all contents of a folder, optionally including subdirectories.
    /// </summary>
    /// <param name="path">Target folder path.</param>
    /// <param name="subdirectories">Include subfolders if true.</param>
    /// <returns>True if deletion succeeded.</returns>
    public static async Task<bool> DeleteAllContents(string path, bool subdirectories = true)
    {
        if (string.IsNullOrEmpty(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        if (!Directory.Exists(path))
            return false;

        try
        {
            var option = subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(path, FileHandlerResources.FileSeparator, option);

            if (!files.Any())
                return false;

            FileHandlerRegister.SendOverview?.Invoke(nameof(DeleteAllContents),
                new FileItems { Elements = files.ToList(), Message = FileHandlerResources.InformationFileDeletion });

            var results = await Task.WhenAll(files.Select(DeleteFile));
            return results.All(r => r);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            FileHandlerRegister.AddError(nameof(DeleteAllContents), path, ex);
            Trace.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    ///     Deletes all files in a folder matching extensions asynchronously.
    /// </summary>
    /// <param name="path">Target folder path.</param>
    /// <param name="fileExtList">List of file extensions to delete.</param>
    /// <param name="subdirectories">Include subfolders if true.</param>
    /// <returns>True if deletion succeeded.</returns>
    public static async Task<bool> DeleteFolderContentsByExtension(string path, List<string> fileExtList, bool subdirectories = true)
    {
        if (string.IsNullOrEmpty(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
        if (!Directory.Exists(path) || fileExtList == null || fileExtList.Count == 0)
            return false;

        fileExtList = FileHandlerProcessing.CleanUpExtensionList(fileExtList);

        var filesToDelete = fileExtList
            .SelectMany(ext => FileHandleSearch.GetFilesByExtensionFullPath(path, ext, subdirectories))
            .ToList();

        if (!filesToDelete.Any())
            return false;

        FileHandlerRegister.SendOverview?.Invoke(nameof(DeleteFolderContentsByExtension),
            new FileItems { Elements = filesToDelete, Message = FileHandlerResources.InformationFileDeletion });

        var results = await Task.WhenAll(filesToDelete.Select(DeleteFile));
        return results.All(r => r);
    }

    /// <summary>
    ///     Deletes a folder and all its contents asynchronously.
    /// </summary>
    /// <param name="path">Target folder path.</param>
    /// <returns>True if deletion succeeded.</returns>
    public static async Task<bool> DeleteCompleteFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        await DeleteAllContents(path);
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            FileHandlerRegister.AddError(nameof(DeleteCompleteFolder), path, ex);
            Trace.WriteLine(ex);
            return false;
        }

        return !Directory.Exists(path);
    }

    /// <summary>
    ///     Checks if a file is locked by attempting exclusive access.
    /// </summary>
    /// <param name="path">Target file path.</param>
    /// <returns>True if file is locked, false otherwise.</returns>
    public static bool IsFileLocked(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        }
        catch (Exception ex) when (ex is ArgumentException or PathTooLongException or IOException
                                       or UnauthorizedAccessException or NotSupportedException)
        {
            Trace.WriteLine($"{FileHandlerResources.ErrorLock} {ex}");
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Deletes a folder but does not check contents (sync).
    /// </summary>
    /// <param name="path">Target folder path.</param>
    /// <returns>True if folder deleted.</returns>
    public static bool DeleteFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        if (!Directory.Exists(path))
            return true;

        try
        {
            Directory.Delete(path, true);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            FileHandlerRegister.AddError(nameof(DeleteFolder), path, ex);
            Trace.WriteLine(ex);
            return false;
        }
    }
}
