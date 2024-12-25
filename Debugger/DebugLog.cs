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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    ///     The debug log class.
    /// </summary>
    public sealed class DebugLog : IDebugLog
    {
        /// <summary>
        ///     Holds all messages for the.
        /// </summary>
        /// <value>
        ///     The current log messages
        /// </value>
        public static List<string> CurrentLog { get; internal set; }

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
        /// Create the log file.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="debugLvl">The debug level, optional. Defines the abstraction lvl.</param>
        public void LogFile(string error, ErCode lvl, int debugLvl = 1)
        {
            var st = new StackTrace(true);

            var methodName = st.GetFrame(debugLvl)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(debugLvl).GetFileLineNumber();
            var file = st.GetFrame(debugLvl)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, info, path);
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
            var st = new StackTrace(true);

            var methodName = st.GetFrame(debugLvl)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(debugLvl).GetFileLineNumber();
            var file = st.GetFrame(debugLvl)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, obj, info, path);
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
            var st = new StackTrace(true);

            var methodName = st.GetFrame(debugLvl)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(debugLvl).GetFileLineNumber();
            var file = st.GetFrame(debugLvl)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, objLst, info, path);
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
            var st = new StackTrace(true);

            var methodName = st.GetFrame(debugLvl)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(debugLvl).GetFileLineNumber();
            var file = st.GetFrame(debugLvl)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            DebugProcessing.DebugLogEntry(error, lvl, objectDictionary, info, path);
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
