﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugProcessing.cs
 * PURPOSE:     Handle the messages
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable SwitchStatementMissingSomeCases

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Debugger
{
    /// <summary>
    ///     Simple class for handling .txt files
    /// </summary>
    internal static class DebugProcessing
    {
        /// <summary>
        ///     The is active flag, should we log Data?
        /// </summary>
        private static bool _isActive;

        /// <summary>
        ///     The processing task
        /// </summary>
        private static readonly Task ProcessingTask;

        /// <summary>
        ///     The write semaphore
        /// </summary>
        private static readonly SemaphoreSlim WriteSemaphore = new(1, 1);

        /// <summary>
        ///     The message queue
        /// </summary>
        private static readonly ConcurrentQueue<string> MessageQueue = new();

        /// <summary>
        ///     The cancellation token source
        /// </summary>
        private static readonly CancellationTokenSource CancellationTokenSource = new();

        /// <summary>
        ///     The message queued event
        /// </summary>
        private static readonly ManualResetEventSlim MessageQueuedEvent = new(false);

        /// <summary>
        ///     The directory where log files are stored.
        /// </summary>
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        ///     The log file name prefix.
        /// </summary>
        private static readonly string LogFileName = "debug_log";

        /// <summary>
        ///     The log file extension.
        /// </summary>
        private static readonly string LogFileExtension = ".log";

        /// <summary>
        ///     Initializes the <see cref="DebugProcessing" /> class.
        /// </summary>
        static DebugProcessing()
        {
            // Ensure log directory exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            // Start a background task to process the message queue
            ProcessingTask = Task.Run(() => ProcessMessageQueueAsync(CancellationTokenSource.Token));
        }

        /// <summary>
        ///     Entry Point for all Debug Messages
        ///     Only called by the Error Box
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        ///     3 ... External Source
        /// </summary>
        /// <param name="error">Error Message</param>
        /// <param name="lvl">Level of Error</param>
        /// <param name="info">The information.</param>
        internal static void CreateLogFile(string error, ErCode lvl, string info)
        {
            if (!_isActive)
            {
                return;
            }

            var message = CreateLogMessage(error, string.Empty, lvl, info);
            LogError(message, lvl);
        }

        /// <summary>
        ///     Just creates adds something to the Log File if it doesn't exist it will create one
        ///     Only called by the Error Box
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        ///     3 ... External Source
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">Error Message</param>
        /// <param name="lvl">Level of Error</param>
        /// <param name="obj">The Object</param>
        /// <param name="info">The information.</param>
        internal static void CreateLogFile<T>(string error, ErCode lvl, T obj, string info)
        {
            if (!_isActive)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertObjectToXml(obj);
            var message = CreateLogMessage(error, objectString, lvl, info);
            LogError(message, lvl);
        }

        /// <summary>
        ///     Just creates adds something to the Log File if it doesn't exist it will create one
        ///     Only called by the Error Box
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        ///     3 ... External Source
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="error">Error Message</param>
        /// <param name="lvl">Level of Error</param>
        /// <param name="objLst">Enumeration of Objects</param>
        /// <param name="info">The information.</param>
        internal static void CreateLogFile<T>(string error, ErCode lvl, IEnumerable<T> objLst, string info)
        {
            if (!_isActive)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertListXml(objLst);
            var message = CreateLogMessage(error, objectString, lvl, info);
            LogError(message, lvl);
        }

        /// <summary>
        ///     Just creates adds something to the Log File if it doesn't exist it will create one
        ///     Only called by the Error Box
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        ///     3 ... External Source
        /// </summary>
        /// <typeparam name="T">Type of Key</typeparam>
        /// <typeparam name="TU">Type of Value</typeparam>
        /// <param name="error">Error Message</param>
        /// <param name="lvl">Level of Error</param>
        /// <param name="objectDictionary">Dictionary Object</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        internal static void CreateLogFile<T, TU>(string error, ErCode lvl,
            Dictionary<T, TU> objectDictionary, string info)
        {
            if (!_isActive)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertDictionaryXml(objectDictionary);
            var message = CreateLogMessage(error, objectString, lvl, info);
            LogError(message, lvl);
        }

        /// <summary>
        ///     Simple Debug Dump of the latest Messages
        /// </summary>
        internal static void CreateDump()
        {
            Trace.Flush();
            DebugRegister.IsDumpActive = true;
        }

        /// <summary>
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        ///     3 ... External Source
        /// </summary>
        /// <param name="error">Error Message</param>
        /// <param name="objectString">Object converted to XML</param>
        /// <param name="lvl">Level of Error</param>
        /// <param name="methods">Name of the method</param>
        /// <returns>
        ///     Error Message
        /// </returns>
        private static string CreateLogMessage(string error, string objectString, ErCode lvl,
            string methods)
        {
            //decide with Log lvl
            switch (lvl)
            {
                case ErCode.Error:
                    error = string.Concat(DebuggerResources.LogLvlOne, DateTime.Now, DebuggerResources.Spacer, error);
                    break;

                case ErCode.Warning:
                    error = string.Concat(DebuggerResources.LogLvlTwo, DateTime.Now, DebuggerResources.Spacer, error);
                    break;

                case ErCode.Information:
                    error = string.Concat(DebuggerResources.LogLvlThree, DateTime.Now, DebuggerResources.Spacer, error);
                    break;

                case ErCode.External:
                    error = string.Concat(DebuggerResources.LogLvlFour, DateTime.Now, DebuggerResources.Spacer, error);
                    break;
            }

            //Add Object if we deliver it
            if (!string.IsNullOrEmpty(objectString))
            {
                error = string.Concat(error, DebuggerResources.ObjectFormatting, objectString);
            }

            // Add Call Stack
            return string.Concat(error, Environment.NewLine, methods);
        }

        /// <summary>
        ///     Just creates adds something to the Log File if it doesn't exist it will create one
        ///     0 ... error
        ///     1 ... warning
        ///     2 ... Information
        /// </summary>
        /// <param name="message">Complete Error Message</param>
        /// <param name="lvl">Level of Error</param>
        private static void LogError(string message, ErCode lvl)
        {
            //we don't want to crash just because we exceed the Log
            if (DebugLog.CurrentLog.Capacity < DebugLog.CurrentLog.Count)
            {
                DebugLog.CurrentLog.Clear();
            }

            DebugLog.CurrentLog.Add(message);

            Trace.WriteLine(message);

            /*
             *  Errors will always be logged down,
             *  if someone issued the Dump Command so we add everything to the File.
             *  Of course we still Trace everything.
            */
            if (lvl == 0 && !DebugRegister.IsDumpActive)
            {
                WriteFile(message);
            }

            if (DebugRegister.IsDumpActive)
            {
                WriteFile(message);
            }
        }

        /// <summary>
        ///     Initiate debug.
        /// </summary>
        internal static void InitiateDebug()
        {
            //Initiate Log file
            DebugLog.CurrentLog = new List<string>();
            //say we started
            DebugRegister.IsRunning = _isActive = true;
        }

        /// <summary>
        ///     Stop Debugging Window
        /// </summary>
        internal static async Task StopDebuggingAsync()
        {
            DebugRegister.IsRunning = _isActive = false;

            // Signal the cancellation token
            CancellationTokenSource.Cancel();

            // Await the background task to finish processing
            if (ProcessingTask != null)
            {
                await ProcessingTask;
            }

            Trace.Close();
        }

        /// <summary>
        ///     Writes the file.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void WriteFile(string message)
        {
            // Enqueue the message
            MessageQueue.Enqueue(message);
            // Signal that a message has been queued
            MessageQueuedEvent.Set();
        }

        /// <summary>
        ///     Processes the message queue asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private static async Task ProcessMessageQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || !MessageQueue.IsEmpty)
            {
                try
                {
                    // Wait for a message to be enqueued or until cancellation is requested
                    MessageQueuedEvent.Wait(cancellationToken);

                    // Dequeue all messages and write them to the file
                    while (MessageQueue.TryDequeue(out var message))
                    {
                        await WriteToFileAsync(message);
                    }

                    // Reset the event after processing all messages
                    MessageQueuedEvent.Reset();
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Concat(DebuggerResources.ErrorProcessing, ex.Message));
                }
            }
        }

        /// <summary>
        ///     Writes to file asynchronously, handling file size and rotation.
        /// </summary>
        /// <param name="message">The message.</param>
        private static async Task WriteToFileAsync(string message)
        {
            await WriteSemaphore.WaitAsync(); // Ensure thread safety using SemaphoreSlim

            try
            {
                var filePath = GetLogFilePath();

                // Check file size and rotate if necessary
                if (new FileInfo(filePath).Length > DebugRegister.MaxFileSize)
                {
                    RotateLogFiles();
                    filePath = GetLogFilePath(); // Update path after rotation
                }

                // Append the message to the file asynchronously
                await File.AppendAllTextAsync(filePath, string.Concat(message, Environment.NewLine));
            }
            finally
            {
                WriteSemaphore.Release(); // Release the semaphore
            }

            // Simulate some delay (optional)
            await Task.Delay(DebuggerResources.Idle);
        }

        /// <summary>
        ///     Gets the current log file path.
        /// </summary>
        /// <returns>The current log file path.</returns>
        private static string GetLogFilePath()
        {
            return Path.Combine(LogDirectory, $"{LogFileName}{LogFileExtension}");
        }

        /// <summary>
        ///     Rotates the log files by renaming the current log file and deleting the oldest one if necessary.
        /// </summary>
        private static void RotateLogFiles()
        {
            // Get all existing log files
            var logFiles = Directory.GetFiles(LogDirectory, $"{LogFileName}*{LogFileExtension}");

            // Rename each file to shift the versions
            for (var i = logFiles.Length - 1; i >= 0; i--)
            {
                var newVersion = i + 1;
                var newFilePath = Path.Combine(LogDirectory, $"{LogFileName}_{newVersion}{LogFileExtension}");

                // If the new version exceeds the max number of files, delete it
                if (newVersion > DebugRegister.MaxFileCount)
                {
                    File.Delete(logFiles[i]);
                }
                else
                {
                    File.Move(logFiles[i], newFilePath);
                }
            }
        }
    }
}
