/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.FileManager
 * FILE:        FileObserverCommand.cs
 * PURPOSE:     Watches a folder and emits file change events as command outputs. Console based for now.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.IO;
using Core.Apps.Interface;
using Core.Apps.UI;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Core.Apps.FileManager
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

        /// <summary>
        /// The read lock
        /// </summary>
        private readonly object _readLock = new();

        /// <summary>
        /// The last length
        /// </summary>
        private long _lastLength;

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
            // 1. Fix Instant Crash
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
                return CommandResult.Fail("Usage: LogTail([filePath])");

            var filePath = args[0];

            if (!File.Exists(filePath))
                return CommandResult.Fail($"File does not exist: {filePath}");

            // 3. Fix Memory Leak (Dispose old watcher if it exists)
            StopWatching();

            var directory = Path.GetDirectoryName(filePath)!;
            var fileName = Path.GetFileName(filePath);

            _lastLength = new FileInfo(filePath).Length;

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (s, e) => ReadNewContent(filePath);

            _watcher.Created += (s, e) => OnEvent("Created", e.FullPath);
            _watcher.Deleted += (s, e) => OnEvent("Deleted", e.FullPath);
            _watcher.Renamed += (s, e) => OnEvent("Renamed", e.FullPath);
            _watcher.Error += (s, e) => OnEvent("Error", e.GetException()?.Message ?? "Unknown error");

            return new CommandResult
            {
                Message = $"Watching folder '{filePath}' for changes...",
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
        /// Reads the new content.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void ReadNewContent(string filePath)
        {
            // 4. Fix Thread Race Conditions
            lock (_readLock)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length < _lastLength)
                    {
                        // file rotated or truncated
                        _lastLength = 0;
                    }

                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_lastLength, SeekOrigin.Begin);

                    using var reader = new StreamReader(fs);
                    var newText = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(newText))
                        _output.Write(newText.TrimEnd());

                    // 2. Fix "Time Travel" Duplicate Log
                    _lastLength = fs.Position;
                }
                catch (IOException)
                {
                    // Silently ignore IO exceptions caused by the writing process locking the file.
                    // The watcher will fire again a millisecond later when the lock releases.
                }
                catch (Exception ex)
                {
                    _output.Write($"[ERROR] {ex.Message}");
                }
            }
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
