/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Debugger
* FILE:        Debugger/DebugLog.cs
* PURPOSE:     Handle all incoming input
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    ///     The debug log class.
    /// </summary>
    public sealed class DebugLog : IDebugLog
    {
        /// <summary>
        /// Holds all messages for the.
        /// </summary>
        /// <value>
        /// The current log messages
        /// </value>
        public static IReadOnlyCollection<string> CurrentLog => Container.ToList();

        /// <summary>
        /// Gets or sets the container.
        /// Internal Handler more thead safe
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        internal static List<string> Container { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Start Debugging.
        /// </summary>
        public void Start()
        {
            DebugRegister.SuppressWindow = false;
            DebugProcessing.StartDebug();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Start with window.
        /// </summary>
        public void StartWindow()
        {
            DebugRegister.SuppressWindow = true;
            DebugProcessing.StartDebug();
            InitiateWindow();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Stop the debugging.
        /// </summary>
        public void StopDebugging()
        {
            DebugRegister.IsRunning = false;
            DebugProcessing.StopDebuggingClose();
            CloseWindow();
        }

        /// <summary>
        ///     Create a dump.
        /// </summary>
        public void CreateDump()
        {
            DebugProcessing.DebugFlushActivateDump();
        }

        /// <summary>
        ///     Delete the Log File.
        /// </summary>
        internal void Delete()
        {
            DebugProcessing.StopDebuggingClose();
            try
            {
                var logFiles = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath,
                    $"{DebugRegister.DebugPath}{DebuggerResources.LogFileExtension}");

                File.Delete(logFiles);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
            {
                LogFile(string.Concat(DebuggerResources.ErrorLogFileDelete, ex), ErCode.Error);
            }
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="debugLvl">The debug level, optional. Defines the abstraction lvl.</param>
        public void LogFile(string error, ErCode lvl, int debugLvl = 1)
        {
            //collect Stacktrace on verbose or Error
            var stackTrace = GetStackTraceInfo(lvl, debugLvl);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, stackTrace, path);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="obj">The object.</param>
        /// <param name="debugLvl">The debug level, optional. Defines the abstraction lvl.</param>
        public void LogFile<T>(string error, ErCode lvl, T obj, int debugLvl = 1)
        {
            //collect Stacktrace on verbose or Error
            var stackTrace = GetStackTraceInfo(lvl, debugLvl);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, obj, stackTrace, path);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="objLst">The object List.</param>
        /// <param name="debugLvl">The debug level, optional. Defines the abstraction lvl.</param>
        public void LogFile<T>(string error, ErCode lvl, IEnumerable<T> objLst, int debugLvl = 1)
        {
            //collect Stacktrace on verbose or Error
            var stackTrace = GetStackTraceInfo(lvl, debugLvl);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, objLst, stackTrace, path);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Key</typeparam>
        /// <typeparam name="TU">Type of Value</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="objectDictionary">The objectDictionary.</param>
        /// <param name="debugLvl">The debug level, optional. Defines the abstraction lvl.</param>
        public void LogFile<T, TU>(string error, ErCode lvl,
            Dictionary<T, TU> objectDictionary, int debugLvl = 1)
        {
            //collect Stacktrace on verbose or Error
            var stackTrace = GetStackTraceInfo(lvl, debugLvl);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, objectDictionary, stackTrace, path);
        }

        /// <summary>
        /// Helper method to generate stack trace info.
        /// </summary>
        /// <param name="lvl">The level of error.</param>
        /// <param name="debugLvl">The debug level. Defines the abstraction level.</param>
        /// <returns>
        /// Generated stack trace information as a string.
        /// </returns>
        private string GetStackTraceInfo(ErCode lvl, int debugLvl)
        {
            //only collect Stacktrace on verbose or Error
            var stackTrace = string.Empty;

            if (!DebugRegister.IsVerbose && lvl != ErCode.Error)
            {
                return stackTrace;
            }

            var st = new StackTrace(true);
            var methodName = st.GetFrame(debugLvl + 1)?.GetMethod()?.Name; // Add +1 here
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(debugLvl + 1).GetFileLineNumber(); // Adjust frame level
            var file = st.GetFrame(debugLvl + 1)?.GetFileName();
            stackTrace = GenerateInfo(methodName, line, file);

            return stackTrace;
        }


        /// <summary>
        ///     Generates the information.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="line">The line.</param>
        /// <param name="file">The file.</param>
        /// <returns>
        ///     The basic Caller Infos
        /// </returns>
        private static string GenerateInfo(string methodName, int line, string file)
        {
            return string.Concat(DebuggerResources.Caller, methodName, DebuggerResources.LineNumber, line,
                Environment.NewLine, DebuggerResources.Location, DebuggerResources.Formatting, file);
        }

        /// <summary>
        ///     The initiate window.
        /// </summary>
        private void InitiateWindow()
        {
            var path = Directory.GetCurrentDirectory();
            // Use ProcessStartInfo class.
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = Path.Combine(path, DebuggerResources.TrailWindow),
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = DebuggerResources.ArgumentsNone
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using-statement will close.
                Process.Start(startInfo);
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or ArgumentNullException
                                           or IOException)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        ///     Close the window.
        ///     With our Process Listener
        /// </summary>
        internal void CloseWindow()
        {
            Process[] proc = null;

            try
            {
                proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DebuggerResources.TrailWindow));

                //nothing found just bail out
                if (proc.Length == 0)
                {
                    return;
                }

                var debugger = proc[0];

                if (!debugger.HasExited)
                {
                    debugger.Kill();
                }
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                if (proc != null)
                {
                    foreach (var p in proc)
                    {
                        p.Dispose();
                    }
                }
            }
        }
    }
}
