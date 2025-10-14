/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        ILogger.cs
 * PURPOSE:     Defines a common logging interface with persistence support.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace CoreMemoryLog;

/// <summary>
/// Defines a flexible logger interface with configurable levels,
/// core log methods, convenience shortcuts, and optional persistence.
/// </summary>
public interface ILogger
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
    /// Logs a message with the specified <paramref name="level"/>.
    /// </summary>
    /// <param name="level">The severity of the log message.</param>
    /// <param name="message">The log message text.</param>
    /// <param name="exception">Optional exception associated with the log entry.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    void Log(LogLevel level, string? message, Exception? exception = null, params object[] args);

    // ---------------------------------------------------------
    // Convenience shortcuts
    // ---------------------------------------------------------

    /// <summary>
    /// Logs the debug.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void LogDebug(string? message, params object[] args);

    /// <summary>
    /// Logs the trace.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void LogTrace(string? message, params object[] args);

    /// <summary>
    /// Logs the information.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void LogInformation(string? message, params object[] args);

    /// <summary>
    /// Logs the warning.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void LogWarning(string? message, params object[] args);

    /// <summary>
    /// Logs the error.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void LogError(string? message, params object[] args);

    // ---------------------------------------------------------
    // Optional Microsoft.Extensions.Logging bridge
    // ---------------------------------------------------------

    /// <summary>
    /// Bridges to Microsoft.Extensions.Logging infrastructure.
    /// </summary>
    void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter);

    // ---------------------------------------------------------
    // Persistence
    // ---------------------------------------------------------

    /// <summary>
    /// Writes the current log entries to a file.
    /// </summary>
    /// <param name="filePath">Target file path.</param>
    /// <param name="append">If true, append instead of overwrite.</param>
    /// <param name="minimumLevel">Minimum level to include in the dump.</param>
    void DumpToFile(string filePath, bool append = false, LogLevel minimumLevel = LogLevel.Trace);

    /// <summary>
    /// Gets the in-memory collection of log entries.
    /// </summary>
    /// <returns>A list of all Log entries.</returns>
    List<LogEntry> GetLog();
}
