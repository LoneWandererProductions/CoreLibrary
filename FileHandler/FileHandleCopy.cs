﻿/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     FileHandler
* FILE:        FileHandler/FileHandleCopy.cs
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FileHandler;

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

        if (!Directory.Exists(source))
        {
            return false;
        }

        var sourceDir = new DirectoryInfo(source);
        var targetDir = new DirectoryInfo(target);
        if (!targetDir.Exists)
        {
            targetDir.Create();
        }

        var files = sourceDir.GetFiles();
        foreach (var file in files)
        {
            try
            {
                file.CopyTo(Path.Combine(target, file.Name), overwrite);
                FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                           or NotSupportedException)
            {
                FileHandlerRegister.AddError(nameof(CopyFiles), file.Name, ex);
                Trace.WriteLine(ex);
                return false;
            }
        }

        foreach (var subDir in sourceDir.GetDirectories())
        {
            CopyFiles(subDir.FullName, Path.Combine(target, subDir.Name), overwrite);
        }

        return true;
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
        {
            throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
        }

        if (!Directory.Exists(target))
        {
            _ = Directory.CreateDirectory(target);
        }

        var check = true;

        //Give the User Optional Infos about the Amount we Copy
        var itm = new FileItems
        {
            Elements = new List<string>(source), Message = FileHandlerResources.InformationFileDeletion
        };

        FileHandlerRegister.SendOverview?.Invoke(nameof(CopyFiles), itm);

        //Do the work
        var root = FileHandlerProcessing.SearchRoot(source);
        var file = new FileInfo(root);
        root = file.Directory.FullName;

        foreach (var element in source)
        {
            try
            {
                file = new FileInfo(element);

                var directoryPath = file.Directory.FullName;

                //Get Sub Folder
                var path = FileHandlerProcessing.GetSubFolder(directoryPath, root, target);

                if (path?.Length == 0)
                {
                    continue;
                }

                var tempPath = Path.Combine(path!, file.Name);

                if (!Directory.Exists(path))
                {
                    _ = Directory.CreateDirectory(path);
                }

                _ = file.CopyTo(tempPath, overwrite);

                FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                           or NotSupportedException)
            {
                check = false;
                FileHandlerRegister.AddError(nameof(CopyFiles), element, ex);
                Trace.WriteLine(ex);
            }
        }

        return check;
    }

    /// <summary>
    ///     Copies all Files to another Location, includes subdirectories, does not Replace the files but returns a List of
    ///     Files that might be overwritten.
    /// </summary>
    /// <param name="source">Full qualified location Path</param>
    /// <param name="target">Full qualified target Path</param>
    /// <returns>List of Files that were not copied, can be null.</returns>
    /// <exception cref="FileHandlerException">No Correct Path was provided</exception>
    [return: MaybeNull]
    public static IList<string> CopyFiles(string source, string target)
    {
        FileHandlerProcessing.ValidatePaths(source, target);

        //if nothing exists we can return anyways
        if (!Directory.Exists(source))
        {
            return null;
        }

        var sourceFiles = FileHandlerProcessing.GetFilesByExtension(source, FileHandlerResources.AllFiles,
            FileHandlerResources.SubFolders);

        if (sourceFiles == null)
        {
            return null;
        }

        var targetFiles = FileHandlerProcessing.GetFilesByExtension(target, FileHandlerResources.AllFiles,
            FileHandlerResources.SubFolders);

        if (targetFiles == null)
        {
            return null;
        }

        //Handle the diff
        var intersect = sourceFiles.Select(i => i).Intersect(targetFiles).ToList();
        var except = sourceFiles.Except(targetFiles).ToList();

        if (intersect.Count == 0)
        {
            return except.Count == 0 ? null : except;
        }

        var check = CopyFiles(intersect, target, false);

        if (!check)
        {
            return null;
        }

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

        //if nothing exists we can return anyways
        if (!Directory.Exists(source))
        {
            return false;
        }

        var check = true;
        var dir = new DirectoryInfo(source);
        var dirs = dir.GetDirectories();
        var files = dir.GetFiles();

        //Give the User Optional Infos about the Amount we Copy
        var lstFiles = (from file in files
            select file.Name).ToList();

        var itm = new FileItems
        {
            Elements = new List<string>(lstFiles), Message = FileHandlerResources.InformationFileDeletion
        };

        FileHandlerRegister.SendOverview?.Invoke(nameof(CopyFiles), itm);

        //do the actual work

        if (files.Length > 0)
        {
            if (!Directory.Exists(target))
            {
                _ = Directory.CreateDirectory(target);
            }

            foreach (var file in files)
            {
                var tempPath = Path.Combine(target, file.Name);

                try
                {
                    if (File.Exists(tempPath) && file.LastWriteTime > File.GetLastWriteTime(tempPath))
                    {
                        _ = file.CopyTo(tempPath, FileHandlerResources.FileOverwrite);
                        FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
                    }
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException
                                               or NotSupportedException)
                {
                    check = false;
                    FileHandlerRegister.AddError(nameof(CopyFiles), file.Name, ex);
                    Trace.WriteLine(ex);
                }
            }
        }

        if (!Directory.Exists(target))
        {
            _ = Directory.CreateDirectory(target);
        }

        foreach (var subDir in dirs)
        {
            var tempPath = Path.Combine(target, subDir.Name);

            if (File.Exists(tempPath))
            {
                continue;
            }

            _ = CopyFiles(subDir.FullName, tempPath, FileHandlerResources.FileOverwrite);
        }

        return check;
    }
}
