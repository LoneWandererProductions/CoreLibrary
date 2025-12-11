/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        IEngineLogger.cs
 * PURPOSE:     Defines a lightweight, engine-facing logging interface.
 *              Used by DebugLogger and other components to allow flexible
 *              backend implementations without imposing dependencies.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreMemoryLog
{
    /// <summary>
    /// Defines a minimal, engine-facing logging interface used by debug-only
    /// wrappers such as <c>DebugLogger</c>.
    /// <para>
    /// Implementations may route messages to any logging backend, such as
    /// in-memory buffers, consoles, files, or external monitoring systems.
    /// The interface is intentionally lightweight to minimize overhead when
    /// used together with conditional logging in debug builds.
    /// </para>
    /// </summary>
    public interface IEngineLogger
    {
        /// <summary>
        /// Writes a debug-level log message.
        /// </summary>
        /// <param name="message">The format string representing the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Writes an information-level log message.
        /// </summary>
        /// <param name="message">The format string representing the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// Writes a warning-level log message.
        /// </summary>
        /// <param name="message">The format string representing the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Writes an error-level log message.
        /// </summary>
        /// <param name="message">The format string representing the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Writes a trace-level log message.
        /// Trace logs are typically used for very fine-grained debugging
        /// such as rendering loops, memory allocators, or ECS pipelines.
        /// </summary>
        /// <param name="message">The format string representing the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        void LogTrace(string message, params object[] args);
    }
}
