/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreWorker
 * FILE:        CoreWorker/InMemoryLogger.cs
 * PURPOSE:     Stub for a Logger (in-memory storage for logs)
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using Debugger;

namespace CoreWorker
{
    /// <summary>
    /// A simple in-memory logger for testing/logging purposes.
    /// </summary>
    public class InMemoryLogger : ILogger
    {
        private readonly ConcurrentBag<string> _logs = new();

        /// <summary>
        /// Gets all logged messages.
        /// </summary>
        public IReadOnlyCollection<string> Logs => _logs;

        /// <summary>
        /// Logs an informational message by storing it in-memory.
        /// </summary>
        /// <param name="message">The log message.</param>
        public void LogInformation(string message)
        {
            _logs.Add(message);
        }

        /// <summary>
        /// Clears all logged messages.
        /// </summary>
        public void ClearLogs()
        {
            _logs.Clear();
        }
    }
}
