/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        CoreMemoryLog/IInMemoryLogger.cs
 * PURPOSE:     Logger Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog;

public interface IInMemoryLogger
{
    /// <summary>
    ///     Adds a log entry to the memory (this can be directly from logging operations).
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="libraryName">The name of the library (to categorize the log).</param>
    /// <param name="exception">The exception (optional).</param>
    /// <param name="callerMethod">The method calling the logger (optional, uses CallerMemberName).</param>
    void Log(LogLevel logLevel, string message, string libraryName, Exception exception = null,
        [CallerMemberName] string callerMethod = "");

    /// <summary>
    ///     Gets all logs from the memory.
    /// </summary>
    /// <returns>All log entries</returns>
    IEnumerable<LogEntry> GetLogs();

    /// <summary>
    ///     Gets logs for a specific library.
    /// </summary>
    /// <param name="libraryName">The name of the library.</param>
    /// <returns>Log entries for the specified library.</returns>
    IEnumerable<LogEntry> GetLogsByLibrary(string libraryName);

    /// <summary>
    ///     Clears log entries from a specific library.
    /// </summary>
    /// <param name="libraryName">The name of the library.</param>
    void ClearLogs(string libraryName);

    /// <summary>
    ///     Gets the latest log entries from a specific library.
    /// </summary>
    /// <param name="libraryName">The name of the library.</param>
    /// <param name="count">The number of latest logs to retrieve.</param>
    /// <returns>The latest log entries for the specified library.</returns>
    IEnumerable<LogEntry> GetLatestLogs(string libraryName, int count);

    /// <summary>
    ///     Determines whether there are logs with the specified log level for a specific library.
    /// </summary>
    /// <param name="libraryName">The name of the library.</param>
    /// <param name="logLevel">The log level.</param>
    /// <returns><c>true</c> if there are logs with the specified log level for the library; otherwise, <c>false</c>.</returns>
    bool HasLogsWithLevel(string libraryName, LogLevel logLevel);

    /// <summary>
    ///     Gets logs by log level for a specific library.
    /// </summary>
    /// <param name="libraryName">The name of the library.</param>
    /// <param name="logLevel">The log level.</param>
    /// <returns>Logs matching the specified log level for the specified library.</returns>
    IEnumerable<LogEntry> GetLogsByLevel(string libraryName, LogLevel logLevel);

    /// <summary>
    ///     Clears all logs for all libraries.
    /// </summary>
    void ClearAllLogs();
}
