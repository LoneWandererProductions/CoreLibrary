/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/Trail.xaml.cs
 * PURPOSE:     Output Window (Log Viewer)
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using CommonDialogs;
using CommonFilter;

namespace Debugger;

/// <inheritdoc cref="Window" />
/// <summary>
///     The <see cref="Trail"/> class represents the main log viewer window.
///     It displays log messages from a given <see cref="ILogSource"/> and allows filtering,
///     switching sources, clearing logs, and basic configuration.
/// </summary>
public sealed partial class Trail
{
    /// <summary>
    /// Filter instance used to evaluate whether lines should be highlighted.
    /// </summary>
    private readonly Filter _filter;

    /// <summary>
    /// The current log source. Can be swapped at runtime.
    /// </summary>
    private ILogSource _logSource;

    /// <summary>
    /// Dispatcher timer used to periodically poll the log source for updates.
    /// </summary>
    private DispatcherTimer _dispatcherTimer;

    /// <summary>
    /// Tracks the last known line count to avoid re-appending duplicate entries.
    /// </summary>
    private int _lastLineCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="Trail"/> class
    /// with a default <see cref="FileLogSource"/> ("default.log").
    /// This constructor is required for XAML/WPF designer support.
    /// </summary>
    public Trail() : this(new FileLogSource("default.log"))
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Trail" /> class
    /// with a specific <paramref name="logSource" />.
    /// </summary>
    /// <param name="logSource">The initial log source (must not be null).</param>
    /// <exception cref="System.ArgumentNullException">logSource</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logSource" /> is null.</exception>
    public Trail(ILogSource logSource)
    {
        InitializeComponent();
        _filter = new Filter();
        _logSource = logSource ?? throw new ArgumentNullException(nameof(logSource));
        _logSource.LineReceived += OnLogLineReceived;
    }

    /// <summary>
    /// Handles the window <see cref="FrameworkElement.Loaded" /> event.
    /// Loads all current log entries, scrolls to the end, and starts listening for updates.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (var line in _logSource.ReadAll())
        {
            AppendLine(line, false);
        }

