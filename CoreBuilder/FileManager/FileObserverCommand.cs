/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.FileManager
 * FILE:        FileObserverCommand.cs
 * PURPOSE:     Watches a folder and emits file change events as command outputs. Console based for now.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.IO;
using CoreBuilder.Interface;
using CoreBuilder.UI;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.FileManager
{
    /// <inheritdoc />
    /// <summary>
    /// Watches a folder and emits file change events as command outputs.
    /// </summary>
    public sealed class FileObserverCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "FileObserver";

        /// <inheritdoc />
        public string Description => "Monitors a folder and reports file changes.";

        /// <inheritdoc />
        public string Namespace => "FileManager";

        /// <inheritdoc />
        /// <summary>
        /// Gets the parameter count, 1 for folder path.
        /// </summary>
        /// <value>
        /// The parameter count.
        /// </value>
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// The watcher
        /// </summary>
        private FileSystemWatcher? _watcher;

        /// <summary>
        /// The output
        /// </summary>
        private readonly IEventOutput _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileObserverCommand"/> class.
        /// </summary>
        /// <param name="output">The side channel for output.</param>
        public FileObserverCommand(IEventOutput? output = null)
        {
            _output = output ?? new WpfEventOutput();
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var path = args[0];
            if (!Directory.Exists(path))
                return CommandResult.Fail($"Directory does not exist: {path}");

            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += (s, e) => OnEvent("Created", e.FullPath);
            _watcher.Changed += (s, e) => OnEvent("Changed", e.FullPath);
            _watcher.Deleted += (s, e) => OnEvent("Deleted", e.FullPath);
            _watcher.Renamed += (s, e) => OnEvent("Renamed", e.FullPath);
            _watcher.Error += (s, e) => OnEvent("Error", e.GetException()?.Message ?? "Unknown error");

            return new CommandResult
            {
                Message = $"Watching folder '{path}' for changes...",
                RequiresConfirmation = true,
                Feedback = new FeedbackRequest(
                    prompt: "Send 'stop' to end watching.",
                    options: new[] { "stop" },
                    onRespond: input =>
                    {
                        if (input.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase))
                        {
                            StopWatching();
                            return CommandResult.Ok("Watcher stopped.");
                        }

                        return new CommandResult
                        {
                            Message = $"Unknown input '{input}'. Type 'stop' to stop watching.",
                            RequiresConfirmation = true
                        };
                    })
            };
        }

        /// <summary>
        /// Called when [event].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="path">The path.</param>
        private void OnEvent(string type, string path)
        {
            _output.Write($"[{type}] {path}");
        }

        /// <summary>
        /// Stops the watching.
        /// </summary>
        private void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
