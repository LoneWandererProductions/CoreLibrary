/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        LogLevel.cs
 * PURPOSE:     Debug Levels and Log levels.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CoreMemoryLog
{
    /// <summary>
    ///     Entries of our Log
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        ///     The trace level, used for very fine-grained debugging messages that are typically only relevant during development and debugging sessions, such as rendering loops, memory allocators, or ECS pipelines.
        /// </summary>
        Trace = 0,

        /// <summary>
        ///     The debug level, used for detailed debugging messages that are typically only relevant during development and debugging sessions.
        /// </summary>
        Debug = 1,

        /// <summary>
        ///     The information level, used for general informational messages that highlight the progress of the application at a coarse-grained level.
        /// </summary>
        Information = 2,

        /// <summary>
        ///     The warning level, used for potentially harmful situations that may require attention but do not necessarily indicate application failure.
        /// </summary>
        Warning = 3,

        /// <summary>
        ///     The error level, used for recoverable errors that may require attention but do not necessarily indicate application failure.
        /// </summary>
        Error = 4,

        /// <summary>
        ///     The critical level, used for severe errors that may cause application failure.
        /// </summary>
        Critical = 5,

        /// <summary>
        /// The none level, used to disable logging.
        /// </summary>
        None = 6
    }
}
