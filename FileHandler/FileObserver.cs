/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileObserver.cs
 * PURPOSE:     File Watcher, that observes changes to Folder.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileHandler
{
    /// <inheritdoc />
    /// <summary>
    /// Observes a folder for changes and raises async events safely.
    /// Supports cancellation via <see cref="T:System.Threading.CancellationToken" />.
    /// </summary>
    public sealed class FileObserver : IDisposable
    {
        /// <summary>
        /// The debounce limit
        /// </summary>
        private readonly TimeSpan _debounceLimit = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// The event times
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _eventTimes = new();

        /// <summary>
        /// The watcher
        /// </summary>
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// Occurs when [created].
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Created;

        /// <summary>
        /// Occurs when [changed].
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Changed;

        /// <summary>
        /// Occurs when [deleted].
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Deleted;

        /// <summary>
        /// Occurs when [renamed].
        /// </summary>
        public event Func<RenamedEventArgs, Task>? Renamed;

        /// <summary>
        /// Occurs when [error].
        /// </summary>
        public event Func<ErrorEventArgs, Task>? Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileObserver"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        public FileObserver(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                InternalBufferSize = 64 * 1024
            };

            // Safely route the events
            _watcher.Created += async (s, e) =>
            {
                if (!IsDebounced(e.FullPath, e.ChangeType))
                    await InvokeAsyncSafe(Created, e);
            };

            _watcher.Changed += async (s, e) =>
            {
                if (!IsDebounced(e.FullPath, e.ChangeType))
                    await InvokeAsyncSafe(Changed, e);
            };

            _watcher.Deleted += async (s, e) => await InvokeAsyncSafe(Deleted, e);

            _watcher.Renamed += async (s, e) =>
            {
                if (!IsDebounced(e.FullPath, e.ChangeType))
                    await InvokeAsyncSafe(Renamed, e);
            };

            _watcher.Error += async (s, e) => await InvokeAsyncSafe(Error, e);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start() => _watcher.EnableRaisingEvents = true;

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop() => _watcher.EnableRaisingEvents = false;

        /// <summary>
        /// Runs the observer until cancelled via token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunUntilCancelledAsync(CancellationToken cancellationToken)
        {
            Start();
            try
            {
                // This is a much safer way to wait for cancellation than TaskCompletionSource
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected behavior when cancellation is requested
            }
            finally
            {
                Stop();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
            // Suppress finalization since we've manually cleaned up
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Safely invokes async multicast delegates without crashing the app on exceptions.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="asyncEvent">The asynchronous event.</param>
        /// <param name="args">The arguments.</param>
        private async Task InvokeAsyncSafe<T>(Func<T, Task>? asyncEvent, T args)
        {
            if (asyncEvent == null) return;

            // Extract all subscribers to ensure we await all of them
            var delegates = asyncEvent.GetInvocationList().Cast<Func<T, Task>>();

            foreach (var handler in delegates)
            {
                try
                {
                    await handler(args);
                }
                catch (Exception ex)
                {
                    // In a production app, you should log this exception here!
                    // If you let it bubble up, the async void caller will crash the app.
                    Trace.WriteLine($"Error in FileObserver event handler: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if an event is a duplicate fired in rapid succession.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="changeType">Type of the change.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path is debounced; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDebounced(string filePath, WatcherChangeTypes changeType)
        {
            // Create a unique key for the file and the specific action
            var key = $"{filePath}|{changeType}";
            var now = DateTime.UtcNow;

            if (_eventTimes.TryGetValue(key, out var lastEventTime))
            {
                // If the time since the last event is less than our limit, it's spam. Ignore it.
                if (now - lastEventTime < _debounceLimit)
                {
                    // Update the time to create a "sliding window" that waits for the spam to stop
                    _eventTimes[key] = now;
                    return true; // Yes, debounce this (ignore)
                }
            }

            // It's a fresh event. Record the time and let it through.
            _eventTimes[key] = now;
            return false; // No, do not debounce (process)
        }
    }
}
