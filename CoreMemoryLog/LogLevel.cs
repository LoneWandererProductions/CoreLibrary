/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        CoreMemoryLog/LogLevel.cs
 * PURPOSE:     Debug Levels and Log levels.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CoreMemoryLog;

/// <summary>
///     Entries of our Log
/// </summary>
public enum LogLevel
{
    /// <summary>
    ///     The trace
    /// </summary>
    Trace = 0,

    /// <summary>
    ///     The debug
    /// </summary>
    Debug = 1,

    /// <summary>
    ///     The information
    /// </summary>
    Information = 2,

    /// <summary>
    ///     The warning
    /// </summary>
    Warning = 3,

    /// <summary>
    ///     The error
    /// </summary>
    Error = 4,

    /// <summary>
    ///     The critical
    /// </summary>
    Critical = 5
}
