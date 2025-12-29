/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/MessageHandling.cs
 * PURPOSE:     Collects all Errors and Status Messages
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using CoreMemoryLog;

namespace SqliteHelper
{
    /// <summary>
    ///     The error logging class.
    /// </summary>
    internal static class MessageHandling
    {
        // Store references to the InMemoryLogger instance for both error and regular logs
        private static readonly InMemoryLogger Logger = InMemoryLogger.Instance;

        /// <summary>
        ///     Logs all activities
        /// </summary>
        internal static int MaxLinesError { get; set; } = 1000;

        /// <summary>
        ///     Logs for all activities in the library
        /// </summary>
        internal static int MaxLinesLog { get; set; } = 10000;

        /// <summary>
        ///     Logs all activities
        /// </summary>
        internal static List<string> LogFile { get; } = new();

        /// <summary>
        ///     Last Message
        /// </summary>
        internal static string LastError { get; private set; }

        /// <summary>
        ///     List of Errors
        /// </summary>
        internal static List<string> ListError { get; } = new();

        /// <summary>
        ///     Error Logging
        /// </summary>
        /// <param name="error">Error Message</param>
        /// <param name="logLvl">Level of Error, 0 is error, 1 is warning, 2 is information, later we can add higher lvl</param>
        internal static string SetMessage(string error, int logLvl)
        {
            // Map logLvl to LogLevel (you can extend this if more levels are needed)
            var level = logLvl switch
            {
                0 => LogLevel.Error,
                1 => LogLevel.Warning,
                2 => LogLevel.Information,
                _ => LogLevel.Information
            };

            // Log the message using InMemoryLogger
            Logger.Log(level, error, "SqliteHelper");

            // Store the message in the ListError for future reference
            if (logLvl == 0) // Error messages only for this list
            {
                ListError.Add(error);
                LastError = error; // Update last error message
            }

            // Optionally add to the log file for backup
            LogFile.Add(error);
            TrimLogsIfNeeded();

            // Return a confirmation string or the error message itself
            return $"Message logged: {error}";
        }

        /// <summary>
        ///     Clears the list of errors
        /// </summary>
        internal static void ClearErrors()
        {
            ListError.Clear(); // Clear only the ListError, not the InMemoryLogger
            LastError = null; // Clear the last error message
        }

        /// <summary>
        ///     Trims the logs if the size exceeds the MaxLinesError or MaxLinesLog
        /// </summary>
        private static void TrimLogsIfNeeded()
        {
            if (ListError.Count > MaxLinesError)
            {
                ListError.RemoveAt(0); // Remove the oldest error
            }

            if (LogFile.Count > MaxLinesLog)
            {
                LogFile.RemoveAt(0); // Remove the oldest log
            }
        }
    }
}
