/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Commands
 * FILE:        LoadPluginCommand.cs
 * PURPOSE:     Loads external command plugins and registers them at runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Loader;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Commands
{
    /// <summary>
    /// Command that loads external command plugins from assemblies.
    /// </summary>
    public sealed class LoadPluginCommand : ICommand
    {
        private readonly Weave _weave;
        private readonly PluginLoading _loader = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadPluginCommand"/> class.
        /// </summary>
        /// <param name="weave">The weave engine.</param>
        public LoadPluginCommand(Weave weave)
        {
            _weave = weave ?? throw new ArgumentNullException(nameof(weave));
        }

        /// <inheritdoc />
        public string Name => "load";

        /// <inheritdoc />
        public string Description => "Loads command plugins from a DLL or directory.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var path = args[0];

            if (string.IsNullOrWhiteSpace(path))
                return CommandResult.Fail("Path is empty.");

            try
            {
                var fullPath = Path.GetFullPath(path);

                if (File.Exists(fullPath))
                {
                    return LoadFile(fullPath);
                }

                if (Directory.Exists(fullPath))
                {
                    return LoadDirectory(fullPath);
                }

                return CommandResult.Fail($"Path not found: {fullPath}");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Plugin load failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        /// Result of the command. Contains execution status, a message and the if available the data of the operation.
        /// </returns>
        private CommandResult LoadFile(string file)
        {
            var dir = Path.GetDirectoryName(file)!;

            var commands = _loader.Load<ICommand>(dir)
                                  .Where(c => c.GetType().Assembly.Location == file)
                                  .ToList();

            if (commands.Count == 0)
                return CommandResult.Fail("No ICommand implementations found.");

            foreach (var cmd in commands)
                _weave.Register(cmd);

            return CommandResult.Ok($"Loaded {commands.Count} command plugin(s).");
        }

        /// <summary>
        /// Loads the directory.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>
        /// Result of the command. Contains execution status, a message and the if available the data of the operation.
        /// </returns>
        private CommandResult LoadDirectory(string dir)
        {
            var commands = _loader.Load<ICommand>(dir);

            if (commands.Count == 0)
                return CommandResult.Fail("No ICommand implementations found.");

            foreach (var cmd in commands)
                _weave.Register(cmd);

            return CommandResult.Ok($"Loaded {commands.Count} command plugin(s).");
        }
    }
}
