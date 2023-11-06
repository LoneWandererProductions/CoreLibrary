/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/ErrorLogging.cs
 * PURPOSE:     The usual error Logging
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;

namespace Interpreter
{
    /// <summary>
    ///     The error logging class.
    /// </summary>
    internal static class ErrorLogging
    {
        /// <summary>
        ///     Error Logging
        /// </summary>
        /// <param name="error">Error Message</param>
        /// <param name="logLvl">Level of Error, 0 is Warning, 1 is error, later we can add higher lvl</param>
        /// <returns>The converted Error Message<see cref="string" />.</returns>
        internal static string SetLastError(string error, int logLvl)
        {
            string message;

            switch (logLvl)
            {
                case 0:
                    message = string.Concat(IrtConst.MessageError, DateTime.Now, IrtConst.End, Environment.NewLine,
                        error,
                        Environment.NewLine);
                    break;

                case 1:
                    message = string.Concat(IrtConst.MessageWarning, DateTime.Now, IrtConst.End, Environment.NewLine,
                        error,
                        Environment.NewLine);
                    break;

                case 2:
                    message = string.Concat(IrtConst.MessageInfo, DateTime.Now, IrtConst.End, Environment.NewLine,
                        error,
                        Environment.NewLine);
                    break;

                default: return string.Empty;
            }

            Trace.WriteLine(string.Concat(message, Environment.NewLine));

            return message;
        }
    }
}
