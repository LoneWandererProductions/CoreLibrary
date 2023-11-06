/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/MessageHandling.cs
 * PURPOSE:     Collects all Errors and Status Messages
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SQLiteHelper
{
    /// <summary>
    ///     The error logging class.
    /// </summary>
    internal static class MessageHandling
    {
        /// <summary>
        ///     The error log.
        /// </summary>
        private static int _errorLog;

        /// <summary>
        ///     The message log.
        /// </summary>
        private static int _messageLog;

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
            switch (logLvl)
            {
                case 0:
                    LastError = string.Concat(SqLiteHelperResources.MessageError, DateTime.Now,
                        SqLiteHelperResources.End, Environment.NewLine, error);
                    break;
                case 1:
                    LastError = string.Concat(SqLiteHelperResources.MessageWarning, DateTime.Now,
                        SqLiteHelperResources.End, Environment.NewLine, error);
                    break;
                case 2:
                    LastError = string.Concat(SqLiteHelperResources.MessageInfo, DateTime.Now,
                        SqLiteHelperResources.End, Environment.NewLine, error);
                    break;
                default: return string.Empty;
            }

            if (_errorLog == MaxLinesError)
            {
                ListError.RemoveAt(0);
            }

            if (_messageLog == MaxLinesError)
            {
                LogFile.RemoveAt(0);
            }

            if (logLvl == 0)
            {
                ListError.Add(LastError);
                _errorLog++;
            }

            LogFile.Add(LastError);

            _messageLog++;

            Trace.WriteLine(string.Concat(LastError, Environment.NewLine));

            return LastError;
        }

        /// <summary>
        ///     If we delete, create or Switch Database Context we don't need the old error Logs
        /// </summary>
        internal static void ClearErrors()
        {
            LastError = SqLiteHelperResources.ErrorCheck;
            ListError.Clear();
            LogFile.Clear();
            ListError.Clear();
        }
    }
}
