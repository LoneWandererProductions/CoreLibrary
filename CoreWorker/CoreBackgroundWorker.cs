/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreWorker
 * FILE:        CoreWorker/CoreBackgroundWorker.cs
 * PURPOSE:     Actual Implementation of a BackgroundWorker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Threading;
using System.Threading.Tasks;
using Debugger;

namespace CoreWorker
{
    public sealed class CoreBackgroundWorker : ICoreBackgroundWorker
    {
        private readonly ILogger _logger;
        private Task? _task;
        private CancellationTokenSource _cts = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBackgroundWorker"/> class.
        /// Parameterless constructor, providing a default logger
        /// </summary>
        public CoreBackgroundWorker()
        {
            _logger = default; // to be implemented 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBackgroundWorker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CoreBackgroundWorker(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _logger?.LogInformation("Background worker started.");

            _cts = new CancellationTokenSource();
            _task = Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        _logger?.LogInformation("Worker is running...");
                        await Task.Delay(5000, _cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogInformation($"Worker encountered an error: {ex.Message}");
                }
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _logger?.LogInformation("Stopping worker...");
            _cts.Cancel();
            _task?.Wait(); // Ensure task is completed
        }
    }
}
