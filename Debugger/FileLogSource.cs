/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/FileLogSource.xaml.cs
 * PURPOSE:     Sample file-backed log source
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Debugger
{
    /// <summary>
    /// A simple file-backed log source that can read all lines and tail appended lines.
    /// Note: this implementation is intentionally simple; it tails by keeping an open reader
    /// and polling for new lines. If you need rotation/truncation handling we can harden it.
    /// </summary>
    public class FileLogSource : ILogSource, IDisposable
    {
        private readonly string _path;
        private CancellationTokenSource? _cts;
        private Task? _tailTask;

        /// <summary>
        /// Raised when a new line arrives.
        /// </summary>
        public event EventHandler<string>? LineReceived;

        /// <summary>
        /// Gets the file path for this source.
        /// </summary>
        public string LogFilePath => _path;

        /// <summary>
        /// Initializes a new instance of <see cref="FileLogSource"/>.
        /// </summary>
        /// <param name="path">Path to the log file.</param>
        public FileLogSource(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>
        /// Returns all current lines in the file.
        /// </summary>
        public IEnumerable<string> ReadAll()
        {
            if (!File.Exists(_path))
            {
                // ensure file exists
                using var fs = new FileStream(_path, FileMode.CreateNew);
                return Enumerable.Empty<string>();
            }

            // return snapshot
            return File.ReadAllLines(_path, Encoding.UTF8);
        }

        /// <summary>
        /// Starts tailing the file for appended lines.
        /// </summary>
        public void Start()
        {
            if (_tailTask != null && !_tailTask.IsCompleted) return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _tailTask = Task.Run(async () =>
            {
                try
                {
                    // Ensure file exists
                    if (!File.Exists(_path))
                    {
                        using var _ = new FileStream(_path, FileMode.CreateNew);
                    }

                    using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fs, Encoding.UTF8);

                    // fast-forward to end to only get new lines
                    while (!reader.EndOfStream)
                    {
                        await reader.ReadLineAsync().ConfigureAwait(false);
                    }

                    while (!token.IsCancellationRequested)
                    {
                        string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (line != null)
                        {
                            LineReceived?.Invoke(this, line);
                        }
                        else
                        {
                            // nothing new — small delay
                            await Task.Delay(200, token).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected on stop
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"FileLogSource tail error: {ex}");
                }
            }, token);
        }

        /// <summary>
        /// Stops tailing.
        /// </summary>
        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _tailTask?.Wait(500);
            }
            catch { /* ignore */ }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _tailTask = null;
            }
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
