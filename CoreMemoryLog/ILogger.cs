/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        ILogger.cs
 * PURPOSE:     Sample Logger Interface
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog;

/// <summary>
///     ILogger Contract
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Logs the specified level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="callerMethod">The caller method.</param>
    void Log(LogLevel level, string message, Exception exception = null,
        [CallerMemberName] string callerMethod = "");

    /// <summary>
    ///     Logs the information.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="callerMethod">The caller method.</param>
    void LogInformation(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

    /// <summary>
    ///     Logs the warning.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="callerMethod">The caller method.</param>
    void LogWarning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

    /// <summary>
    ///     Logs the error.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="callerMethod">The caller method.</param>
    void LogError(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

    /// <summary>
    ///     Gets the log.
    /// </summary>
    /// <returns>Get Log Entries.</returns>
    List<LogEntry> GetLog();
}
