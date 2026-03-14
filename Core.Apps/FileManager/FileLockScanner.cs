/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.FileManager
 * FILE:        FileLockScanner.cs
 * PURPOSE:     Command to find locked files in a directory.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using Core.Apps.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Simple command to scan a directory for locked files and list the processes locking them.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class FileLockScanner : ICommand, IRegistryProducer
    {
        /// <inheritdoc />
        public string CurrentRegistryKey => StoreKey;

        /// <inheritdoc />
        public EnumTypes DataType => EnumTypes.Wobject;

        /// <inheritdoc />
        public IVariableRegistry Variables => _variables;

        /// <summary>
        /// The variables
        /// </summary>
        private readonly IVariableRegistry _variables;

        /// <summary>
        /// The store key
        /// </summary>
        private string StoreKey = "lockedfiles";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLockScanner"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public FileLockScanner(IVariableRegistry variables)
        {
            _variables = variables;
        }

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

            // 1. Prepare the Dictionary for the VM Heap
            var lockedData = new Dictionary<string, VmValue>();

            // 2. Use SafeEnumerateFiles to prevent UnauthorizedAccessException crashes!
            IEnumerable<string> files;
            try
            {
                files = CoreHelper.SafeEnumerateFiles(directoryPath, "*.*");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to enumerate directory safely: {ex.Message}");
            }

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
                    var procsString = string.Join(", ", lockingProcs);

                    sb.AppendLine($"{file} locked by: {procsString}");

                    // Add it to our machine-readable payload!
                    // Key = FilePath, Value = String of processes
                    lockedData[file] = VmValue.FromString(procsString);
                }
            }

            if (lockedData.Count == 0)
            {
                sb.AppendLine("No locked files found.");
            }

            // 3. Store the Object in the Registry
            _variables.SetObject(StoreKey, lockedData);

            // 4. Dual-Channel Return
            return CommandResult.Ok(sb.ToString(), StoreKey, EnumTypes.Wobject);
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
                            return false; // Safely ignores Access Denied on elevated processes
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
    }
}