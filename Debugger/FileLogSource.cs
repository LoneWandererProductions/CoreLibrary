/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        FileLogSource.xaml.cs
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
using System.Windows;

namespace Debugger
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// A simple file-backed log source that can read all lines and tail appended lines.
    /// Note: this implementation is intentionally simple; it tails by keeping an open reader
    /// and polling for new lines. If you need rotation/truncation handling we can harden it.
    /// </summary>
    public sealed class FileLogSource : ILogSource, IDisposable
    {
        /// <summary>
        /// The cancellation Token
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// The tail task
        /// </summary>
        private Task? _tailTask;

        /// <inheritdoc />
        /// <summary>
        /// Raised when a new line arrives.
        /// </summary>
        public event EventHandler<string>? LineReceived;

        /// <summary>
        /// Gets the file path for this source.
        /// </summary>
        /// <value>
        /// The log file path.
        /// </value>
        internal string LogFilePath { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FileLogSource"/>.
        /// </summary>
        /// <param name="path">Path to the log file.</param>
        internal FileLogSource(string path)
        {
            LogFilePath = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns all current lines in the file.
        /// </summary>
        public IEnumerable<string> ReadAll()
        {
            if (File.Exists(LogFilePath))
            {
                return File.ReadAllLines(LogFilePath, Encoding.UTF8);
            }

            // ensure file exists
            using var fs = new FileStream(LogFilePath, FileMode.CreateNew);
            return Enumerable.Empty<string>();

            // return snapshot
        }

        /// <inheritdoc />
        /// <summary>
        /// Starts tailing the file for appended lines.
        /// </summary>
        public void Start()
        {
            if (_tailTask?.IsCompleted == false) return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _tailTask = Task.Run(async () =>
            {
                try
                {
                    // Ensure file exists
                    if (!File.Exists(LogFilePath))
                    {
                        await using var _ = new FileStream(LogFilePath, FileMode.CreateNew);
                    }

                    await using var fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite);
                    using var reader = new StreamReader(fs, Encoding.UTF8);

                    // fast-forward to end to only get new lines
                    while (!reader.EndOfStream)
                    {
                        await reader.ReadLineAsync(token).ConfigureAwait(false);
                    }

                    while (!token.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(token).ConfigureAwait(false);
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

        /// <inheritdoc />
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
            catch
            {
                /* ignore */
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _tailTask = null;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
