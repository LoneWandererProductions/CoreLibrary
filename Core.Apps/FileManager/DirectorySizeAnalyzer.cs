/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.FileManager
 * FILE:        DirectorySizeAnalyzer.cs
 * PURPOSE:     Command to analyze and display file sizes and total percentage usage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using Core.Apps.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Registry;

namespace Core.Apps.FileManager
{
    /// <inheritdoc cref="ICommand" />
    /// <summary>
    /// Provides functionality to analyze directory size and
    /// display file contributions as percentages of total size.
    /// </summary>
    public sealed class DirectorySizeAnalyzer : ICommand, IRegistryProducer
    {
        /// <inheritdoc />
        public string CurrentRegistryKey => StoreKey;

        /// <inheritdoc />
        public EnumTypes DataType =>
            EnumTypes.Wobject; // We will return an object containing the total size and a list of files

        /// <inheritdoc />
        public IVariableRegistry Variables => _variables;

        /// <summary>
        /// The variables
        /// </summary>
        private readonly IVariableRegistry _variables;

        /// <summary>
        /// The store key
        /// </summary>
        private string StoreKey = "directorysize";

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
        /// Initializes a new instance of the <see cref="DirectorySizeAnalyzer"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public DirectorySizeAnalyzer(IVariableRegistry variables)
        {
            _variables = variables;
        }

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
        public (string TextOutput, Dictionary<string, VmValue>? RawData) AnalyzeDirectory(string? directoryPath,
            bool includeSubdirectories)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return ("Directory does not exist.", null);

            List<FileInfo> files;

            try
            {
                // ... (Your existing file enumeration logic stays exactly the same) ...
                var filePaths = includeSubdirectories
                    ? CoreHelper.SafeEnumerateFiles(directoryPath, "*.*")
                    : Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

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
            catch (Exception ex)
            {
                return ($"Error accessing directory: {directoryPath}\n{ex.Message}", null);
            }

            if (files.Count == 0)
                return ("No files found.", null);

            var totalSize = files.Sum(f => f.Length);

            // 1. Build the Console String (Human channel)
            var sb = new StringBuilder();
            sb.AppendLine($"Listing files in: {directoryPath}");
            sb.AppendLine();
            sb.AppendLine($"{"Name",-50} {"Size (KB)",12} {"% of Total",10}");
            sb.AppendLine(new string('-', 75));

            // 2. Build the Raw Data Object (Machine channel)
            var fileList = new List<VmValue>();

            foreach (var file in files)
            {
                var sizeInKb = file.Length / 1024.0;
                var percent = (double)file.Length / totalSize * 100;

                sb.AppendLine($"{Truncate(file.Name, 50),-50} {sizeInKb,10:N0} KB {percent,8:F1}%");

                // Create a Wobject for each file so scripts can read them
                var fileObj = new Dictionary<string, VmValue>
                {
                    { "name", VmValue.FromString(file.Name) },
                    { "size_bytes", VmValue.FromInt(file.Length) },
                    { "percent", VmValue.FromDouble(percent) }
                };

                // Add it to our list, assigning it the Wobject type
                fileList.Add(new VmValue(EnumTypes.Wobject, 0, 0, false, null,
                    null)); // You might need a helper method in VmValue for this depending on how you handle nested objects
            }

            sb.AppendLine();
            sb.AppendLine($"Total size: {totalSize / 1024.0:N0} KB");

            // Combine the totals into the main parent object
            var rawData = new Dictionary<string, VmValue>
            {
                { "total_size_bytes", VmValue.FromInt(totalSize) }, { "file_count", VmValue.FromInt(files.Count) }
            };

            return (sb.ToString(), rawData);
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
                return CommandResult.Fail("Usage: DirectorySize([path] [, includeSubDirs])");

            var directoryPath = args[0];
            var includeSubDirs = args.Length > 1 && args[1].Equals("true", StringComparison.OrdinalIgnoreCase);

            try
            {
                // Run the analysis
                var result = AnalyzeDirectory(directoryPath, includeSubDirs);

                if (result.RawData != null)
                {
                    // Store the raw data in the VM Heap!
                    _variables.SetObject(StoreKey, result.RawData);

                    // Return the string to the console, but pass the StoreKey as the Value payload
                    return CommandResult.Ok(result.TextOutput, StoreKey, EnumTypes.Wobject);
                }

                // If it failed (directory didn't exist), just return the error string
                return CommandResult.Fail(result.TextOutput, EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"DirectorySize execution failed: {ex.Message}", EnumTypes.Wstring);
            }
        }
    }
}