        Log.ScrollToEnd();
        _logSource.Start();
        StartTick();
    }

    /// <summary>
    /// Starts the dispatcher timer used to periodically refresh log content.
    /// </summary>
    private void StartTick()
    {
        _dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) // configurable interval
        };
        _dispatcherTimer.Tick += DispatcherTimer_Tick;
        _dispatcherTimer.Start();
    }

    /// <summary>
    /// Periodic tick handler. Compares line counts and appends any new lines from the log source.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        var lines = _logSource.ReadAll().ToList();
        if (lines.Count == _lastLineCount)
        {
            return;
        }

        foreach (var line in lines.Skip(_lastLineCount))
        {
            AppendLine(line, false);
        }

        Log.ScrollToEnd();
        _lastLineCount = lines.Count;
    }

    /// <summary>
    /// Event handler called when the log source emits a new line.
    /// Appends the line to the UI on the dispatcher thread.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="line">The line.</param>
    private void OnLogLineReceived(object sender, string line)
    {
        Dispatcher.Invoke(() => AppendLine(line, false));
    }

    /// <summary>
    /// Replaces the current log source with a new one.
    /// Stops and unsubscribes from the old source, clears the UI,
    /// loads the new source, and resumes listening.
    /// </summary>
    /// <param name="newSource">The new log source (must not be null).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="newSource"/> is null.</exception>
    private void ReplaceLogSource(ILogSource newSource)
    {
        if (newSource == null) throw new ArgumentNullException(nameof(newSource));
        if (ReferenceEquals(newSource, _logSource)) return;

        _dispatcherTimer?.Stop();

        try
        {
            _logSource.LineReceived -= OnLogLineReceived;
            _logSource.Stop();
        }
        catch
        {
            // Defensive: swallow exceptions during cleanup
        }

        _logSource = newSource;
        _logSource.LineReceived += OnLogLineReceived;

        Log.Document.Blocks.Clear();
        var lines = _logSource.ReadAll().ToList();
        foreach (var line in lines)
        {
            AppendLine(line, false);
        }

        _lastLineCount = lines.Count;
        Log.ScrollToEnd();

        _logSource.Start();
        _dispatcherTimer?.Start();
    }

    /// <summary>
    /// Appends a line of text to the log viewer.
    /// </summary>
    /// <param name="line">The text to append.</param>
    /// <param name="highlight">True to highlight the line, false otherwise.</param>
    private void AppendLine(string line, bool highlight)
    {
        var textRange = new TextRange(Log.Document.ContentEnd, Log.Document.ContentEnd);
        DebugHelper.AddRange(textRange, line, highlight);
    }

    /// <summary>
    /// Menu handler: Closes the window.
    /// </summary>
    private void MenClose_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Menu handler: Stops the log source and attempts to delete its backing file (if file-based).
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenDel_Click(object sender, RoutedEventArgs e)
    {
        _dispatcherTimer?.Stop();
        _logSource.Stop();

        try
        {
            if (_logSource is FileLogSource fileSource &&
                File.Exists(fileSource.LogFilePath))
            {
                File.Delete(fileSource.LogFilePath);
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Error deleting log file: {ex}", nameof(Trace));
        }
    }

    /// <summary>
    /// Menu handler: Stops log updates.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenStop_Click(object sender, RoutedEventArgs e)
    {
        _dispatcherTimer?.Stop();
        _logSource.Stop();
    }

    /// <summary>
    /// Menu handler: Starts or resumes log updates.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenStart_Click(object sender, RoutedEventArgs e)
    {
        _lastLineCount = _logSource.ReadAll().Count();
        _logSource.Start();
        _dispatcherTimer?.Start();
    }

    /// <summary>
    /// Menu handler: Loads all lines asynchronously into the UI.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private async void MenLoadA_Click(object sender, RoutedEventArgs e)
    {
        await LoadFileAsync();
    }

    /// <summary>
    /// Menu handler: Opens a log file chosen by the user and replaces the current source.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenLog_Click(object sender, RoutedEventArgs e)
    {
        var file = DialogHandler.HandleFileOpen("*.log");

        if (file == null || !File.Exists(file.FilePath))
        {
            return;
        }

        var newSource = new FileLogSource(file.FilePath);
        ReplaceLogSource(newSource);
    }

    /// <summary>
    /// Menu handler: Clears the UI log display.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenClear_Click(object sender, RoutedEventArgs e)
    {
        Log.Document.Blocks.Clear();
    }

    /// <summary>
    /// Menu handler: Opens the configuration window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenConfig_Click(object sender, RoutedEventArgs e)
    {
        var conf = new ConfigWindow();
        conf.Show();
    }

    /// <summary>
    /// Loads the current source contents asynchronously into the UI.
    /// </summary>
    private async Task LoadFileAsync()
    {
        _dispatcherTimer?.Stop();
        _logSource.Stop();

        Log.Document.Blocks.Clear();
        var lines = _logSource.ReadAll().ToList();

        await Task.Run(() =>
        {
            foreach (var line in lines)
            {
                Dispatcher.Invoke(() => AppendLine(line, false));
            }
        });

        Log.ScrollToEnd();
        _lastLineCount = lines.Count;

        _logSource.Start();
        _dispatcherTimer?.Start();
    }

    /// <summary>
    /// Menu handler: Starts the filter dialog and listens for filter changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MenFilter_Click(object sender, RoutedEventArgs e)
    {
        _filter.FilterChanged += FilterChanged;
        _filter.Start();
    }

    /// <summary>
    /// Event handler called when the filter changes.
    /// Re-applies the filter to all lines and highlights matches.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void FilterChanged(object sender, EventArgs e)
    {
        Log.Document.Blocks.Clear();

        foreach (var line in _logSource.ReadAll())
        {
            var highlight = _filter.CheckFilter(line);
            AppendLine(line, highlight);
        }

        Log.ScrollToEnd();
        _lastLineCount = _logSource.ReadAll().Count();
    }
}
