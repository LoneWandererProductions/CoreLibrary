using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreMemoryLog;

namespace Debugger
{
    public class FullLogger
    {
        private readonly IInMemoryLogger _inMemoryLogger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5); // Adjust as needed

        public FullLogger(IInMemoryLogger inMemoryLogger)
        {
            _inMemoryLogger = inMemoryLogger;
        }

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
