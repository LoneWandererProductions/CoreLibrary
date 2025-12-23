/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        ILogEntry.cs
 * PURPOSE:     Interface defining a log entry
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace CoreMemoryLog
{
    /// <summary>
    /// Define for a log entry
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        LogLevel Level { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        string? Message { get; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        Exception? Exception { get; }

        /// <summary>
        /// Gets the caller method.
        /// </summary>
        /// <value>
        /// The caller method.
        /// </value>
        string? CallerMethod { get; }

        /// <summary>
        /// Gets the name of the library.
        /// </summary>
        /// <value>
        /// The name of the library.
        /// </value>
        string? LibraryName { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        object[]? Args { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        string? FileName { get; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        int? LineNumber { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        string? MethodName { get; }
    }
}
