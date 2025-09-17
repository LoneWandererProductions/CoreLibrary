/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        IInMemoryLogger.cs
 * PURPOSE:     Defines an in-memory logger interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog;

/// <summary>
/// Provides an in-memory logging interface with support for
/// filtering, querying, clearing, and persistence.
/// </summary>
public interface IInMemoryLogger
{
    // ---------------------------------------------------------
    // Configuration
    // ---------------------------------------------------------

    /// <summary>
    /// Gets or sets the minimum log level for this logger.
    /// Messages below this level will be ignored.
    /// </summary>
    LogLevel LogLevel { get; set; }

    // ---------------------------------------------------------
    // Core logging
    // ---------------------------------------------------------

    /// <summary>
    /// Adds a log entry to the memory (this can be directly from logging operations).
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="libraryName">The name of the library (for categorization).</param>
    /// <param name="exception">Optional exception.</param>
    /// <param name="callerMethod">The calling method name (auto-filled via CallerMemberName).</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    void Log(LogLevel logLevel,
             string message,
             string libraryName,
             Exception? exception = null,
             [CallerMemberName] string callerMethod = "",
             params object[] args);

    // ---------------------------------------------------------
    // Retrieval
    // ---------------------------------------------------------

    /// <summary>
    /// Gets all logs from memory.
    /// </summary>
    IEnumerable<LogEntry> GetLogs();

    /// <summary>
    /// Gets logs for a specific library.
    /// </summary>
    IEnumerable<LogEntry> GetLogsByLibrary(string libraryName);

    /// <summary>
    /// Gets the latest log entries from a specific library.
    /// </summary>
    IEnumerable<LogEntry> GetLatestLogs(string libraryName, int count);

    /// <summary>
    /// Gets logs by log level for a specific library.
    /// </summary>
    IEnumerable<LogEntry> GetLogsByLevel(string libraryName, LogLevel logLevel);

    /// <summary>
    /// Determines whether there are logs with the specified log level for a specific library.
    /// </summary>
    bool HasLogsWithLevel(string libraryName, LogLevel logLevel);

    // ---------------------------------------------------------
    // Persistence
    // ---------------------------------------------------------

    /// <summary>
    /// Dumps the in-memory logs to a file.
    /// </summary>
    /// <param name="filePath">The target file path.</param>
    /// <param name="append">If true, append instead of overwrite.</param>
    /// <param name="minimumLevel">Minimum level to include in the dump.</param>
    void DumpToFile(string filePath, bool append = false, LogLevel minimumLevel = LogLevel.Trace);

    // ---------------------------------------------------------
    // Clearing
    // ---------------------------------------------------------

    /// <summary>
    /// Clears log entries from a specific library.
    /// </summary>
    void ClearLogs(string libraryName);

    /// <summary>
    /// Clears all logs for all libraries.
    /// </summary>
    void ClearAllLogs();
}
