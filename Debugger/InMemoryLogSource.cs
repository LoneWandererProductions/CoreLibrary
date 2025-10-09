/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/InMemoryLogSource.cs
 * PURPOSE:     Interface to InMemoryLogger to show logs in the Debugger.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CoreMemoryLog;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    /// Adapter: exposes InMemoryLogger as an ILogSource.
    /// </summary>
    public sealed class InMemoryLogSource : ILogSource
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly InMemoryLogger _logger;

        /// <inheritdoc />
        public event EventHandler<string>? LineReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogSource"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">logger</exception>
        public InMemoryLogSource(InMemoryLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public IEnumerable<string> ReadAll()
        {
            return _logger.GetLogs()
                .OrderBy(l => l.Timestamp)
                .Select(FormatLogEntry);
        }

        /// <inheritdoc />
        public void Start()
        {
            // subscribe directly to the logger’s event
            _logger.LogAdded += OnLogAdded;
        }

        /// <inheritdoc />
        public void Stop()
        {
            _logger.LogAdded -= OnLogAdded;
        }

        /// <summary>
        /// Called when [log added].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="entry">The entry.</param>
        private void OnLogAdded(object? sender, LogEntry entry)
        {
            LineReceived?.Invoke(this, FormatLogEntry(entry));
        }

        /// <summary>
        /// Formats the log entry.
        /// </summary>
        private static string FormatLogEntry(LogEntry entry)
        {
            var msg = entry.Args is { Length: > 0 }
                ? string.Format(entry.Message ?? string.Empty, entry.Args)
                : entry.Message;

            var baseLine = $"[{entry.Timestamp:O}] [{entry.LibraryName}] [{entry.Level}] {entry.CallerMethod}: {msg}";
            if (entry.Exception != null)
                baseLine += $" Exception: {entry.Exception.Message}";
            return baseLine;
        }
    }
}
