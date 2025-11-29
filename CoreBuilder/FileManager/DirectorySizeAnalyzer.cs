/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.FileManager
 * FILE:        DirectorySizeAnalyzer.cs
 * PURPOSE:     Command to analyze and display file sizes and total percentage usage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using CoreBuilder.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.FileManager;

/// <inheritdoc />
/// <summary>
/// Provides functionality to analyze directory size and
/// display file contributions as percentages of total size.
/// </summary>
public sealed class DirectorySizeAnalyzer : ICommand
{
    /// <inheritdoc />
    public string Name => "DirectorySize";

    /// <inheritdoc />
    public string Description => "Analyzes directory size and displays file percentage usage.";

    /// <inheritdoc />
    public string Namespace => "FileManager";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// Generates a textual overview of file sizes in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory to analyze.</param>
    /// <param name="includeSubdirectories">
    /// Whether to include files in subdirectories.
    /// </param>
    /// <returns>
    /// A formatted string containing file size and percentage information.
    /// </returns>
    public string DisplayDirectorySizeOverview(string? directoryPath, bool includeSubdirectories)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            return "Directory does not exist.";

        List<FileInfo> files;

        try
        {
            IEnumerable<string> filePaths;

            if (includeSubdirectories)
            {
                // Use safe recursive enumeration
                filePaths = CoreHelper.SafeEnumerateFiles(directoryPath, "*.*");
            }
            else
            {
                // Standard top-level enumeration
                try
                {
                    filePaths = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    return $"Error accessing directory: {directoryPath}\n{ex.Message}";
                }
            }

            files = filePaths
                .Select(path =>
                {
                    try
                    {
                        return new FileInfo(path);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(f => f is not null)
                .ToList()!;
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
            return "No files found.";

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
    /// Truncates a string to a specified maximum length, appending ellipsis if necessary.
    /// </summary>
    /// <param name="value">The string value to truncate.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <returns>The truncated string, or the original if within limits.</returns>
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength - 3) + "...";
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length < 1)
            return CommandResult.Fail("Usage: DirectorySize([path])");

        // Parse path + optional subfolder flag
        var directoryPath = args[0];
        var includeSubDirs = args.Length > 1 &&
                             args[1].Equals("true", StringComparison.OrdinalIgnoreCase);

        try
        {
            var output = DisplayDirectorySizeOverview(directoryPath, includeSubDirs);
            return CommandResult.Ok(output, EnumTypes.Wstring);
        }
        catch (Exception ex)
        {
            return CommandResult.Fail($"DirectorySize execution failed: {ex.Message}", ex, EnumTypes.Wstring);
        }
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}
