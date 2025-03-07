/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        CoreMemoryLog/LogEntry.cs
 * PURPOSE:     All possible log informations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace CoreMemoryLog
{
    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public Exception Exception { get; set; }

        public string CallerMethod { get; set; } // Stores the calling method name

        public string LibraryName { get; set; }
    }

}
