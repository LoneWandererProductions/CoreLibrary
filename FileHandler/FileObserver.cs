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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileHandler
{
    /// <inheritdoc />
    /// <summary>
    /// Observes a folder for changes and raises async events.
    /// Supports cancellation via <see cref="T:System.Threading.CancellationToken" />.
    /// </summary>
    public class FileObserver : IDisposable
    {
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// Triggered when a file or folder is created.
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Created;

        /// <summary>
        /// Triggered when a file or folder is changed.
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Changed;

        /// <summary>
        /// Triggered when a file or folder is deleted.
        /// </summary>
        public event Func<FileSystemEventArgs, Task>? Deleted;

        /// <summary>
        /// Triggered when a file or folder is renamed.
        /// </summary>
        public event Func<RenamedEventArgs, Task>? Renamed;

        /// <summary>
        /// Triggered on watcher errors.
        /// </summary>
        public event Func<ErrorEventArgs, Task>? Error;

        /// <summary>
        /// Initializes a new instance of <see cref="FileObserver"/>.
        /// </summary>
        /// <param name="path">Folder to observe.</param>
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

            _watcher.Created += async (s, e) =>
            {
                if (Created != null) await Created.Invoke(e);
            };
            _watcher.Changed += async (s, e) =>
            {
                if (Changed != null) await Changed.Invoke(e);
            };
            _watcher.Deleted += async (s, e) =>
            {
                if (Deleted != null) await Deleted.Invoke(e);
            };
            _watcher.Renamed += async (s, e) =>
            {
                if (Renamed != null) await Renamed.Invoke(e);
            };
            _watcher.Error += async (s, e) =>
            {
                if (Error != null) await Error.Invoke(e);
            };
        }

        /// <summary>
        /// Starts watching the folder.
        /// </summary>
        public void Start() => _watcher.EnableRaisingEvents = true;

        /// <summary>
        /// Stops watching the folder.
        /// </summary>
        public void Stop() => _watcher.EnableRaisingEvents = false;

        /// <summary>
        /// Runs the observer until cancelled via token.
        /// </summary>
        /// <param name="cancellationToken">Token to stop watching.</param>
        /// <returns>Task that completes when cancelled.</returns>
        public Task RunUntilCancelledAsync(CancellationToken cancellationToken)
        {
            Start();
            var tcs = new TaskCompletionSource<object?>();
            cancellationToken.Register(() =>
            {
                Stop();
                tcs.TrySetResult(null);
            });
            return tcs.Task;
        }

        /// <summary>
        /// Disposes the internal watcher.
        /// </summary>
        public void Dispose() => _watcher.Dispose();
    }
}
