/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/Trail.xaml.cs
 * PURPOSE:     Output Window
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, you can't make a starting Window internal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using CommonDialogs;
using CommonFilter;

namespace Debugger
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The trail class.
    /// </summary>
    public sealed partial class Trail
    {
        /// <summary>
        ///     The filter
        /// </summary>
        private readonly Filter _filter;

        /// <summary>
        ///     The counter.
        /// </summary>
        private int _counter;

        /// <summary>
        ///     The dispatcher timer.
        /// </summary>
        private DispatcherTimer _dispatcherTimer;

        /// <summary>
        ///     The index.
        /// </summary>
        private int _index;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Trail" /> class.
        /// </summary>
        public Trail()
        {
            InitializeComponent();
            _filter = new Filter();
        }

        /// <summary>
        ///     The window loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //set Index and Counter
            _index = _counter = ReadLines(DebugRegister.DebugPath).Count();

            DebugProcessing.StartDebug();

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            //Tell them that we started.
            DebugProcessing.DebugLogEntry(DebuggerResources.InformationCreateLogFile, ErCode.Information,
                DebuggerResources.ManualStart, path);

            StartTick();
        }

        /// <summary>
        ///     Start the tick.
        /// </summary>
        private void StartTick()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(DebugRegister.HourTick, DebugRegister.MinutesTick,
                DebugRegister.SecondsTick);

            _dispatcherTimer.Start();
        }

        /// <summary>
        ///     The dispatcher timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            _counter = ReadLines(DebugRegister.DebugPath).Count();

            if (_index == _counter)
            {
                return;
            }

            var diff = _counter - _index;

            var lst = ReadLines(DebugRegister.DebugPath).ToList();

            foreach (var line in lst.GetRange(_counter - diff, diff))
            {
                var textRange = new TextRange(Log.Document.ContentEnd, Log.Document.ContentEnd);

                DebugHelper.AddRange(textRange, line, false);
            }

            _index = _counter;
        }

        /// <summary>
        ///     Read the lines.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     The line we have read<see cref="T:IEnumerable{string}" />.
        /// </returns>
        private static IEnumerable<string> ReadLines(string path)
        {
            if (!File.Exists(path))
            {
                using var fs = new FileStream(path, FileMode.CreateNew);
            }
            else
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000,
                    FileOptions.SequentialScan);
                using var sr = new StreamReader(fs, Encoding.UTF8);

                while (sr.ReadLine() is { } line)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        ///     The menu close click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     The menu delete click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenDel_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer?.Stop();
            DebugProcessing.StopDebuggingClose();
            try
            {
                File.Delete(DebugRegister.DebugPath);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
            {
                Trace.WriteLine(string.Concat(DebuggerResources.ErrorLogFileDelete, ex), nameof(ErCode.Error));
            }
        }

        /// <summary>
        ///     The menu stop click. Stops Debugging
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenStop_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer?.Stop();
            DebugProcessing.StopDebuggingClose();
        }

        /// <summary>
        ///     The menu start click. Start Debugging
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenStart_Click(object sender, RoutedEventArgs e)
        {
            //get index
            _index = ReadLines(DebugRegister.DebugPath).Count();
            DebugProcessing.StartDebug();

            var path = DebugHelper.GetLogFile(DebugRegister.DebugPath);
            //Tell them we started
            DebugProcessing.DebugLogEntry(DebuggerResources.InformationCreateLogFile, ErCode.Information,
                DebuggerResources.ManualStart, path);
            StartTick();
        }

        /// <summary>
        ///     The menu load log File click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenLoadA_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(DebugRegister.DebugPath))
            {
                return;
            }

            _ = LoadFile();
        }

        /// <summary>
        ///     Handles the Click event of the MenLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void MenLog_Click(object sender, RoutedEventArgs e)
        {
            var file = FileIoHandler.HandleFileOpen(DebuggerResources.FileExt);

            if (file != null && !File.Exists(file.FilePath))
            {
                return;
            }

            //already checked, can't be null
            // ReSharper disable PossibleNullReferenceException
            DebugRegister.DebugPath = file.FilePath;
            // ReSharper restore PossibleNullReferenceException

            _ = LoadFile();
        }

        /// <summary>
        ///     The menu clear click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenClear_Click(object sender, RoutedEventArgs e)
        {
            Log.Document.Blocks.Clear();
        }

        /// <summary>
        ///     The menu config click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MenConfig_Click(object sender, RoutedEventArgs e)
        {
            var conf = new ConfigWindow();
            conf.Show();
        }

        /// <summary>
        ///     Loads the file async to speed up the whole mess.
        /// </summary>
        private async Task LoadFile()
        {
            _dispatcherTimer?.Stop();

            DebugProcessing.StopDebuggingClose();

            Log.Document.Blocks.Clear();

            var lines = ReadLines(DebugRegister.DebugPath).ToList();

            await Task.Run(() =>
            {
                foreach (var line in lines)
                {
                    // Use Dispatcher to update the UI
                    Dispatcher.Invoke(() =>
                    {
                        var textRange = new TextRange(Log.Document.ContentEnd, Log.Document.ContentEnd);
                        DebugHelper.AddRange(textRange, line, false);
                    });
                }
            });

            //set index
            _index = ReadLines(DebugRegister.DebugPath).Count();
            DebugProcessing.StartDebug();

            _dispatcherTimer?.Start();
        }

        /// <summary>
        ///     Handles the Click event of the MenFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void MenFilter_Click(object sender, RoutedEventArgs e)
        {
            _filter.FilterChanged += FilterChanged;
            _filter.Start();
        }

        /// <summary>
        ///     Handles the FilterChanged event of the _filter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void FilterChanged(object sender, EventArgs e)
        {
            Log.Document.Blocks.Clear();

            foreach (var line in ReadLines(DebugRegister.DebugPath).ToList())
            {
                var textRange = new TextRange(Log.Document.ContentEnd, Log.Document.ContentEnd);

                var check = _filter.CheckFilter(line);

                DebugHelper.AddRange(textRange, line, check);
            }

            //set index
            _index = ReadLines(DebugRegister.DebugPath).Count();
        }
    }
}
