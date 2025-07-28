/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        CoreMemoryLog/LogEntry.cs
 * PURPOSE:     All possible log information
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace CoreMemoryLog;

/// <summary>
///     Logger Object, holds all needed data
/// </summary>
public sealed class LogEntry
{
    /// <summary>
    ///     Gets or sets the level.
    /// </summary>
    /// <value>
    ///     The level.
    /// </value>
    public LogLevel Level { get; set; }

    /// <summary>
    ///     Gets or sets the message.
    /// </summary>
    /// <value>
    ///     The message.
    /// </value>
    public string Message { get; init; }

    /// <summary>
    ///     Gets or sets the timestamp.
    /// </summary>
    /// <value>
    ///     The timestamp.
    /// </value>
    public DateTime Timestamp { get; init; }

    /// <summary>
    ///     Gets or sets the exception.
    /// </summary>
    /// <value>
    ///     The exception.
    /// </value>
    public Exception Exception { get; init; }

    /// <summary>
    ///     Gets or sets the caller method.
    /// </summary>
    /// <value>
    ///     The caller method.
    /// </value>
    public string CallerMethod { get; init; } // Stores the calling method name

    /// <summary>
    ///     Gets or sets the name of the library that called the Log.
    /// </summary>
    /// <value>
    ///     The name of the library.
    /// </value>
    public string LibraryName { get; init; }
}
