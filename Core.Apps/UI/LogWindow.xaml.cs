/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.UI
 * FILE:        LogWindow.xaml.cs
 * PURPOSE:     Basic log window for displaying messages.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.UI;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace CoreBuilder.UI
{
    /// <summary>
    /// Log Window - A simple WPF window that displays log messages in a ListBox. It supports auto-scrolling, timestamp display, and clearing the log.
    /// Designed for use in long-running applications to monitor events and messages.
    /// </summary>
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LogWindow
    {
        /// <summary>
        /// The data source for the ListBox
        /// </summary>
        private readonly ObservableCollection<LogEntry> _logEntries = new();

        /// <summary>
        /// Prevent infinite memory growth for long-running sessions
        /// The maximum log lines
        /// </summary>
        private const int MaxLogLines = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogWindow"/> class.
        /// </summary>
        public LogWindow()
        {
            InitializeComponent();
            LogList.ItemsSource = _logEntries; // Bind the UI to the collection
        }

        /// <summary>
        /// Appends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Append(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                AppendInternal(message);
            }
            else
            {
                // BeginInvoke is better here so background threads aren't blocked waiting for the UI
                Dispatcher.BeginInvoke(() => AppendInternal(message));
            }
        }

        /// <summary>
        /// Appends the internal.
        /// </summary>
        /// <param name="message">The message.</param>
        private void AppendInternal(string message)
        {
            // 1. Create the entry
            var entry = new LogEntry
            {
                Message = message,
                Timestamp = ShowTimestampCheck.IsChecked == true
                    ? DateTime.Now.ToString("HH:mm:ss.fff")
                    : string.Empty
            };

            // 2. Add it to the list
            _logEntries.Add(entry);

            // 3. Keep memory in check (Rolling Log)
            if (_logEntries.Count > MaxLogLines)
            {
                _logEntries.RemoveAt(0); // Drop the oldest entry
            }

            // 4. Handle Auto-Scroll
            if (AutoScrollCheck.IsChecked == true)
            {
                ScrollToBottom();
            }
        }

        /// <summary>
        /// Scrolls to bottom.
        /// </summary>
        private void ScrollToBottom()
        {
            if (_logEntries.Count > 0)
            {
                LogList.ScrollIntoView(_logEntries[^1]); // Scroll to the last item
            }
        }

        /// <summary>
        /// Scrolls to top.
        /// </summary>
        private void ScrollToTop()
        {
            if (_logEntries.Count > 0)
            {
                LogList.ScrollIntoView(_logEntries[0]);
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
            => _logEntries.Clear(); // Just clear the collection, UI updates automatically

        /// <summary>
        /// Handles the Click event of the GoTop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GoTop_Click(object sender, RoutedEventArgs e) => ScrollToTop();

        /// <summary>
        /// Handles the Click event of the GoBottom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GoBottom_Click(object sender, RoutedEventArgs e) => ScrollToBottom();
    }
}