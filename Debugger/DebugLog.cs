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
            DebugRegister.ReadConfigFile();
            DebugRegister.SuppressWindow = false;
            DebugProcessing.InitiateDebug();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Start with window.
        /// </summary>
        public void StartWindow()
        {
            DebugRegister.SuppressWindow = true;
            DebugProcessing.InitiateDebug();
            InitiateWindow();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Stop the debugging.
        /// </summary>
        public void StopDebugging()
        {
            DebugRegister.IsRunning = false;
            DebugProcessing.StopDebugging();
            CloseWindow();
        }

        /// <summary>
        ///     Create a dump.
        /// </summary>
        public static void CreateDump()
        {
            DebugProcessing.CreateDump();
        }

        /// <summary>
        ///     Delete the Log File.
        /// </summary>
        internal static void Delete()
        {
            DebugProcessing.StopDebugging();
            try
            {
                File.Delete(DebugRegister.DebugPath);
            }
            catch (ArgumentException ex)
            {
                CreateLogFile(string.Concat(DebuggerResources.ErrorLogFileDelete, ex), ErCode.Error);
            }
            catch (IOException ex)
            {
                CreateLogFile(string.Concat(DebuggerResources.ErrorLogFileDelete, ex), ErCode.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                CreateLogFile(string.Concat(DebuggerResources.ErrorLogFileDelete, ex), ErCode.Error);
            }
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        public static void CreateLogFile(string error, ErCode lvl)
        {
            var st = new StackTrace(true);

            var methodName = st.GetFrame(1)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(1).GetFileLineNumber();
            var file = st.GetFrame(1)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            DebugProcessing.CreateLogFile(error, lvl, info);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="obj">The object.</param>
        public static void CreateLogFile<T>(string error, ErCode lvl, T obj)
        {
            var st = new StackTrace(true);

            var methodName = st.GetFrame(1)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(1).GetFileLineNumber();
            var file = st.GetFrame(1)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            DebugProcessing.CreateLogFile(error, lvl, obj, info);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="objLst">The object List.</param>
        public static void CreateLogFile<T>(string error, ErCode lvl, IEnumerable<T> objLst)
        {
            var st = new StackTrace(true);

            var methodName = st.GetFrame(1)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(1).GetFileLineNumber();
            var file = st.GetFrame(1)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            DebugProcessing.CreateLogFile(error, lvl, objLst, info);
        }

        /// <summary>
        ///     Create the log file.
        /// </summary>
        /// <typeparam name="T">Type of Key</typeparam>
        /// <typeparam name="TU">Type of Value</typeparam>
        /// <param name="error">The error.</param>
        /// <param name="lvl">The lvl.</param>
        /// <param name="objectDictionary">The objectDictionary.</param>
        public static void CreateLogFile<T, TU>(string error, ErCode lvl,
            Dictionary<T, TU> objectDictionary)
        {
            var st = new StackTrace(true);

            var methodName = st.GetFrame(1)?.GetMethod()?.Name;
            // ReSharper disable once PossibleNullReferenceException
            var line = st.GetFrame(1).GetFileLineNumber();
            var file = st.GetFrame(1)?.GetFileName();

            var info = GenerateInfo(methodName, line, file);

            DebugProcessing.CreateLogFile(error, lvl, objectDictionary, info);
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
                Environment.NewLine, DebuggerResources.Location, DebuggerResources.Formating, file);
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
            catch (Win32Exception ex)
            {
                Debug.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        ///     Close the window.
        ///     With our Process Listener
        /// </summary>
        private static void CloseWindow()
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
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine(ex);
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex);
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
