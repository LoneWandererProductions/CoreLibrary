/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        DebugLogger.cs
 * PURPOSE:     A lightweight wrapper for debug-only logging.
 *              Provides zero-cost removal in Release builds.
 *              Uses IEngineLogger as its backend, allowing any logging
 *              implementation to be plugged in.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog
{

    /// <summary>
    /// Provides a static, debug-only logging wrapper that is **entirely removed**
    /// from Release builds using <see cref="ConditionalAttribute"/>.
    /// <para>
    /// This allows all engine subsystems (renderer, memory manager, physics,
    /// scene graph, tile map, etc.) to call logging code without incurring
    /// **any runtime overhead** in Release builds.
    /// </para>
    /// <para>
    /// All logging work and parameter evaluation is skipped at compile time.
    /// In Debug builds, calls are routed to <see cref="Backend"/>,
    /// which can be swapped at runtime.
    /// </para>
    /// </summary>
    public static class DebugLogger
    {
        /// <summary>
        /// Gets or sets the backend logger instance used for debug logging.
        /// <para>
        /// Must implement <see cref="IEngineLogger"/>.
        /// Defaults to <see cref="InMemoryLogger.Instance"/>.
        /// </para>
        /// <para>
        /// You can replace this with any logger (console, file, network, UI)
        /// during engine initialization, as long as <c>DEBUG</c> is defined.
        /// </para>
        /// <para>
        /// In Release builds, this property has **no effect**, since all debug
        /// logging calls are removed at compile time.
        /// </para>
        /// </summary>
        public static IEngineLogger Backend { get; set; } = InMemoryLogger.Instance;

        /// <summary>
        /// Writes a debug-level message to the configured backend.
        /// <para>This method is **compiled out in Release builds**.</para>
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Debug(string message, params object[] args)
            => Backend.LogDebug(message, args);

        /// <summary>
        /// Writes a trace-level message to the configured backend.
        /// <para>This method is **compiled out in Release builds**.</para>
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Trace(string message, params object[] args)
            => Backend.LogTrace(message, args);

        /// <summary>
        /// Writes an information-level message to the configured backend.
        /// <para>This method is **compiled out in Release builds**.</para>
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Info(string message, params object[] args)
            => Backend.LogInformation(message, args);

        /// <summary>
        /// Writes a warning-level message to the configured backend.
        /// <para>This method is **compiled out in Release builds**.</para>
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Warn(string message, params object[] args)
            => Backend.LogWarning(message, args);

        /// <summary>
        /// Writes an error-level message to the configured backend.
        /// <para>This method is **compiled out in Release builds**.</para>
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Error(string message, params object[] args)
            => Backend.LogError(message, args);

        /// <summary>
        /// Writes a debug-level message including caller metadata
        /// (file name, line number and calling method).
        /// <para>
        /// Useful for debugging the engine’s execution flow,
        /// especially in hot paths where stack traces are too slow.
        /// </para>
        /// <para>
        /// This method is **compiled out in Release**.
        /// </para>
        /// </summary>
        /// <param name="message">The main log message format string.</param>
        /// <param name="caller">Automatically filled with the calling member name.</param>
        /// <param name="file">Automatically filled with the calling source file path.</param>
        /// <param name="line">Automatically filled with the source line number.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG")]
        public static void Debug(
            string message,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            params object[] args)
        {
            var prefix = $"{Path.GetFileName(file)}:{line} {caller} → ";
            Backend.LogDebug(prefix + message, args);
        }
    }
}
