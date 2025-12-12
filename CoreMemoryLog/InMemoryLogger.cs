/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        InMemoryLogger.cs
 * PURPOSE:     My in-memory logger implementation of an Logger. 
 *              This library can be used as a fallback logger for any library and will also feature other kind of logging.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace CoreMemoryLog
{
    /// <summary>
    /// In-memory logger that supports structured logging similar to Microsoft.Extensions.Logging
    /// and acts as a fallback logger for any library.
    /// </summary>
    public sealed class InMemoryLogger : ILogger, IInMemoryLogger, IEngineLogger, Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// The memory instance
        /// </summary>
        private static readonly Lazy<InMemoryLogger> MemoryInstance = new(() => new InMemoryLogger());

        /// <summary>
        /// The library logs
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentQueue<LogEntry>> _libraryLogs = new();

        /// <summary>
        /// The maximum log entries
        /// </summary>
        private readonly int _maxLogEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogger" /> class.
        /// </summary>
        /// <param name="maxLogEntries">Maximum log entries per library queue.</param>
        public InMemoryLogger(int maxLogEntries = 1000) => _maxLogEntries = maxLogEntries;

        /// <summary>
        /// Singleton instance of the logger.
        /// </summary>
        public static InMemoryLogger Instance => MemoryInstance.Value;

        /// <summary>
        /// Gets or sets the minimum log level for this logger.
        /// Messages below this level will be ignored.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable stack trace].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable stack trace]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableStackTrace { get; set; }

        /// <summary>
        /// Occurs when [log added].
        /// </summary>
        public event EventHandler<LogEntry>? LogAdded;

        /// <summary>
        /// Gets or sets the formatter.
        /// </summary>
        /// <value>
        /// The formatter.
        /// </value>
        public ILogFormatter Formatter { get; set; } = new DefaultLogFormatter();

        /// <summary>
        /// Returns the caller method name (optional helper).
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <returns>Name of the caller</returns>
        private static string GetCaller([CallerMemberName] string caller = "") => caller;

        /// <summary>
        /// Adds a log entry to the library queue, removing oldest entries if the queue exceeds max size.
        /// </summary>
        /// <param name="library">The library.</param>
        /// <param name="entry">The entry.</param>
        private void EnqueueLog(string library, LogEntry entry)
        {
            var queue = _libraryLogs.GetOrAdd(library, _ => new ConcurrentQueue<LogEntry>());
            if (queue.Count >= _maxLogEntries) queue.TryDequeue(out _);
            queue.Enqueue(entry);

            LogAdded?.Invoke(this, entry);
        }

        /// <summary>
        /// Core structured logging method.
        /// Stores the original template and args without formatting.
        /// </summary>
        public void Log(LogLevel level,
            string? message,
            string? libraryName = null,
            Exception? exception = null,
            [CallerMemberName] string callerMethod = "",
            params object[] args)
        {
            // Skip if filtered
            if (!IsEnabled(LogLevel))
                return;

            libraryName ??= GetDefaultLibraryName(); // auto-resolve if not set

            var entry = new LogEntry
            {
                Level = level,
                Message = message, // Store template
                Args = args, // Store structured args
                Timestamp = DateTime.UtcNow,
                Exception = exception,
                CallerMethod = callerMethod,
                LibraryName = libraryName
            };

            // Capture stack trace info for Error and Trace levels if enabled
            if (level is LogLevel.Error or LogLevel.Trace && EnableStackTrace)
            {
                var st = new StackTrace(true);
                var frame = st.GetFrame(2); // skip logger itself
                entry.MethodName = frame?.GetMethod()?.Name;
                entry.LineNumber = frame?.GetFileLineNumber();
                entry.FileName = frame?.GetFileName();
            }

            EnqueueLog(libraryName, entry);

            // Always also output to Trace
            Trace.WriteLine(Formatter.Format(entry));
        }

        /// <summary>
        /// Default ILogger log (uses "ILogger" as library).
        /// </summary>
        public void Log(LogLevel level, string? message, Exception? exception = null, params object[] args)
            => Log(level, message, "ILogger", exception, GetCaller(), args);

        #region Convenience methods

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Debug message</returns>
        public void LogDebug(string? message, params object[] args) => Log(LogLevel.Debug, message, null, args);

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Trace Message</returns>
        public void LogTrace(string? message, params object[] args) => Log(LogLevel.Trace, message, null, args);

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Information message</returns>
        public void LogInformation(string? message, params object[] args) =>
            Log(LogLevel.Information, message, null, args);

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Warning Mssage</returns>
        public void LogWarning(string? message, params object[] args) => Log(LogLevel.Warning, message, null, args);

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Error Message</returns>
        public void LogError(string? message, params object[] args) => Log(LogLevel.Error, message, null, args);

        #endregion

        #region Query logs

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <returns></returns>
        public List<LogEntry> GetLog() => GetLogsByLibrary("ILogger").ToList();

        /// <summary>
        /// Gets logs for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        /// <returns>
        /// Log entries for the specified library.
        /// </returns>
        public IEnumerable<LogEntry> GetLogsByLibrary(string libraryName)
            => _libraryLogs.TryGetValue(libraryName, out var q) ? q.ToList() : Enumerable.Empty<LogEntry>();

        /// <summary>
        /// Gets all logs from the memory.
        /// </summary>
        /// <returns>
        /// All log entries
        /// </returns>
        public IEnumerable<LogEntry> GetLogs()
            => _libraryLogs.Values.SelectMany(q => q).OrderByDescending(l => l.Timestamp);

        /// <summary>
        /// Gets the latest log entries from a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        /// <param name="count">The number of latest logs to retrieve.</param>
        /// <returns>
        /// The latest log entries for the specified library.
        /// </returns>
        public IEnumerable<LogEntry> GetLatestLogs(string libraryName, int count)
            => GetLogsByLibrary(libraryName).TakeLast(count);

        /// <summary>
        /// Determines whether there are logs with the specified log level for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        /// <param name="logLevel">The log level.</param>
        /// <returns>
        ///   <c>true</c> if there are logs with the specified log level for the library; otherwise, <c>false</c>.
        /// </returns>
        public bool HasLogsWithLevel(string libraryName, LogLevel logLevel)
            => _libraryLogs.ContainsKey(libraryName) && _libraryLogs[libraryName].Any(l => l.Level == logLevel);

        /// <summary>
        /// Gets logs by log level for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        /// <param name="logLevel">The log level.</param>
        /// <returns>
        /// Logs matching the specified log level for the specified library.
        /// </returns>
        public IEnumerable<LogEntry> GetLogsByLevel(string libraryName, LogLevel logLevel)
            => _libraryLogs.TryGetValue(libraryName, out var value)
                ? value.Where(l => l.Level == logLevel)
                : Enumerable.Empty<LogEntry>();

        /// <summary>
        /// Clears log entries from a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        public void ClearLogs(string libraryName)
        {
            if (_libraryLogs.TryGetValue(libraryName, out var value))
                value.Clear();
        }

        /// <summary>
        /// Clears all logs for all libraries.
        /// </summary>
        public void ClearAllLogs()
        {
            foreach (var queue in _libraryLogs.Values)
                queue.Clear();
        }

        #endregion

        /// <summary>
        /// Dumps all logs to a file, formatting template with args.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="append">If set to <c>true</c>, appends instead of overwriting.</param>
        /// <param name="minimumLevel">Minimum log level to include (default: Trace).</param>
        public void DumpToFile(string filePath, bool append = false, LogLevel minimumLevel = LogLevel.Trace)
        {
            using var writer = new StreamWriter(filePath, append);

            foreach (var log in GetLogs()
                         .Where(l => l.Level >= minimumLevel)
                         .OrderBy(l => l.Timestamp))
            {
                writer.WriteLine(Formatter.Format(log));
            }
        }

        /// <summary>
        /// Returns the default library name based on the calling type/assembly.
        /// </summary>
        /// <returns>Namespace of the caller.</returns>
        private static string GetDefaultLibraryName()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetCallingAssembly();
                return assembly.GetName().Name ?? "UnknownLibrary";
            }
            catch
            {
                return "UnknownLibrary";
            }
        }

        #region Microsoft.Extensions.Logging compatibility

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An <see cref="T:System.IDisposable" /> that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        /// <summary>
        /// Determines whether the specified log level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>
        ///   <c>true</c> if the specified log level is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= LogLevel;

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">Level to be checked.</param>
        /// <returns>
        ///   <see langword="true" /> if enabled.
        /// </returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        /// <summary>
        /// Logs the specified log level.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string?> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var msg = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty;
            Log(logLevel, msg, exception);
        }

        /// <summary>
        /// Logs the specified log level.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
            => Log((LogLevel)logLevel, formatter(state, exception), exception);

        #endregion
    }
}
