/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Viewer
 * FILE:        App.cs
 * PURPOSE:     Entry point and application definition for the WPF application.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Core.Viewer
{
    /// <summary>
    /// Start of the application, defined in App.xaml.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// The library name
        /// </summary>
        private const string LibraryName = "Core.Viewer.App";

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
            Trace.WriteLine("Unhandled UI-thread exception", e.Exception.ToString());

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
            Trace.WriteLine("Unhandled fatal exception - process is terminating", e.ToString());

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
            Trace.WriteLine("Unobserved task exception", e.ToString());
            e.SetObserved();
        }

        /// <summary>
        /// Flushes the log to disk best effort.
        /// </summary>
        private static void FlushLogToDiskBestEffort()
        {
            try
            {
                //TODO
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
