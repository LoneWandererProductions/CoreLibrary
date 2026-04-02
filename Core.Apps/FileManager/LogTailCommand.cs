/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.FileManager
 * FILE:        LogTailCommand.cs
 * PURPOSE:     A simple log tail command that watches a file for new lines and prints them to the output.
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
    /// A simple log tail command that watches a file for new lines and prints them to the output.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class LogTailCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "LogTail";

        /// <inheritdoc />
        public string Description => "Watches a file and prints newly appended lines.";

        /// <inheritdoc />
        public string Namespace => "FileManager";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// The output
        /// </summary>
        private readonly IEventOutput _output;

        /// <summary>
        /// The watcher
        /// </summary>
        private FileSystemWatcher? _watcher;

        /// <summary>
        /// The last length
        /// </summary>
        private long _lastLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTailCommand"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public LogTailCommand(IEventOutput? output = null)
        {
            _output = output ?? new WpfEventOutput();
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args == null || args.Length == 0)
                return CommandResult.Fail("No file path provided.");

            var filePath = args[0];

            if (!File.Exists(filePath))
                return CommandResult.Fail($"File does not exist: {filePath}");

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

            return new CommandResult
            {
                Message = $"Watching file '{filePath}' for new content...",
                RequiresConfirmation = true,
                Feedback = new FeedbackRequest(
                    prompt: "Send 'stop' to end watching.",
                    options: new[] { "stop" },
                    onRespond: input =>
                    {
                        if (input.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase))
                        {
                            StopWatching();
                            return CommandResult.Ok("Log watching stopped.");
                        }

                        return new CommandResult
                        {
                            Message = $"Unknown input '{input}'. Type 'stop'.", RequiresConfirmation = true
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
            // Lock ensures multiple Rapid FileSystemWatcher events don't step on each other
            lock (_syncRoot)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length < _lastLength)
                    {
                        // File rotated or truncated
                        _lastLength = 0;
                    }

                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_lastLength, SeekOrigin.Begin);

                    using var reader = new StreamReader(fs);
                    var newText = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(newText))
                        _output.Write(newText.TrimEnd());

                    // Accurately capture where we stopped reading
                    _lastLength = fs.Position;
                }
                catch (IOException)
                {
                    // Often happens if another process has an exclusive lock momentarily.
                    // Safe to ignore; the next Changed event will pick it up.
                }
                catch (Exception ex)
                {
                    _output.Write($"[ERROR] {ex.Message}");
                }
            }
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
