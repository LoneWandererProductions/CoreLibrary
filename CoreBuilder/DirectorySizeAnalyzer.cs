/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/DirectorySizeAnalyzer.cs
 * PURPOSE:     Tool to analyze and display file sizes and total percentage usage
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text;
using CoreBuilder.Interface;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Default implementation of IDirectorySizeAnalyzer.
/// </summary>
public sealed class DirectorySizeAnalyzer : IDirectorySizeAnalyzer
{
    /// <inheritdoc />
    /// <summary>
    ///     Lists file sizes and their percentage of total size in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory to analyze.</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories.</param>
    /// <returns>
    ///     A formatted string showing size and percentage information.
    /// </returns>
    public string DisplayDirectorySizeOverview(string? directoryPath, bool includeSubdirectories)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return "Directory does not exist.";
        }

        List<FileInfo> files;

        try
        {
            files = Directory
                .EnumerateFiles(directoryPath, "*.*",
                    includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(f => new FileInfo(f))
                .ToList();
        }
        catch (UnauthorizedAccessException ex)
        {
            return $"Access denied to one or more folders in: {directoryPath}\n{ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error accessing directory: {directoryPath}\n{ex.Message}";
        }


        if (files.Count == 0)
        {
            return "No files found.";
        }

        var totalSize = files.Sum(f => f.Length);

        var sb = new StringBuilder();
        sb.AppendLine($"Listing files in: {directoryPath}");
        sb.AppendLine();
        sb.AppendLine($"{"Name",-50} {"Size (KB)",12} {"% of Total",10}");
        sb.AppendLine(new string('-', 75));

        foreach (var file in files)
        {
            var sizeInKb = file.Length / 1024.0;
            var percent = (double)file.Length / totalSize * 100;
            sb.AppendLine($"{Truncate(file.Name, 50),-50} {sizeInKb,10:N0} KB {percent,8:F1}%");
        }

        sb.AppendLine();
        sb.AppendLine($"Total size: {totalSize / 1024.0:N0} KB");

        return sb.ToString();
    }

    /// <summary>
    ///     Truncates the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>Truncated string.</returns>
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }
}
