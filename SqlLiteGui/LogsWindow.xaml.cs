/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/InputBinaryWindow.xaml.cs
 * PURPOSE:     Show all Logs from the current Connection
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Windows;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The logs window class.
    /// </summary>
    internal sealed partial class LogsWindow
    {
        /// <summary>
        ///     The list errors (readonly).
        /// </summary>
        private readonly List<string> _listErrors;

        /// <summary>
        ///     The log file (readonly).
        /// </summary>
        private readonly List<string> _logFile;

        /// <summary>
        ///     The title (readonly).
        /// </summary>
        private readonly string _title;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.LogsWindow" /> class.
        /// </summary>
        internal LogsWindow()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.LogsWindow" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="listErrors">The list Errors.</param>
        /// <param name="logFile">The log File.</param>
        public LogsWindow(string title, List<string> listErrors, List<string> logFile)
        {
            InitializeComponent();
            _title = title;
            _listErrors = listErrors;
            _logFile = logFile;
        }

        /// <summary>
        ///     Get the basics in Place
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = _title;
            foreach (var errors in _listErrors)
            {
                TextBoxErrors.AppendText(string.Concat(errors, Environment.NewLine));
            }

            foreach (var logs in _logFile)
            {
                TextBoxLogs.AppendText(string.Concat(logs, Environment.NewLine));
            }
        }
    }
}
