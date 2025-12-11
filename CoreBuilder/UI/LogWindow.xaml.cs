/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.UI
 * FILE:        LogWindow.xaml.cs
 * PURPOSE:     Basic log window for displaying messages.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows;

namespace CoreBuilder.UI
{
    /// <summary>
    /// Simple Display window for logging messages
    /// </summary>
    public partial class LogWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogWindow"/> class.
        /// </summary>
        public LogWindow()
        {
            InitializeComponent();
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
                Dispatcher.Invoke(() => AppendInternal(message));
            }
        }

        /// <summary>
        /// Appends the internal.
        /// </summary>
        /// <param name="message">The message.</param>
        private void AppendInternal(string message)
        {
            if (ShowTimestampCheck.IsChecked == true)
            {
                var ts = DateTime.Now.ToString("HH:mm:ss.fff");
                message = $"[{ts}] {message}";
            }

            LogText.Text += message + Environment.NewLine;

            if (AutoScrollCheck.IsChecked == true)
                ScrollToBottom();
        }

        /// <summary>
        /// Scrolls to bottom.
        /// </summary>
        private void ScrollToBottom()
        {
            ScrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Scrolls to top.
        /// </summary>
        private void ScrollToTop()
        {
            ScrollViewer.ScrollToHome();
        }

        // Menu Handlers

        /// <summary>
        /// Handles the Click event of the ClearLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
            => LogText.Text = string.Empty;

        /// <summary>
        /// Handles the Click event of the GoTop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GoTop_Click(object sender, RoutedEventArgs e)
            => ScrollToTop();

        /// <summary>
        /// Handles the Click event of the GoBottom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GoBottom_Click(object sender, RoutedEventArgs e)
            => ScrollToBottom();
    }
}
