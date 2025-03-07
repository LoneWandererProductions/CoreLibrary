/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        CoreMemoryLog/InMemoryLogger.cs
 * PURPOSE:     Implementation of Logger Interface with dual interface for fallback and in-memory logging
 *              This class implements both ILogger (for dependency injection compatibility) and 
 *              IInMemoryLogger (for specialized in-memory logging with library-specific logs).
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog
{
    /// <summary>
    /// Internal Memory Logger that implements both ILogger and IInMemoryLogger
    /// <para>
    /// Implements ILogger to be compatible with dependency injection systems and serve as a fallback 
    /// logger when no other logger is provided. Implements IInMemoryLogger for managing logs in memory 
    /// with support for library-specific logs and additional functionality like clearing logs or querying 
    /// logs by level.
    /// </para>
    /// </summary>
    public class InMemoryLogger : ILogger, IInMemoryLogger
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<LogEntry>> _libraryLogs = new();
        private readonly ConcurrentQueue<LogEntry> _ILoggerLogs = new();
        private readonly int _maxLogEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogger"/> class.
        /// </summary>
        /// <param name="maxLogEntries">The maximum number of log entries to store.</param>
        public InMemoryLogger(int maxLogEntries = 1000)
        {
            _maxLogEntries = maxLogEntries;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogger"/> class with a default maximum of 1000 log entries.
        /// </summary>
        public InMemoryLogger()
        {
            _maxLogEntries = 1000;
        }

        /// <summary>
        /// Logs the given message with the information level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <param name="callerMethod">The method from which the log is called, automatically populated by the compiler.</param>
        public void LogInformation(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            LogToQueue(_ILoggerLogs, LogLevel.Information, message, exception, callerMethod);
        }

        /// <summary>
        /// Logs the given message with the warning level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <param name="callerMethod">The method from which the log is called, automatically populated by the compiler.</param>
        public void LogWarning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            LogToQueue(_ILoggerLogs, LogLevel.Warning, message, exception, callerMethod);
        }

        /// <summary>
        /// Logs the given message with the error level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <param name="callerMethod">The method from which the log is called, automatically populated by the compiler.</param>
        public void LogError(string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            LogToQueue(_ILoggerLogs, LogLevel.Error, message, exception, callerMethod);
        }

        /// <summary>
        /// Generic log method for ILogger to log messages with any log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <param name="callerMethod">The method from which the log is called, automatically populated by the compiler.</param>
        public void Log(LogLevel level, string message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            Log(level, message, "ILogger", exception, callerMethod);
        }

        /// <summary>
        /// Logs a message with a specified log level and library name.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="libraryName">The name of the library or component logging the message.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <param name="callerMethod">The method from which the log is called, automatically populated by the compiler.</param>
        public void Log(LogLevel level, string message, string libraryName, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            // Ensure that the dictionary has a queue for the specified library
            var libraryQueue = _libraryLogs.GetOrAdd(libraryName, new ConcurrentQueue<LogEntry>());

            lock (libraryQueue)
            {
                if (libraryQueue.Count >= _maxLogEntries)
                {
                    libraryQueue.TryDequeue(out _); // Discard the oldest log if max is reached
                }

                libraryQueue.Enqueue(new LogEntry
                {
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Exception = exception,
                    CallerMethod = callerMethod,
                    LibraryName = libraryName
                });
            }
        }

        /// <summary>
        /// Gets all logs for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library for which to retrieve logs.</param>
        /// <returns>All logs for the specified library.</returns>
        public IEnumerable<LogEntry> GetLogsByLibrary(string libraryName)
        {
            if (_libraryLogs.TryGetValue(libraryName, out var libraryQueue))
            {
                return libraryQueue.ToList();
            }
            return Enumerable.Empty<LogEntry>();
        }

        /// <summary>
        /// Gets all logs from all libraries.
        /// </summary>
        /// <returns>All logs from all libraries.</returns>
        public IEnumerable<LogEntry> GetLogs()
        {
            return _libraryLogs.Values
                .SelectMany(queue => queue.ToList())
                .OrderByDescending(log => log.Timestamp);
        }

        /// <summary>
        /// Gets the latest logs from the ILogger log queue.
        /// </summary>
        /// <returns>All logs from the ILogger log queue.</returns>
        public List<LogEntry> GetLog()
        {
            return _ILoggerLogs.ToList();
        }

        /// <summary>
        /// Clears the logs for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library for which to clear logs.</param>
        public void ClearLogs(string libraryName)
        {
            if (_libraryLogs.ContainsKey(libraryName))
            {
                _libraryLogs[libraryName].Clear();
            }
        }

        /// <summary>
        /// Clears all logs.
        /// </summary>
        public void ClearAllLogs()
        {
            foreach (var libraryQueue in _libraryLogs.Values)
            {
                libraryQueue.Clear();
            }
        }

        /// <summary>
        /// Gets the latest logs for a specific library.
        /// </summary>
        /// <param name="libraryName">The name of the library for which to retrieve logs.</param>
        /// <param name="count">The number of latest logs to retrieve.</param>
        /// <returns>The latest logs for the specified library.</returns>
        public IEnumerable<LogEntry> GetLatestLogs(string libraryName, int count)
        {
            return GetLogsByLibrary(libraryName)
                .Reverse() // Reverse to get the latest logs first
                .Take(count)
                .Reverse(); // Reverse back to original order
        }

        /// <summary>
        /// Checks if logs of a specific level exist for a given library.
        /// </summary>
        /// <param name="libraryName">The name of the library to check.</param>
        /// <param name="logLevel">The log level to search for.</param>
        /// <returns>True if logs of the specified level exist, otherwise false.</returns>
        public bool HasLogsWithLevel(string libraryName, LogLevel logLevel)
        {
            return _libraryLogs.ContainsKey(libraryName) &&
                   _libraryLogs[libraryName].Any(log => log.Level == logLevel);
        }

        /// <summary>
        /// Gets logs of a specific level for a given library.
        /// </summary>
        /// <param name="libraryName">The name of the library for which to retrieve logs.</param>
        /// <param name="logLevel">The log level to filter by.</param>
        /// <returns>Logs of the specified level for the given library.</returns>
        public IEnumerable<LogEntry> GetLogsByLevel(string libraryName, LogLevel logLevel)
        {
            return _libraryLogs.ContainsKey(libraryName)
                ? _libraryLogs[libraryName].Where(log => log.Level == logLevel)
                : Enumerable.Empty<LogEntry>();
        }

        /// <summary>
        /// A helper method that handles log entry insertion into the queue with log level, message, and exception.
        /// </summary>
        /// <param name="queue">The queue to insert the log entry into.</param>
        /// <param name="level">The log level of the entry.</param>
        /// <param name="message">The message of the log entry.</param>
        /// <param name="exception">An optional exception associated with the log entry.</param>
        /// <param name="callerMethod">The method name from which the log was called.</param>
        private void LogToQueue(ConcurrentQueue<LogEntry> queue, LogLevel level, string message, Exception exception, string callerMethod)
        {
            if (queue.Count >= _maxLogEntries)
            {
                queue.TryDequeue(out _); // Discard the oldest log if max is reached
            }

            queue.Enqueue(new LogEntry
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Exception = exception,
                CallerMethod = callerMethod
            });
        }
    }
}
