﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMemoryLog
{
    public interface ILogger
    {
        void Log(LogLevel level, string message, Exception exception = null,
            [CallerMemberName] string callerMethod = "");

        void LogInformation(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

        void LogWarning(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

        void LogError(string message, Exception exception = null, [CallerMemberName] string callerMethod = "");

        List<LogEntry> GetLog();
    }
}
