/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        FullLogger.cs
 * PURPOSE:     My In Memory Logger
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreMemoryLog;

namespace Debugger
{
    /// <summary>
    ///     The Full Logger Class
    /// </summary>
    public class FullLogger
    {
        /// <summary>
        ///     The in memory logger
        /// </summary>
        private readonly IInMemoryLogger _inMemoryLogger;

        /// <summary>
        ///     The polling interval
        /// </summary>
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5); // Adjust as needed

        /// <summary>
        ///     Initializes a new instance of the <see cref="FullLogger" /> class.
        /// </summary>
        /// <param name="inMemoryLogger">The in memory logger.</param>
        public FullLogger(IInMemoryLogger inMemoryLogger)
        {
            _inMemoryLogger = inMemoryLogger;
        }

        /// <summary>
        ///     Starts the polling asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartPollingAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var logs = _inMemoryLogger.GetLogs();
                if (logs.Any())
                {
                    foreach (var log in logs)
                    {
                        // Here, you can log to your full logger (e.g., to a file, DB, etc.)
                        Trace.WriteLine($"[{log.Timestamp}] {log.Level}: {log.Message}");
                    }
                }

                await Task.Delay(_pollingInterval, cancellationToken);
            }
        }
    }
}
