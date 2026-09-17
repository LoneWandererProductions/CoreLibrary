/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        App.xaml.cs
 * PURPOSE:     Entry Point, with Exception handlung
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Core.MemoryLog;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    ///     The app class.
    ///     Entry Point for the Executable
    /// </summary>
    internal sealed partial class App
    {
        /// <summary>
        /// The library name
        /// </summary>
        private const string LibraryName = "Debugger.App";

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Exceptions thrown while WPF processes UI-thread work (event handlers,
            // command execution, bindings) surface here. This is the main safety net
            // for a desktop app - without it, an exception that escapes an event
            // handler tears down the whole process with no record of what happened.
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Exceptions on any OTHER thread (raw Task.Run work, timers, etc) that
            // would otherwise crash the process outright. By the time this fires the
            // runtime has already decided to terminate - that can't be stopped from
            // here - but the exception can still be logged before it goes down.
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

            // Exceptions from "fire and forget" Tasks nobody awaited (e.g. a call like
            // `LoadThumbs(folder, file);` with no `await` or `_ =`). Without this,
            // those exceptions are silently swallowed once the Task is collected.
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        /// <summary>
        /// Called when [dispatcher unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            InMemoryLogger.Instance.Log(LogLevel.Error, "Unhandled UI-thread exception",
                libraryName: LibraryName, exception: e.Exception);

            MessageBox.Show(
                "Something went wrong and the last action couldn't be completed. Details were logged.",
                "SlimViewer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Keeps the app alive instead of crashing - a reasonable default for a
            // viewer, where one bad file or filter shouldn't end the whole session.
            // If the same exception shows up repeatedly, treat it as a bug to fix
            // rather than something to keep swallowing: continuing blindly can leave
            // the UI in a half-updated state.
            e.Handled = true;
        }

        /// <summary>
        /// Called when [application domain unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            InMemoryLogger.Instance.Log(LogLevel.Critical, "Unhandled fatal exception - process is terminating",
                libraryName: LibraryName, exception: ex);

            // The process is going down. InMemoryLogger is, as the name says, only
            // in memory - that queue dies with the process unless it's flushed to
            // disk right now, synchronously, before this handler returns.
            FlushLogToDiskBestEffort();
        }

        /// <summary>
        /// Called when [unobserved task exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            InMemoryLogger.Instance.Log(LogLevel.Error, "Unobserved task exception",
                libraryName: LibraryName, exception: e.Exception);
            e.SetObserved();
        }

        /// <summary>
        /// Flushes the log to disk best effort.
        /// </summary>
        private static void FlushLogToDiskBestEffort()
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SlimViewer");
                Directory.CreateDirectory(dir);

                InMemoryLogger.Instance.DumpToFile(
                    Path.Combine(dir, "crash.log"),
                    append: true,
                    minimumLevel: LogLevel.Warning);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
