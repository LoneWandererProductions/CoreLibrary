/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandler/FileHandleCreate.cs
 * PURPOSE:     Handles all types of file creation operations
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;

namespace FileHandler;

/// <summary>
///     Handles file and folder creation.
/// </summary>
public static class FileHandleCreate
{
    /// <summary>
    ///     Creates a folder at the specified path.
    /// </summary>
    /// <param name="path">Target folder path.</param>
    /// <exception cref="FileHandlerException">Thrown if the path is null or empty.</exception>
    public static void CreateFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        _ = CreateDirectory(path);
    }

    /// <summary>
    ///     Creates a folder with a specific name inside a parent path.
    /// </summary>
    /// <param name="path">Parent path.</param>
    /// <param name="name">Folder name.</param>
    /// <returns>True if the folder was created or already exists; false if creation failed.</returns>
    /// <exception cref="FileHandlerException">Thrown if the path or name is null or empty.</exception>
    public static bool CreateFolder(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(name))
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);

        var fullPath = Path.Combine(path, name);

        return CreateDirectory(fullPath);
    }

    /// <summary>
    ///     Creates the directory safely.
    /// </summary>
    /// <param name="path">Full folder path.</param>
    /// <returns>True if the folder was created or already exists; false if creation failed.</returns>
    private static bool CreateDirectory(string path)
    {
        try
        {
            // Directory.CreateDirectory is safe: it does nothing if the directory already exists.
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException
                                   or IOException
                                   or PathTooLongException
                                   or NotSupportedException)
        {
            FileHandlerRegister.AddError(nameof(CreateFolder), path, ex);
            Trace.WriteLine(ex);
            return false;
        }
    }
}
