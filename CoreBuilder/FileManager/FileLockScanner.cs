/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.FileManager
 * FILE:        FileLockScanner.cs
 * PURPOSE:     Command to find locked files in a directory.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.FileManager;

/// <inheritdoc />
/// <summary>
/// Simple command to scan a directory for locked files and list the processes locking them.
/// </summary>
/// <seealso cref="ICommand" />
public sealed class FileLockScanner : ICommand
{
    /// <inheritdoc />
    public string Name => "FileLockScanner";

    /// <inheritdoc />
    public string Description => "Lists locked files in a directory and the processes locking them.";

    /// <inheritdoc />
    public string Namespace => "FileManager";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length < 1)
            return CommandResult.Fail("Usage: FileLockScanner([path])");

        var directoryPath = args[0];
        if (!Directory.Exists(directoryPath))
            return CommandResult.Fail("Directory does not exist.");

        var sb = new StringBuilder();
        sb.AppendLine($"Scanning for locked files in: {directoryPath}");
        sb.AppendLine("-----------------------------------");

        var files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // File is not locked
                }
            }
            catch
            {
                // File is locked; find locking processes
                var lockingProcs = GetLockingProcesses(file);
                sb.AppendLine($"{file} locked by: {string.Join(", ", lockingProcs)}");
            }
        }

        return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
    }

    /// <summary>
    /// Gets the locking processes.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>Name of locking processes.</returns>
    private static string[] GetLockingProcesses(string file)
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        return p.Modules.Cast<ProcessModule>()
                            .Any(m => string.Equals(m.FileName, file, StringComparison.OrdinalIgnoreCase));
                    }
                    catch
                    {
                        return false;
                    }
                })
                .Select(p => p.ProcessName)
                .Distinct()
                .ToArray();
            return processes.Length > 0 ? processes : new[] { "Unknown" };
        }
        catch
        {
            return new[] { "Unknown" };
        }
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
        => CommandResult.Fail($"'{Name}' has no extensions.");
}
